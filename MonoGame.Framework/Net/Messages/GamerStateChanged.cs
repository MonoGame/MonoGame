using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerStateChangedSender : IInternalMessageContent
    {
        private LocalNetworkGamer localGamer;
        private bool sendNames;
        private bool sendFlags;

        public GamerStateChangedSender(LocalNetworkGamer localGamer, bool sendNames, bool sendFlags)
        {
            this.localGamer = localGamer;
            this.sendNames = sendNames;
            this.sendFlags = sendFlags;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.GamerStateChanged; } }
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
                output.Write(localGamer.IsPrivateSlot);
                output.Write(localGamer.IsReady);
            }
        }
    }

    internal struct GamerStateChangedReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }

            byte id = input.ReadByte();
            NetworkGamer remoteGamer = currentMachine.Session.FindGamerById(id);

            if (remoteGamer.Machine != senderMachine)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
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
                remoteGamer.IsPrivateSlot = input.ReadBoolean();
                remoteGamer.SetReadyState(input.ReadBoolean());
            }
        }
    }
}