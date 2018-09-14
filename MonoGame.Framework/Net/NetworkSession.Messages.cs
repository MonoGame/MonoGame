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
            msg.Write((byte)(recipientMachine?.Id ?? 255));
            msg.Write((byte)localMachine.Id);
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

            Debug.WriteLine($"S [self] ({(originMachine == localMachine ? "[self]" : originMachine.Id.ToString())})->{recipientMachine?.Id.ToString() ?? "[all]"} {msgType}");

            if (!sendToAll && recipientMachine.IsLocal)
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
                Debug.Write($"Received empty message from machine {senderMachine.Id.ToString() ?? "[self]"}");
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
                Debug.WriteLine($"Received message with malformed header from machine {senderMachine.Id.ToString() ?? "[self]"}");
                return;
            }
            if (headerMsgType >= MessageTypeCount)
            {
                // TODO: Kick machine?
                Debug.WriteLine($"Received message with malformed header from machine {senderMachine.Id.ToString() ?? "[self]"}");
                return;
            }

            MessageType msgType = (MessageType)headerMsgType;
            bool sendToAll = headerRecipientId == 255;

            if ((!sendToAll && !machineFromId.ContainsKey(headerRecipientId)) || !machineFromId.ContainsKey(headerOriginId))
            {
                if (isHost)
                {
                    // TODO: Kick machine?
                    Debug.WriteLine($"Received message with malformed header from machine {senderMachine.Id.ToString() ?? "[self]"}");
                }
                return;
            }

            var recipientMachine = sendToAll ? null : machineFromId[headerRecipientId];
            var originMachine = machineFromId[headerOriginId];

            if (isHost && senderMachine != originMachine)
            {
                // TODO: Kick machine?
                Debug.WriteLine($"Received message with malformed header from machine {senderMachine.Id.ToString() ?? "[self]"}");
                return;
            }

            Debug.WriteLine($"R {(senderMachine == localMachine ? "[self]" : senderMachine.Id.ToString())} ({originMachine.Id.ToString() ?? "[self]"})->{recipientMachine?.Id.ToString() ?? "[all]"} {msgType}");

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
                        success = ReceiveGamerJoined(msg, originMachine);
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
                Debug.WriteLine($"Forwarding {msgType} message to machine {recipientMachine?.Id.ToString() ?? "[all]"}");

                SendMessage(CreateMessageFrom(msg), deliveryMethod, ignoreSelf: true, ignoreMachine: senderMachine);
            }
        }

        private void SendMachineConnectedMessage(NetworkMachine machine, NetworkMachine recipient)
        {
            if (!isHost) throw new InvalidOperationException();

            var msg = CreateMessage(MessageType.MachineConnected, recipient);
            msg.Write(machine.Id);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveMachineConnectedMessage(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.IsHost)
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
            if (id == localMachine.Id)
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
            msg.Write(machine.Id);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveMachineDisconnectedMessage(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.IsHost)
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
            if (id == localMachine.Id)
            {
                // Some race condition occured, we should already be disconnected from host
                // TODO: Suitable place to end session as we were probably kicked?
                return true;
            }

            if (!isHost)
            {
                // Host already removed machine
                RemoveMachine(machineFromId[id]);
            }
            return true;
        }

        private void SendGamerIdRequest()
        {
            if (isHost) throw new InvalidOperationException();

            var msg = CreateMessage(MessageType.GamerIdRequest, hostMachine);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerIdRequest(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!isHost)
            {
                return false;
            }

            bool available = GetNewUniqueId(gamerFromId, out byte id);
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
            if (isHost || !originMachine.IsHost)
            {
                return false;
            }
            bool success;
            byte id;
            try
            {
                success = msg.ReadBoolean();
                id = msg.ReadByte();
            }
            catch
            {
                return false;
            }
            if (success && id == 255)
            {
                return false;
            }
            if (pendingSignedInGamers.Count == 0)
            {
                return true; // Don't treat as error
            }

            var localGamer = new LocalNetworkGamer(pendingSignedInGamers[0], localMachine, id, false);
            pendingSignedInGamers.RemoveAt(0);
            AddGamer(localGamer);
            SendGamerJoined(localGamer, null);
            return true;
        }

        private void SendGamerJoined(LocalNetworkGamer localGamer, NetworkMachine recipient)
        {
            var msg = CreateMessage(MessageType.GamerJoined, recipient);
            msg.Write(localGamer.Id);
            msg.Write(localGamer.DisplayName);
            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.IsPrivateSlot);
            msg.Write(localGamer.IsReady);
            SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveGamerJoined(NetBuffer msg, NetworkMachine originMachine)
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
            if (isHost && gamerFromId.ContainsKey(id))
            {
                // TODO: Make sure client is not spamming recipient
                return gamerFromId[id].Machine == originMachine;
            }

            if (originMachine.IsLocal)
            {
                // Already added local gamer
                return true;
            }

            if (id == 0)
            {
                if (originMachine.IsHost)
                {
                    // Already added host gamer with id 0, just update it
                    Host.DisplayName = displayName;
                    Host.Gamertag = gamertag;
                    Host.SetReadyState(isReady);
                    return true;
                }
                return false;
            }

            AddGamer(new NetworkGamer(originMachine, id, isPrivateSlot, displayName, gamertag, isReady));
            return true;
        }

        private void SendGamerLeft(LocalNetworkGamer localGamer)
        {
            var msg = CreateMessage(MessageType.GamerLeft, null);
            msg.Write(localGamer.Id);
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
            if (gamer.Machine != originMachine)
            {
                return false;
            }

            RemoveGamer(gamer);
            return true;
        }

        internal void SendGamerStateChanged(LocalNetworkGamer localGamer)
        {
            var msg = CreateMessage(MessageType.GamerStateChanged, null);
            msg.Write(localGamer.Id);
            msg.Write(localGamer.DisplayName);
            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.IsReady);
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
            if (gamer.Machine != originMachine)
            {
                return false;
            }

            gamer.DisplayName = displayName;
            gamer.Gamertag = gamertag;
            gamer.SetReadyState(isReady);
            return true;
        }

        private void SendResetReady()
        {
            if (!isHost) throw new InvalidOperationException();

            SendMessage(CreateMessage(MessageType.ResetReady, null), NetDeliveryMethod.ReliableOrdered);
        }

        private bool ReceiveResetReady(NetBuffer msg, NetworkMachine originMachine)
        {
            if (!originMachine.IsHost)
            {
                return false;
            }

            foreach (var gamer in allGamers)
            {
                gamer.SetReadyState(false);
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
            if (!originMachine.IsHost)
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
            if (!originMachine.IsHost)
            {
                return false;
            }

            foreach (var gamer in allGamers)
            {
                gamer.SetReadyState(false);
            }
            state = NetworkSessionState.Lobby;
            InvokeGameEndedEvent(new GameEndedEventArgs());
            return true;
        }

        internal void SendUserMessage(LocalNetworkGamer sender, SendDataOptions options, Packet packet, NetworkGamer recipient = null)
        {
            var msg = CreateMessage(MessageType.User, recipient?.Machine);
            msg.Write(sender.Id);
            msg.Write((byte)(recipient == null ? 255 : recipient.Id));
            msg.Write((byte)options);
            msg.Write(packet.length);
            msg.Write(packet.data);
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
            if (sender != null && sender.Machine != originMachine)
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
