using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class ConnectionAcknowledged : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.ConnectionAcknowledged);

            bool isHost = CurrentMachine.IsHost;

            // Send a priori state
            msg.Write(isHost);
            if (isHost)
            {
                msg.Write((byte)CurrentMachine.Session.SessionState);
            }

            msg.Write((int)CurrentMachine.LocalGamers.Count);
            foreach (LocalNetworkGamer localGamer in CurrentMachine.LocalGamers)
            {
                msg.Write(localGamer.DisplayName);
                msg.Write(localGamer.Gamertag);
                msg.Write(localGamer.Id);
                msg.Write(localGamer.IsPrivateSlot);
                msg.Write(localGamer.IsReady);
            }

            Queue.Place(msg);
        }

        public override void Receive(IIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (senderMachine.HasAcknowledgedLocalMachine)
            {
                // TODO: SuspiciousRepeatedInfo
                Debug.Assert(false);
                return;
            }

            bool isHost = msg.ReadBoolean();

            if (isHost && !senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            // Receive a priori state
            if (isHost)
            {
                CurrentMachine.Session.SessionState = (NetworkSessionState)msg.ReadByte();
            }

            int gamerCount = msg.ReadInt();

            if (gamerCount > 0 && !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            for (int i = 0; i < gamerCount; i++)
            {
                string displayName = msg.ReadString();
                string gamertag = msg.ReadString();
                byte id = msg.ReadByte();
                bool isPrivateSlot = msg.ReadBoolean();
                bool isReady = msg.ReadBoolean();

                if (CurrentMachine.Session.FindGamerById(id) != null)
                {
                    // TODO: SuspiciousGamerIdCollision
                    Debug.Assert(false);
                    return;
                }

                NetworkGamer remoteGamer = new NetworkGamer(senderMachine, displayName, gamertag, id, isPrivateSlot, isReady);
                CurrentMachine.Session.AddGamer(remoteGamer);
            }

            // Everything went fine
            senderMachine.HasAcknowledgedLocalMachine = true;
        }
    }
}
