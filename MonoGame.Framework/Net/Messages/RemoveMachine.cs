using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct RemoveMachineSender : IInternalMessageContent
    {
        private NetworkMachine machine;

        public RemoveMachineSender(NetworkMachine machine)
        {
            this.machine = machine;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.RemoveMachine; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(IOutgoingMessage output, NetworkMachine currentMachine)
        {
            output.Write(machine.peer);
        }
    }

    internal class RemoveMachineReceiver : IInternalMessageReceiver
    {
        public void Receive(IIncomingMessage input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            IPeer removePeer = input.ReadPeer();

            if (removePeer == currentMachine.Session.backend.LocalPeer)
            {
                currentMachine.Session.End(NetworkSessionEndReason.RemovedByHost);
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
