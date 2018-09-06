using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class ResetReady : InternalMessage
    {
        public ResetReady() : base(InternalMessageIndex.ResetReady)
        { }

        public void Create()
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send ResetReady");
            }

            Debug.WriteLine($"Sending {Index} to {CurrentMachine.Session.MachineOwnerName(null)}...");
            var msg = Backend.GetMessage(null, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)Index);
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

            // Make sure that the host can not accidentaly start the game too early
            if (CurrentMachine.IsHost)
            {
                foreach (var gamer in CurrentMachine.Session.allGamers)
                {
                    // Safe because any ready state change from a remote gamer will happen after the scope of this Receive() call
                    gamer.SetReadyState(false);
                }
            }

            // Tell everyone that our local gamers are not yet ready
            foreach (var localGamer in CurrentMachine.localGamers)
            {
                localGamer.SetReadyState(false);

                CurrentMachine.Session.InternalMessages.GamerStateChanged.Create(localGamer, false, true, null);
            }
        }
    }
}
