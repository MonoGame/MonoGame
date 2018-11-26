using System;
using System.Diagnostics;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public sealed partial class NetworkSession : IDisposable
    {
        private static NetDeliveryMethod ToDeliveryMethod(SendDataOptions options)
        {
            switch (options)
            {
                case SendDataOptions.InOrder:
                    return NetDeliveryMethod.UnreliableSequenced;
                case SendDataOptions.Reliable:
                    return NetDeliveryMethod.ReliableUnordered;
                case SendDataOptions.ReliableInOrder:
                    return NetDeliveryMethod.ReliableOrdered;
                case SendDataOptions.Chat:
                    return NetDeliveryMethod.ReliableUnordered;
                case SendDataOptions.Chat | SendDataOptions.InOrder:
                    return NetDeliveryMethod.ReliableOrdered;
                default:
                    throw new InvalidOperationException("Could not convert SendDataOptions!");
            }
        }

        private static bool IsSendDataOptionsValid(SendDataOptions options)
        {
            switch (options)
            {
                case SendDataOptions.InOrder:
                case SendDataOptions.Reliable:
                case SendDataOptions.ReliableInOrder:
                case SendDataOptions.Chat:
                case SendDataOptions.Chat | SendDataOptions.InOrder:
                    return true;
                default:
                    return false;
            }
        }

        private enum MessageType
        {
            MachineConnected,
            MachineDisconnected,
            GamerIdRequest,
            GamerIdResponse,
            GamerJoined,
            GamerLeft,
            GamerStateChanged,
            ResetReady,
            StartGame,
            EndGame,
            User,
        }

        private const int MessageTypeCount = 11;

        private NetOutgoingMessage CreateMessage(MessageType type, NetworkMachine recipientMachine)
        {
            var msg = peer.CreateMessage();
            msg.Write((byte)type);
            msg.Write((byte)(recipientMachine != null ? recipientMachine.id : 255));
            msg.Write((byte)localMachine.id);
            return msg;
        }

        private NetOutgoingMessage CreateMessageFrom(NetBuffer buffer)
        {
            var msg = peer.CreateMessage();
            msg.Write(buffer);
            return msg;
        }

        private void SendMessage(NetOutgoingMessage msg, NetDeliveryMethod deliveryMethod, bool ignoreSelf = false, NetworkMachine ignoreMachine = null)
        {
            if (msg.LengthBytes < 3)
            {
                throw new InvalidOperationException();
            }
            var msgType = (MessageType)msg.Data[0];
            var recipientId = msg.Data[1];
            var originMachine = machineFromId[msg.Data[2]];

            bool sendToAll = recipientId == 255;
            var recipientMachine = sendToAll ? null : machineFromId[recipientId];

            if (msgType != MessageType.User)
            {
                Debug.WriteLine("S " + localMachine.id + " (" + originMachine.id + ")->" + (recipientMachine != null ? recipientMachine.id.ToString() : "[all]") + " " + msgType);
            }

            if (!sendToAll && recipientMachine.isLocal)
            {
                // Recipient is local machine
                if (!ignoreSelf)
                {
                    msg.Position = 0;
                    ReceiveMessage(msg, deliveryMethod, localMachine);
                    // TODO: Fix garbage leak here
                }
                return;
            }

            if (!isHost)
            {
                // Host relays all client messages
                peer.SendMessage(msg, connectionFromMachine[hostMachine], deliveryMethod, 0);
                return;
            }

            if (sendToAll)
            {
                if (!ignoreSelf)
                {
                    msg.Position = 0;
                    ReceiveMessage(msg, deliveryMethod, localMachine);
                    // TODO: Fix garbage leak here
                }

                foreach (var machine in allMachines)
                {
                    if (machine == localMachine || machine == ignoreMachine)
                    {
                        continue;
                    }
                    var copy = peer.CreateMessage(msg.LengthBytes);
                    copy.Write(msg);
                    peer.SendMessage(copy, connectionFromMachine[machine], deliveryMethod, 0);
                }
            }
            else
            {
                peer.SendMessage(msg, connectionFromMachine[recipientMachine], deliveryMethod, 0);
            }
        }

        private void ReceiveMessage(NetBuffer msg, NetDeliveryMethod deliveryMethod, NetworkMachine senderMachine)
        {
            // Decode header
            if (msg.LengthBytes < 3)
            {
                // TODO: Kick machine?
                Debug.Write("Received empty message from machine " + senderMachine.id);
                return;
            }

            //ushort header0, header1;
            byte headerMsgType, headerRecipientId, headerOriginId;
            try
            {
                headerMsgType = msg.ReadByte();
                headerRecipientId = msg.ReadByte();
                headerOriginId = msg.ReadByte();
            }
            catch
            {
                // TODO: Kick machine?
                Debug.WriteLine("Received message with malformed header from machine " + senderMachine.id);
                return;
            }
            if (headerMsgType >= MessageTypeCount)
            {
                // TODO: Kick machine?
                Debug.WriteLine("Received message with malformed header from machine " + senderMachine.id);
                return;
            }

            MessageType msgType = (MessageType)headerMsgType;
            bool sendToAll = headerRecipientId == 255;

            if ((!sendToAll && !machineFromId.ContainsKey(headerRecipientId)) || !machineFromId.ContainsKey(headerOriginId))
            {
                if (isHost)
                {
                    // TODO: Kick machine?
                    Debug.WriteLine("Received message with malformed header from machine " + senderMachine.id);
                }
                return;
            }

            var recipientMachine = sendToAll ? null : machineFromId[headerRecipientId];
            var originMachine = machineFromId[headerOriginId];

            if (isHost && senderMachine != originMachine)
            {
                // TODO: Kick machine?
                Debug.WriteLine("Received message with malformed header from machine " + senderMachine.id);
                return;
            }

            if (msgType != MessageType.User)
            {
                Debug.WriteLine("R " + senderMachine.id + " (" + originMachine.id + ")->" + (recipientMachine != null ? recipientMachine.id.ToString() : "[all]") + " " + msgType);
            }

            // Handle message
            bool success = false;

            //try
            {
                switch (msgType)
                {
                    case MessageType.MachineConnected:
                        success = ReceiveMachineConnectedMessage(msg, originMachine);
                        break;
                    case MessageType.MachineDisconnected:
                        success = ReceiveMachineDisconnectedMessage(msg, originMachine);
                        break;
                    case MessageType.GamerIdRequest:
                        success = ReceiveGamerIdRequest(msg, originMachine);
                        break;
                    case MessageType.GamerIdResponse:
                        success = ReceiveGamerIdResponse(msg, originMachine);
                        break;
                    case MessageType.GamerJoined:
                        success = ReceiveGamerJoined(msg, originMachine, recipientMachine);
                        break;
                    case MessageType.GamerLeft:
                        success = ReceiveGamerLeft(msg, originMachine);
                        break;
                    case MessageType.GamerStateChanged:
                        success = ReceiveGamerStateChanged(msg, originMachine);
                        break;
                    case MessageType.ResetReady:
                        success = ReceiveResetReady(msg, originMachine);
                        break;
                    case MessageType.StartGame:
                        success = ReceiveStartGame(msg, originMachine);
                        break;
                    case MessageType.EndGame:
                        success = ReceiveEndGame(msg, originMachine);
                        break;
                    case MessageType.User:
                        success = ReceiveUserMessage(msg, originMachine);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            //catch
            { }

            if (!success)
            {
                // TODO: Kick machine if host and disconnect if client?
                Debug.WriteLine("Failed to parse last message!");
                return;
            }

            // If host, forward message to peers
            if (isHost && senderMachine != localMachine && recipientMachine != localMachine)
            {
                if (msgType != MessageType.User)
                {
                    Debug.WriteLine("Forwarding " + msgType + " message to machine " + (recipientMachine != null ? recipientMachine.id.ToString() : "[all]"));
                }

                SendMessage(CreateMessageFrom(msg), deliveryMethod, ignoreSelf: true, ignoreMachine: senderMachine);
            }
        }

        private void SendMachineConnectedMessage(NetworkMachine machine, NetworkMachine recipient)
        {
            if (!isHost) throw new InvalidOperationException();

            var msg = CreateMessage(MessageType.MachineConnected, recipient);
            msg.Write(machine.id);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveMachineConnectedMessage(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.isHost)
            {
                return false;
            }
            byte id;
            try
            {
                id = msg.ReadByte();
            }
            catch
            {
                return false;
            }
            if (id == localMachine.id)
            {
                // The host is broadcasting our local machine to everyone
                return true;
            }

            NetworkMachine newMachine;
            if (isHost)
            {
                // Host has already added machine
                newMachine = machineFromId[id];

                // Tell new machine about the machines already in the session
                foreach (var existingMachine in allMachines)
                {
                    if (existingMachine == localMachine || existingMachine == newMachine)
                    {
                        continue;
                    }
                    SendMachineConnectedMessage(existingMachine, newMachine);
                }
            }
            else
            {
                newMachine = new NetworkMachine(this, false, false, id);
                AddMachine(newMachine, hostConnection);
            }

            // Tell new machine about our gamers
            foreach (var localGamer in localGamers)
            {
                SendGamerJoined(localGamer, newMachine);
            }

            return true;
        }

        private void SendMachineDisconnectedMessage(NetworkMachine machine)
        {
            if (!isHost) throw new InvalidOperationException();

            var msg = CreateMessage(MessageType.MachineDisconnected, null);
            msg.Write(machine.id);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveMachineDisconnectedMessage(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.isHost)
            {
                return false;
            }
            byte id;
            try
            {
                id = msg.ReadByte();
            }
            catch
            {
                return false;
            }
            if (id == localMachine.id)
            {
                // Some race condition occured, we should already be disconnected from host
                // TODO: Suitable place to end session as we were probably kicked?
                return true;
            }


            if (isHost || !machineFromId.ContainsKey(id))
            {
                // Host already removed machine and machine might have been disconnected before it fully connected
                return true;
            }
            RemoveMachine(machineFromId[id]);
            return true;
        }

        private void SendGamerIdRequest()
        {
            var msg = CreateMessage(MessageType.GamerIdRequest, hostMachine);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerIdRequest(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!isHost)
            {
                return false;
            }
            if (originMachine.gamers.Count >= MaxSupportedLocalGamers)
            {
                // A single requester should not request too many gamer ids
                return false;
            }

            bool available = false;
            byte id = 255;
            if (GetOpenSlotsForMachine(originMachine) > 0)
            {
                available = GetUniqueId(gamerFromId, out id);

                // Let Host create remote gamers directly
                if (available && isHost && originMachine != localMachine)
                {
                    var isPrivateSlot = originMachine.isHost && GetOpenPrivateGamerSlots() > 0;
                    AddGamer(new NetworkGamer(originMachine, id, isPrivateSlot, false, LoadingGamertag, LoadingGamertag));
                }
            }
            SendGamerIdResponse(originMachine, available, id);
            return true;
        }

        private void SendGamerIdResponse(NetworkMachine recipient, bool success, byte id)
        {
            if (!isHost || (success && id == 255)) throw new InvalidOperationException();

            var msg = CreateMessage(MessageType.GamerIdResponse, recipient);
            msg.Write(success);
            msg.Write(id);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerIdResponse(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.isHost)
            {
                return false;
            }
            bool slotAvailable;
            byte id;
            try
            {
                slotAvailable = msg.ReadBoolean();
                id = msg.ReadByte();
            }
            catch
            {
                return false;
            }
            if (slotAvailable && id == 255)
            {
                return false;
            }

            if (pendingSignedInGamers.Count == 0)
            {
                // Don't treat as error but host will have marked slot used by client
                return true;
            }
            if (!slotAvailable)
            {
                pendingSignedInGamers.RemoveAt(0);
                return true;
            }
            var signedInGamer = pendingSignedInGamers[0];
            pendingSignedInGamers.RemoveAt(0);

            var isPrivateSlot = isHost && GetOpenPrivateGamerSlots() > 0;
            var localGamer = new LocalNetworkGamer(signedInGamer, localMachine, id, isPrivateSlot);
            AddGamer(localGamer);
            SendGamerJoined(localGamer, null);
            return true;
        }

        private void SendGamerJoined(LocalNetworkGamer localGamer, NetworkMachine recipient)
        {
            var msg = CreateMessage(MessageType.GamerJoined, recipient);
            msg.Write(localGamer.id);
            msg.Write(localGamer.DisplayName);
            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.isPrivateSlot);
            msg.Write(localGamer.isReady);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerJoined(NetBuffer msg, NetworkMachine originMachine, NetworkMachine recipientMachine)
        {
            byte id;
            string displayName, gamertag;
            bool isPrivateSlot, isReady;
            try
            {
                id = msg.ReadByte();
                displayName = msg.ReadString();
                gamertag = msg.ReadString();
                isPrivateSlot = msg.ReadBoolean();
                isReady = msg.ReadBoolean();
            }
            catch
            {
                return false;
            }
            if (id == 255)
            {
                return false;
            }

            if (originMachine.isLocal)
            {
                // Already added local gamer
                return true;
            }

            if (isHost)
            {
                if (!gamerFromId.ContainsKey(id))
                {
                    // Host must know about all gamers
                    return false;
                }
                if (id == 0)
                {
                    // Someone is impersonating the host gamer
                    return false;
                }
                if (recipientMachine != null && recipientMachine != localMachine)
                {
                    // TODO: Make sure client is not spamming recipient
                }

                // Host already added gamer, just update it
                var gamer = gamerFromId[id];
                gamer.DisplayName = displayName;
                gamer.Gamertag = gamertag;
                gamer.isPrivateSlot = isPrivateSlot;
                gamer.isReady = isReady;
            }
            else
            {
                if (id == 0)
                {
                    // Special case for host gamer
                    if (!originMachine.isHost)
                    {
                        return false;
                    }

                    // Already added host gamer with id 0, just update it
                    Host.DisplayName = displayName;
                    Host.Gamertag = gamertag;
                    Host.isPrivateSlot = isPrivateSlot;
                    Host.isReady = isReady;
                }
                else
                {
                    AddGamer(new NetworkGamer(originMachine, id, isPrivateSlot, isReady, displayName, gamertag));
                }
            }
            return true;
        }

        private void SendGamerLeft(LocalNetworkGamer localGamer)
        {
            var msg = CreateMessage(MessageType.GamerLeft, null);
            msg.Write(localGamer.id);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerLeft(NetBuffer msg, NetworkMachine originMachine)
        {
            byte id;
            try
            {
                id = msg.ReadByte();
            }
            catch
            {
                return false;
            }
            if (id == 255)
            {
                return false;
            }
            if (!gamerFromId.ContainsKey(id))
            {
                return false;
            }
            var gamer = gamerFromId[id];
            if (gamer.machine != originMachine)
            {
                return false;
            }

            RemoveGamer(gamer);
            return true;
        }

        internal void SendGamerStateChanged(LocalNetworkGamer localGamer)
        {
            var msg = CreateMessage(MessageType.GamerStateChanged, null);
            msg.Write(localGamer.id);
            msg.Write(localGamer.DisplayName);
            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.isReady);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerStateChanged(NetBuffer msg, NetworkMachine originMachine)
        {
            byte id;
            string displayName, gamertag;
            bool isReady;
            try
            {
                id = msg.ReadByte();
                displayName = msg.ReadString();
                gamertag = msg.ReadString();
                isReady = msg.ReadBoolean();
            }
            catch
            {
                return false;
            }
            if (id == 255)
            {
                return false;
            }
            if (!gamerFromId.ContainsKey(id))
            {
                return false;
            }
            var gamer = gamerFromId[id];
            if (gamer.machine != originMachine)
            {
                return false;
            }

            gamer.DisplayName = displayName;
            gamer.Gamertag = gamertag;
            gamer.isReady = isReady;
            return true;
        }

        private void SendResetReady()
        {
            if (!isHost) throw new InvalidOperationException();

            SendMessage(CreateMessage(MessageType.ResetReady, null), NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveResetReady(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.isHost)
            {
                return false;
            }

            foreach (var gamer in allGamers)
            {
                gamer.isReady = false;
            }
            return true;
        }

        private void SendStartGame()
        {
            if (!isHost) throw new InvalidOperationException();

            SendMessage(CreateMessage(MessageType.StartGame, null), NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveStartGame(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.isHost)
            {
                return false;
            }

            state = NetworkSessionState.Playing;
            InvokeGameStartedEvent(new GameStartedEventArgs());
            return true;
        }

        private void SendEndGame()
        {
            if (!isHost) throw new InvalidOperationException();

            SendMessage(CreateMessage(MessageType.EndGame, null), NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveEndGame(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.isHost)
            {
                return false;
            }

            foreach (var gamer in allGamers)
            {
                gamer.isReady = false;
            }
            state = NetworkSessionState.Lobby;
            InvokeGameEndedEvent(new GameEndedEventArgs());
            return true;
        }

        internal void SendUserMessage(LocalNetworkGamer sender, SendDataOptions options, byte[] data, NetworkGamer recipient = null)
        {
            var msg = CreateMessage(MessageType.User, recipient != null ? recipient.machine : null);
            msg.Write(sender.id);
            msg.Write((byte)(recipient == null ? 255 : recipient.id));
            msg.Write((byte)options);
            msg.Write(data.Length);
            msg.Write(data);
            SendMessage(msg, ToDeliveryMethod(options));
        }

        internal void SendUserMessage(LocalNetworkGamer sender, SendDataOptions options, PacketWriter data, NetworkGamer recipient = null)
        {
            var msg = CreateMessage(MessageType.User, recipient != null ? recipient.machine : null);
            msg.Write(sender.id);
            msg.Write((byte)(recipient == null ? 255 : recipient.id));
            msg.Write((byte)options);

            msg.Write(data.Length);
            data.BaseStream.Position = 0;
            int dataByte;
            while ((dataByte = data.BaseStream.ReadByte()) != -1)
            {
                msg.Write((byte)dataByte);
            }

            SendMessage(msg, ToDeliveryMethod(options));
        }

        private bool ReceiveUserMessage(NetBuffer msg, NetworkMachine originMachine)
        {
            byte senderId, recipientId;
            SendDataOptions options;
            int length;
            Packet packet;
            try
            {
                senderId = msg.ReadByte();
                recipientId = msg.ReadByte();
                options = (SendDataOptions)msg.ReadByte();
                length = msg.ReadInt32();
                packet = packetPool.Get(length); // Critical TODO: Protect memory
                msg.ReadBytes(packet.data, 0, length);
            }
            catch
            {
                return false;
            }
            bool sendToAll = recipientId == 255;
            if (senderId == 255)
            {
                return false;
            }
            var sender = gamerFromId.ContainsKey(senderId) ? gamerFromId[senderId] : null; // Sender can be null if gamer joined not yet received
            if (sender != null && sender.machine != originMachine)
            {
                return false;
            }
            if (!IsSendDataOptionsValid(options))
            {
                return false;
            }

            if (sendToAll)
            {
                bool firstGamer = true;
                foreach (var localGamer in localGamers)
                {
                    var uniquePacket = firstGamer ? packet : packetPool.GetAndFillWith(packet.data);
                    if (!localGamer.AddInboundPacket(uniquePacket, senderId, options))
                    {
                        // TODO: Kick machine if host?
                        Debug.WriteLine("MaxDelayedInboundPacketsAllowed reached!");
                        return false;
                    }
                    firstGamer = false;
                }
                return true;
            }
            else
            {
                var recipient = FindGamerById(recipientId);
                if (recipient == null)
                {
                    // TODO: Check if recipient is on list of previous gamers, for now assume true
                    bool previousGamer = true;

                    // Message is ok if the recipient was a gamer previously
                    return previousGamer;
                }

                var localGamer = (LocalNetworkGamer)recipient;
                if (localGamer == null)
                {
                    if (isHost)
                    {
                        // The message is meant for someone else (return true so that the message is forwarded)
                        return true;
                    }
                    else
                    {
                        // The message is meant for us but the local gamer is gone? The host made a mistake or our local gamer left, see above
                        bool previousLocalGamer = true;
                        return previousLocalGamer;
                    }
                }

                if (!localGamer.AddInboundPacket(packet, senderId, options))
                {
                    // TODO: Kick machine if host?
                    Debug.WriteLine("MaxDelayedInboundPacketsAllowed reached!");
                    return false;
                }
                return true;
            }
        }
    }
}
