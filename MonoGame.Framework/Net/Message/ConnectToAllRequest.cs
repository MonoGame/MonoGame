using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal struct ConnectToAllRequestMessageSender : IInternalMessageSender
    {
        private ICollection<NetConnection> requestedConnections;

        public ConnectToAllRequestMessageSender(ICollection<NetConnection> requestedConnections)
        {
            this.requestedConnections = requestedConnections;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.ConnectToAllRequest; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host should send ConnectToAllRequest");
            }

            output.Write((int)requestedConnections.Count);
            foreach (NetConnection c in requestedConnections)
            {
                output.Write(c.RemoteEndPoint);
            }
        }
    }

    internal struct ConnectToAllRequestMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                throw new InvalidOperationException("ConnectToAllRequest should never be sent to self!");
            }

            int requestedConnectionCount = input.ReadInt32();

            NetworkSession.Session.pendingEndPoints = new List<IPEndPoint>(requestedConnectionCount);

            for (int i = 0; i < requestedConnectionCount; i++)
            {
                IPEndPoint endPoint = input.ReadIPEndPoint();

                NetworkSession.Session.pendingEndPoints.Add(endPoint);

                if (!NetworkSession.Session.IsConnectedToEndPoint(endPoint))
                {
                    NetworkSession.Session.peer.Connect(endPoint);
                }
            }
        }
    }
}
