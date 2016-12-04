using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class FullyConnected : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.FullyConnected);
            Queue.Place(msg);
        }

        public override void Receive(IIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousRepeatedInfo
                Debug.Assert(false);
                return;
            }

            // The sender machine is now considered fully connected
            senderMachine.IsFullyConnected = true;
            
            if (CurrentMachine.IsHost && !senderMachine.IsLocal)
            {
                senderMachine.hostPendingConnections = null;
            }
        }
    }
}
