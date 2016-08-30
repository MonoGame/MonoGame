using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal struct GamerStateChangeMessageSender : IInternalMessageSender
    {
        private LocalNetworkGamer localGamer;
        private bool sendNames;
        private bool sendFlags;

        public GamerStateChangeMessageSender(LocalNetworkGamer localGamer, bool sendNames, bool sendFlags)
        {
            this.localGamer = localGamer;
            this.sendNames = sendNames;
            this.sendFlags = sendFlags;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.GamerStateChange; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            output.Write(localGamer.Id);

            output.Write(sendNames);
            if (sendNames)
            {
                output.Write(localGamer.DisplayName);
                output.Write(localGamer.Gamertag);
            }

            output.Write(sendFlags);
            if (sendFlags)
            {
                output.Write(localGamer.IsGuest);
                output.Write(localGamer.IsHost);
                output.Write(localGamer.IsPrivateSlot);
                output.Write(localGamer.IsReady);
            }
        }
    }

    internal struct GamerStateChangeMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }

            byte id = input.ReadByte();
            NetworkGamer remoteGamer = NetworkSession.Session.FindGamerById(id);

            if (remoteGamer.Machine != senderMachine)
            {
                return;
            }

            bool readNames = input.ReadBoolean();
            if (readNames)
            {
                remoteGamer.DisplayName = input.ReadString();
                remoteGamer.Gamertag = input.ReadString();
            }

            bool readFlags = input.ReadBoolean();
            if (readFlags)
            {
                bool isGuest = input.ReadBoolean();
                bool isHost = input.ReadBoolean();
                bool isPrivateSlot = input.ReadBoolean();
                bool isReady = input.ReadBoolean();

                remoteGamer.IsGuest = isGuest;

                if (!isHost || (isHost && senderMachine.IsHost))
                {
                    remoteGamer.IsHost = isHost;
                }

                remoteGamer.IsPrivateSlot = isPrivateSlot;
                remoteGamer.SetReadyState(isReady);
            }
        }
    }
}