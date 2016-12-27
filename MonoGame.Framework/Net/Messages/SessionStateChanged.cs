using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class SessionStateChanged : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send SessionStateChanged");
            }

            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.SessionStateChanged);

            msg.Write(CurrentMachine.Session.allowHostMigration);
            msg.Write(CurrentMachine.Session.allowJoinInProgress);
            msg.Write(CurrentMachine.Session.maxGamers);
            msg.Write(CurrentMachine.Session.privateGamerSlots);
            CurrentMachine.Session.SessionProperties.Pack(msg);
            msg.Write((byte)CurrentMachine.Session.SessionState);

            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            CurrentMachine.Session.allowHostMigration = msg.ReadBoolean();
            CurrentMachine.Session.allowJoinInProgress = msg.ReadBoolean();
            CurrentMachine.Session.maxGamers = msg.ReadInt();
            CurrentMachine.Session.privateGamerSlots = msg.ReadInt();
            CurrentMachine.Session.SessionProperties.Unpack(msg);
            CurrentMachine.Session.SessionState = (NetworkSessionState)msg.ReadByte();

            senderMachine.HasSentSessionStateToLocalMachine = true;
        }
    }
}