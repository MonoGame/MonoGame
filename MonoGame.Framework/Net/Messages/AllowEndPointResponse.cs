using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class AllowEndPointResponse : InternalMessage
    {
        public void Create(IPeerEndPoint endPoint, NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.AllowEndPointResponse);
            msg.Write(endPoint);
            Queue.Place(msg);
        }

        public override void Receive(IIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsHost)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            IPeerEndPoint endPoint = msg.ReadPeerEndPoint();

            senderMachine.hostPendingAllowlistInsertions.Remove(endPoint);

            // Should we introduce the peers?
            IPeer remotePeer = CurrentMachine.Session.Backend.FindRemotePeerByEndPoint(endPoint);

            if (remotePeer == null)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            NetworkMachine allowedMachine = remotePeer.Tag as NetworkMachine;

            if (!allowedMachine.hostPendingAllowlistInsertions.Contains(senderMachine.peer.EndPoint))
            {
                CurrentMachine.Session.Backend.Introduce(senderMachine.peer, allowedMachine.peer);
            }
        }
    }
}
