using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class FullyConnected : InternalMessage
    {
        public FullyConnected() : base(InternalMessageIndex.FullyConnected)
        { }

        public void Create(NetworkMachine recipient)
        {
            Debug.WriteLine($"Sending {Index} to {CurrentMachine.Session.MachineOwnerName(recipient)}...");
            var msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)Index);
            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
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
                CurrentMachine.Session.hostPendingConnections.Remove(senderMachine);
            }
        }
    }
}
