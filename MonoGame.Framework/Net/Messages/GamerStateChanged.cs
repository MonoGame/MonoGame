using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerStateChangedSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(LocalNetworkGamer localGamer, bool sendNames, bool sendFlags, NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageType.GamerStateChanged);

            msg.Write(localGamer.Id);

            msg.Write(sendNames);
            if (sendNames)
            {
                msg.Write(localGamer.DisplayName);
                msg.Write(localGamer.Gamertag);
            }

            msg.Write(sendFlags);
            if (sendFlags)
            {
                msg.Write(localGamer.IsPrivateSlot);
                msg.Write(localGamer.IsReady);
            }

            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }

            byte id = input.ReadByte();
            NetworkGamer remoteGamer = CurrentMachine.Session.FindGamerById(id);

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