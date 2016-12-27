using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class RemoveMachine : InternalMessage
    {
        public void Create(NetworkMachine machine, NetworkMachine recipient)
        {
            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.RemoveMachine);

            msg.Write(machine.peer);

            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            Peer removePeer = msg.ReadPeer();

            if (removePeer == CurrentMachine.Session.Backend.LocalPeer)
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
