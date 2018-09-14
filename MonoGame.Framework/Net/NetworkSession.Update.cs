using System;
using System.Diagnostics;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public sealed partial class NetworkSession : IDisposable
    {
        internal static void HandleLidgrenMessage(NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                    Debug.WriteLine($"Lidgren: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.WarningMessage:
                    Debug.WriteLine($"Lidgren Warning: {msg.ReadString()}");
                    break;
                case NetIncomingMessageType.ErrorMessage:
                    Debug.WriteLine($"Lidgren Error: {msg.ReadString()}");
                    break;
                default:
                    Debug.WriteLine($"Unhandled message type: {msg.MessageType}");
                    break;
            }
        }

        private void ReceiveMessages()
        {
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.DiscoveryRequest)
                {
                    Debug.WriteLine("Local discovery request received");

                    if (isHost)
                    {
                        UpdatePublicInfo();

                        var response = peer.CreateMessage();
                        NetworkSessionMasterServer.SerializeRequestHostsResponse(response, guid, publicInfo);
                        peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                {
                    if (!isHost)
                    {
                        throw new InvalidOperationException();
                    }

                    if (allowJoinInProgress || state == NetworkSessionState.Lobby)
                    {
                        if (GetOpenPublicGamerSlots() > 0 && GetNewUniqueId(machineFromId, out byte machineId))
                        {
                            // Approved, create network machine
                            msg.SenderConnection.Tag = new NetworkMachine(this, false, false, machineId);

                            // Send approval to client containing unique machine id
                            var hailMsg = peer.CreateMessage();
                            hailMsg.Write(machineId);
                            msg.SenderConnection.Approve(hailMsg);
                        }
                        else
                        {
                            msg.SenderConnection.Deny(NetworkSessionJoinError.SessionFull.ToString());
                        }
                    }
                    else
                    {
                        msg.SenderConnection.Deny(NetworkSessionJoinError.SessionNotJoinable.ToString());
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    var status = (NetConnectionStatus)msg.ReadByte();
                    Debug.WriteLine($"Connection status updated: {status} (Reason: {msg.ReadString()})");

                    if (status == NetConnectionStatus.Connected)
                    {
                        if (!isHost)
                        {
                            throw new InvalidOperationException("A client cannot accept new connections");
                        }
                        if (msg.SenderConnection.Tag == null)
                        {
                            throw new InvalidOperationException();
                        }

                        var machine = (NetworkMachine)msg.SenderConnection.Tag;
                        AddMachine(machine, msg.SenderConnection);

                        SendMachineConnectedMessage(machine, null);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        if (msg.SenderConnection != null)
                        {
                            if (msg.SenderConnection.Tag == null)
                            {
                                throw new InvalidOperationException();
                            }

                            var machine = (NetworkMachine)msg.SenderConnection.Tag;
                            RemoveMachine(machine);
                            
                            if (isHost)
                            {
                                SendMachineDisconnectedMessage(machine);
                            }
                            else
                            {
                                if (msg.ReadString(out string reasonString) && Enum.TryParse(reasonString, out NetworkSessionEndReason reason))
                                {
                                    End(reason);
                                }
                                else
                                {
                                    End(NetworkSessionEndReason.Disconnected);
                                }
                            }
                        }
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    if (msg.SenderConnection.Tag == null)
                    {
                        throw new InvalidOperationException();
                    }
                    ReceiveMessage(msg, msg.DeliveryMethod, (NetworkMachine)msg.SenderConnection.Tag);
                }
                else
                {
                    HandleLidgrenMessage(msg);
                }
                peer.Recycle(msg);

                if (IsDisposed)
                {
                    break;
                }
            }
        }

        private int GetOpenPrivateGamerSlots()
        {
            int usedSlots = 0;
            foreach (var gamer in allGamers)
            {
                if (gamer.IsPrivateSlot)
                {
                    usedSlots++;
                }
            }
            return PrivateGamerSlots - usedSlots;
        }

        private int GetOpenPublicGamerSlots()
        {
            return maxGamers - PrivateGamerSlots - allGamers.Count;
        }

        private void UpdatePublicInfo()
        {
            publicInfo.Set(type,
                            properties,
                            Host?.Gamertag ?? "Game starting...",
                            maxGamers,
                            PrivateGamerSlots,
                            allGamers.Count,
                            GetOpenPrivateGamerSlots(),
                            GetOpenPublicGamerSlots());
        }

        private void RegisterWithMasterServer()
        {
            if (type != NetworkSessionType.PlayerMatch &&
                type != NetworkSessionType.Ranked)
            {
                return;
            }

            UpdatePublicInfo();
            
            NetworkSessionMasterServer.RegisterHost(peer, guid, GetInternalIp(peer), publicInfo);
        }

        private void UnregisterWithMasterServer()
        {
            if (type != NetworkSessionType.PlayerMatch &&
                type != NetworkSessionType.Ranked)
            {
                return;
            }

            NetworkSessionMasterServer.UnregisterHost(peer, guid);
        }
    }
}
