using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerIdRequestSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerIdRequest; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        { }
    }

    internal struct GamerIdRequestReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!currentMachine.IsHost)
            {
                Debug.WriteLine("Warning: Received GamerIdRequest when not host!");
                // TODO: SuspiciousUnexpectedMessage
                return;
            }
            if (!senderMachine.IsFullyConnected)
            {
                Debug.WriteLine("Warning: Received GamerIdRequest from not fully connected peer!");
                // TODO: SuspiciousUnexpectedMessage
                return;
            }

            currentMachine.Session.Send(new GamerIdResponseSender(), senderMachine);
        }
    }
}
