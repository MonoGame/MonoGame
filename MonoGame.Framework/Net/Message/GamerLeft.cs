using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal struct GamerLeftMessageSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerLeft; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            throw new NotImplementedException();
        }
    }

    internal struct GamerLeftMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            throw new NotImplementedException();
        }
    }
}
