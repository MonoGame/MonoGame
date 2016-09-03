using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct ConnectionAcknowledgedSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.ConnectionAcknowledged; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            bool isHost = currentMachine.IsHost;

            // Send a priori state
            output.Write(isHost);
            if (isHost)
            {
                output.Write((byte)currentMachine.Session.SessionState);
            }

            output.Write((int)currentMachine.LocalGamers.Count);
            foreach (LocalNetworkGamer localGamer in currentMachine.LocalGamers)
            {
                output.Write(localGamer.DisplayName);
                output.Write(localGamer.Gamertag);
                output.Write(localGamer.Id);
                output.Write(localGamer.IsPrivateSlot);
                output.Write(localGamer.IsReady);
            }
        }
    }

    internal struct ConnectionAcknowledgedReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (senderMachine.HasAcknowledgedLocalMachine)
            {
                // TODO: SuspiciousRepeatedInfo
                return;
            }
            bool isHost = input.ReadBoolean();
            if (isHost && !senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                return;
            }

            // Receive a priori state
            if (isHost)
            {
                currentMachine.Session.SessionState = (NetworkSessionState)input.ReadByte();
            }

            int gamerCount = input.ReadInt32();

            if (gamerCount > 0 && !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                return;
            }

            for (int i = 0; i < gamerCount; i++)
            {
                string displayName = input.ReadString();
                string gamertag = input.ReadString();
                byte id = input.ReadByte();
                bool isPrivateSlot = input.ReadBoolean();
                bool isReady = input.ReadBoolean();

                if (currentMachine.Session.FindGamerById(id) != null)
                {
                    // TODO: SuspiciousGamerIdCollision
                    return;
                }

                NetworkGamer remoteGamer = new NetworkGamer(senderMachine, displayName, gamertag, id, isPrivateSlot, isReady);
                currentMachine.Session.AddGamer(remoteGamer);
            }

            // Everything went fine
            senderMachine.HasAcknowledgedLocalMachine = true;
        }
    }
}
