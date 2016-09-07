using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct RemoveMachineSender : IInternalMessageSender
    {
        private NetworkMachine machine;

        public RemoveMachineSender(NetworkMachine machine)
        {
            this.machine = machine;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.RemoveMachine; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            output.Write(machine.connection.RemoteUniqueIdentifier);
        }
    }

    internal struct RemoveMachineReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                return;
            }

            long removeId = input.ReadInt64();

            if (removeId == currentMachine.Session.peer.UniqueIdentifier)
            {
                currentMachine.Session.End(NetworkSessionEndReason.RemovedByHost);
            }
            else
            {
                foreach (NetConnection connection in currentMachine.Session.peer.Connections)
                {
                    if (removeId == connection.RemoteUniqueIdentifier)
                    {
                        connection.Disconnect("Removed by host");
                    }
                }
            }
        }
    }
}
