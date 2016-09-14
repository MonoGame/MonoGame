using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct FullyConnectedSender : IInternalMessageContent
    {
        public InternalMessageType MessageType { get { return InternalMessageType.FullyConnected; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(IOutgoingMessage output, NetworkMachine currentMachine)
        { }
    }

    internal class FullyConnectedReceiver : IInternalMessageReceiver
    {
        public void Receive(IIncomingMessage input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousRepeatedInfo
                Debug.Assert(false);
                return;
            }

            // The sender machine is now considered fully connected
            senderMachine.IsFullyConnected = true;
            
            if (currentMachine.IsHost && !senderMachine.IsLocal)
            {
                currentMachine.Session.pendingPeerConnections.Remove(senderMachine);
            }
        }
    }
}
