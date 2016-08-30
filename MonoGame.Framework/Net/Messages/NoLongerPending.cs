using System;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct NoLongerPendingMessageSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.NoLongerPending; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        { }
    }

    internal struct NoLongerPendingMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsPending)
            {
                return;
            }            

            // The sender machine is now considered fully connected
            senderMachine.IsPending = false;

            // Remote peer?
            if (!senderMachine.IsLocal)
            {
                if (currentMachine.IsHost)
                {
                    NetworkSession.Session.pendingPeerConnections.Remove(senderMachine.connection);
                }

                // Send gamer joined messages to the newly fully connected remote peer
                foreach (LocalNetworkGamer localGamer in currentMachine.localGamers)
                {
                    NetworkSession.Session.Send(new GamerJoinedMessageSender(localGamer), senderMachine);
                }
            }
        }
    }
}
