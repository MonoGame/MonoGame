using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerIdResponse : InternalMessage
    {
        public GamerIdResponse() : base(InternalMessageIndex.GamerIdResponse)
        { }

        public void Create(NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send GamerIdResponse");
            }

            Debug.WriteLine($"Sending {Index} to {CurrentMachine.Session.MachineOwnerName(recipient)}...");
            var msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)Index);

            byte id;
            bool wasApprovedByHost = CurrentMachine.Session.GetNewUniqueId(out id);

            msg.Write(wasApprovedByHost);
            msg.Write(id);

            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }
            if (!CurrentMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            bool wasApprovedByHost = msg.ReadBoolean();
            byte id = msg.ReadByte();

            if (CurrentMachine.Session.FindGamerById(id) != null)
            {
                // TODO: SuspiciousGamerIdCollision
                Debug.Assert(false);
                return;
            }

            if (CurrentMachine.Session.pendingSignedInGamers.Count == 0)
            {
                Debug.WriteLine("Warning: GamerIdResponse received but there are no pending signed in gamers!");
                return;
            }

            // Host approved request, now possible to create network gamer
            var signedInGamer = CurrentMachine.Session.pendingSignedInGamers[0];
            CurrentMachine.Session.pendingSignedInGamers.RemoveAt(0);

            if (!wasApprovedByHost)
            {
                Debug.WriteLine("Warning: GamerIdResponse received, GamerIdRequest declined by host!");
                return;
            }

            var localGamer = new LocalNetworkGamer(CurrentMachine, signedInGamer, id, false);
            CurrentMachine.Session.AddGamer(localGamer);
            CurrentMachine.Session.InternalMessages.GamerJoined.Create(localGamer, null);
        }
    }
}
