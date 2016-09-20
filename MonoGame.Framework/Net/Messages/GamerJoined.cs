using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerJoinedSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(LocalNetworkGamer localGamer, NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageType.GamerJoined);

            msg.Write(localGamer.DisplayName);
            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.Id);
            msg.Write(localGamer.IsPrivateSlot);
            msg.Write(localGamer.IsReady);

            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            string displayName = input.ReadString();
            string gamertag = input.ReadString();
            byte id = input.ReadByte();
            bool isPrivateSlot = input.ReadBoolean();
            bool isReady = input.ReadBoolean();

            if (CurrentMachine.Session.FindGamerById(id) != null)
            {
                // TODO: SuspiciousGamerIdCollision
                Debug.Assert(false);
                return;
            }

            NetworkGamer remoteGamer = new NetworkGamer(senderMachine, displayName, gamertag, id, isPrivateSlot, isReady);
            CurrentMachine.Session.AddGamer(remoteGamer);
        }
    }
}
