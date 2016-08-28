using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal struct GamerJoinRequestMessageSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerJoinRequest; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        { }
    }

    internal struct GamerJoinRequestMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (currentMachine.IsHost)
            {
                NetworkSession.Session.Send(new GamerJoinResponseMessageSender(), senderMachine);
            }
        }
    }
}
