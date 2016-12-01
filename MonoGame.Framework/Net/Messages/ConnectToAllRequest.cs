using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class ConnectToAllRequest : InternalMessage
    {
        public void Create(ICollection<NetworkMachine> requestedConnections, NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.ConnectToAllRequest);

            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send ConnectToAllRequest");
            }

            msg.Write((int)requestedConnections.Count);
            foreach (NetworkMachine machine in requestedConnections)
            {
                msg.Write(machine.peer.EndPoint);
            }

            Queue.Place(msg);
        }

        public override void Receive(IIncomingMessage msg, NetworkMachine senderMachine)
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
            if (CurrentMachine.IsFullyConnected)
            {
                // TODO: SuspiciousRepeatedInfo
                Debug.Assert(false);
                return;
            }

            int requestedConnectionCount = msg.ReadInt();
            CurrentMachine.Session.pendingEndPoints = new List<IPeerEndPoint>(requestedConnectionCount);
            for (int i = 0; i < requestedConnectionCount; i++)
            {
                IPeerEndPoint endPoint = msg.ReadPeerEndPoint();
                CurrentMachine.Session.pendingEndPoints.Add(endPoint);
            }
        }
    }
}
