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

            IPeerEndPoint allowedEndPoint = msg.ReadPeerEndPoint();
            IPeer allowedPeer = CurrentMachine.Session.Backend.FindRemotePeerByEndPoint(allowedEndPoint);

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
                CurrentMachine.Session.Backend.Introduce(senderMachine.peer, allowedMachine.peer);
            }
        }
    }
}
