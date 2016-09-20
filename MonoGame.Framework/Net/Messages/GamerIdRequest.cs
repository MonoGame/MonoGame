using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerIdRequestSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageType.GamerIdRequest);
            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsHost || !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            CurrentMachine.Session.internalMessages.GamerIdResponse.Create(senderMachine);
        }
    }
}
