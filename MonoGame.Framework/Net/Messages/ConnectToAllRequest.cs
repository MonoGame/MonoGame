using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct ConnectToAllRequestSender : IInternalMessageContent
    {
        private ICollection<NetworkMachine> requestedConnections;

        public ConnectToAllRequestSender(ICollection<NetworkMachine> requestedConnections)
        {
            this.requestedConnections = requestedConnections;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.ConnectToAllRequest; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send ConnectToAllRequest");
            }

            output.Write((int)requestedConnections.Count);
            foreach (NetworkMachine machine in requestedConnections)
            {
                output.Write(machine.connection.RemoteEndPoint);
            }
        }
    }

    internal class ConnectToAllRequestReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                throw new NetworkException("ConnectToAllRequest should never be sent to self");
            }
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }
            if (currentMachine.IsFullyConnected)
            {
                // TODO: SuspiciousRepeatedInfo
                Debug.Assert(false);
                return;
            }

            int requestedConnectionCount = input.ReadInt32();
            currentMachine.Session.pendingEndPoints = new List<IPEndPoint>(requestedConnectionCount);
            for (int i = 0; i < requestedConnectionCount; i++)
            {
                IPEndPoint endPoint = input.ReadIPEndPoint();
                currentMachine.Session.pendingEndPoints.Add(endPoint);

                if (!currentMachine.Session.IsConnectedToEndPoint(endPoint))
                {
                    currentMachine.Session.peer.Connect(endPoint);
                }
            }
        }
    }
}
