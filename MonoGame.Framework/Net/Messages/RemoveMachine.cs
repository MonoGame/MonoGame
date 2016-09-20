using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class RemoveMachineSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(NetworkMachine machine, NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageType.RemoveMachine);

            msg.Write(machine.peer);

            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            IPeer removePeer = input.ReadPeer();

            if (removePeer == CurrentMachine.Session.backend.LocalPeer)
            {
                CurrentMachine.Session.End(NetworkSessionEndReason.RemovedByHost);
            }
            else
            {
                if (removePeer != null)
                {
                    removePeer.Disconnect("Removed by host");
                }
            }
        }
    }
}
