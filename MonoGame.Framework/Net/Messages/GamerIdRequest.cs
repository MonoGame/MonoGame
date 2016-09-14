using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerIdRequestSender : IInternalMessageContent
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerIdRequest; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(IOutgoingMessage output, NetworkMachine currentMachine)
        { }
    }

    internal class GamerIdRequestReceiver : IInternalMessageReceiver
    {
        public void Receive(IIncomingMessage input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!currentMachine.IsHost || !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            currentMachine.Session.QueueMessage(new GamerIdResponseSender(), senderMachine);
        }
    }
}
