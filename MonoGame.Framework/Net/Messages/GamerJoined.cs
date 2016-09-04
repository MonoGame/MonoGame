using System;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerJoinedSender : IInternalMessageSender
    {
        private LocalNetworkGamer localGamer;

        public GamerJoinedSender(LocalNetworkGamer localGamer)
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

    internal struct GamerJoinedReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                return;
            }

            string displayName = input.ReadString();
            string gamertag = input.ReadString();
            byte id = input.ReadByte();
            bool isPrivateSlot = input.ReadBoolean();
            bool isReady = input.ReadBoolean();

            if (currentMachine.Session.FindGamerById(id) != null)
            {
                // TODO: SuspiciousGamerIdCollision
                return;
            }

            NetworkGamer remoteGamer = new NetworkGamer(senderMachine, displayName, gamertag, id, isPrivateSlot, isReady);
            currentMachine.Session.AddGamer(remoteGamer);
        }
    }
}
