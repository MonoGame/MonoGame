using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class ConnectToAllRequest : InternalMessage
    {
        public ConnectToAllRequest() : base(InternalMessageIndex.ConnectToAllRequest)
        { }

        public void Create(ICollection<NetworkMachine> machinesToConnectTo, NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send ConnectToAllRequest");
            }

            Debug.WriteLine($"Sending {Index} to {CurrentMachine.Session.MachineOwnerName(recipient)}...");
            var msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)Index);

            msg.Write((int)machinesToConnectTo.Count);
            foreach (NetworkMachine machine in machinesToConnectTo)
            {
                msg.Write(machine.peer.EndPoint);
            }

            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
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
            CurrentMachine.Session.pendingEndPoints = new HashSet<BasePeerEndPoint>();
            for (int i = 0; i < requestedConnectionCount; i++)
            {
                var endPoint = msg.ReadPeerEndPoint();
                CurrentMachine.Session.pendingEndPoints.Add(endPoint);
            }
        }
    }
}
