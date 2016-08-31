using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct InitializePendingMessageSender : IInternalMessageSender
    {
        private ICollection<NetConnection> requestedConnections;

        public InitializePendingMessageSender(ICollection<NetConnection> requestedConnections)
        {
            this.requestedConnections = requestedConnections;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.InitializePending; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send InitializePending");
            }

            // Session state
            output.Write((byte)NetworkSession.Session.SessionState);

            // Requested connections
            output.Write((int)requestedConnections.Count);
            foreach (NetConnection c in requestedConnections)
            {
                output.Write(c.RemoteEndPoint);
            }
        }
    }

    internal struct InitializePendingMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                return;
            }

            if (senderMachine.IsLocal)
            {
                throw new NetworkException("InitializePending should never be sent to self");
            }

            // Session state
            NetworkSession.Session.SessionState = (NetworkSessionState)input.ReadByte();

            // Requested connections
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
