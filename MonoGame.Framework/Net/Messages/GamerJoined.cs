using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerJoinedMessageSender : IInternalMessageSender
    {
        private LocalNetworkGamer localGamer;

        public GamerJoinedMessageSender(LocalNetworkGamer localGamer)
        {
            this.localGamer = localGamer;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.GamerJoined; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            output.Write(localGamer.DisplayName);
            output.Write(localGamer.Gamertag);
            output.Write(localGamer.Id);
            output.Write(localGamer.IsPrivateSlot);
            output.Write(localGamer.IsReady);
        }
    }

    internal struct GamerJoinedMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }

            string displayName = input.ReadString();
            string gamertag = input.ReadString();
            byte id = input.ReadByte();
            bool isPrivateSlot = input.ReadBoolean();
            bool isReady = input.ReadBoolean();

            /*
            if (currentMachine.IsHost && isHost)
            {
                // New gamer is claiming to be host but we know we are, let them
                // join (to keep gamer lists in sync) but kick them asap
                senderMachine.RemoveFromSession();
            }
            */

            if (NetworkSession.Session.FindGamerById(id) != null)
            {
                // New gamer is trying to use an id that is already being used
                if (currentMachine.IsHost)
                {
                    senderMachine.RemoveFromSession();
                }

                return;
            }

            NetworkGamer remoteGamer = new NetworkGamer(senderMachine, displayName, gamertag, id, isPrivateSlot, isReady);
            NetworkSession.Session.AddGamer(remoteGamer);
        }
    }
}
