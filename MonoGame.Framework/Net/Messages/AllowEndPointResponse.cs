using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class AllowEndPointResponse : InternalMessage
    {
        public AllowEndPointResponse() : base(InternalMessageIndex.AllowEndPointResponse)
        { }

        public void Create(BasePeerEndPoint endPoint, NetworkMachine recipient)
        {
            Debug.WriteLine($"Sending {Index} to {CurrentMachine.Session.MachineOwnerName(recipient)}...");
            var msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)Index);
            msg.Write(endPoint);
            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsHost)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            var allowedEndPoint = msg.ReadPeerEndPoint();
            var allowedPeer = CurrentMachine.Session.Backend.FindRemotePeerByEndPoint(allowedEndPoint);

            if (allowedPeer == null)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            var allowedMachine = allowedPeer.Tag as NetworkMachine;

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
