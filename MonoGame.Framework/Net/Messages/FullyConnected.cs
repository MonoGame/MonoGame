using System;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct FullyConnectedSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.FullyConnected; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        { }
    }

    internal struct FullyConnectedReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsFullyConnected)
            {
                return;
            }

            // The sender machine is now considered fully connected
            senderMachine.IsFullyConnected = true;

            // Remote peer?
            if (currentMachine.IsHost && !senderMachine.IsLocal)
            {
                NetworkSession.Session.pendingPeerConnections.Remove(senderMachine.connection);
            }
        }
    }
}
