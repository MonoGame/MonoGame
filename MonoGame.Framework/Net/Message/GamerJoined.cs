using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Message
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
            output.Write(localGamer.IsGuest);
            output.Write(localGamer.IsHost);
            output.Write(localGamer.IsPrivateSlot);
        }
    }

    internal struct GamerJoinedMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            string displayName = input.ReadString();
            string gamertag = input.ReadString();
            byte id = input.ReadByte();
            bool isGuest = input.ReadBoolean();
            bool isHost = input.ReadBoolean();
            bool isPrivateSlot = input.ReadBoolean();

            if (!senderMachine.IsLocal && currentMachine.IsHost && isHost)
            {
                // New gamer is claiming to be host but we know we are, kick their machine
                senderMachine.RemoveFromSession();
                return;
            }

            if (senderMachine.IsLocal)
            {
                NetworkGamer localGamer = NetworkSession.Session.FindGamerById(id);

                if (!localGamer.IsLocal)
                {
                    throw new InvalidOperationException("Remote gamer joined from local machine!");
                }

                NetworkSession.Session.InvokeGamerJoinedEvent(new GamerJoinedEventArgs(localGamer));
            }
            else
            {
                NetworkGamer remoteGamer = new NetworkGamer(displayName, gamertag, id, isGuest, isHost, false, isPrivateSlot, senderMachine, NetworkSession.Session);
                NetworkSession.Session.AddGamer(remoteGamer);

                NetworkSession.Session.InvokeGamerJoinedEvent(new GamerJoinedEventArgs(remoteGamer));
            }
        }
    }
}
