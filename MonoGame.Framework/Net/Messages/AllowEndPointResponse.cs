using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class AllowEndPointResponse : InternalMessage
    {
        public void Create(PeerEndPoint endPoint, NetworkMachine recipient)
        {
            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.AllowEndPointResponse);
            msg.Write(endPoint);
            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsHost)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            PeerEndPoint allowedEndPoint = msg.ReadPeerEndPoint();
            Peer allowedPeer = CurrentMachine.Session.Backend.FindRemotePeerByEndPoint(allowedEndPoint);

            if (allowedPeer == null)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            NetworkMachine allowedMachine = allowedPeer.Tag as NetworkMachine;

            // Should we introduce the peers?
            if (!CurrentMachine.Session.hostPendingAllowlistInsertions.ContainsKey(senderMachine) ||
                !CurrentMachine.Session.hostPendingAllowlistInsertions.ContainsKey(allowedMachine))
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            CurrentMachine.Session.hostPendingAllowlistInsertions[senderMachine].Remove(allowedEndPoint);

            if (!CurrentMachine.Session.hostPendingAllowlistInsertions[allowedMachine].Contains(senderMachine.peer.EndPoint))
            {
                // Introduce both ways in order to support multiple players behind the same router
                CurrentMachine.Session.Backend.Introduce(senderMachine.peer, allowedMachine.peer);
                CurrentMachine.Session.Backend.Introduce(allowedMachine.peer, senderMachine.peer);
            }
        }
    }
}
