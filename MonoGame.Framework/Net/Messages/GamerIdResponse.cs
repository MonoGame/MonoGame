using Lidgren.Network;
using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerIdResponseSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerIdResponse; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send GamerIdResponse");
            }

            byte id;
            bool wasApprovedByHost = currentMachine.Session.GetNewUniqueId(out id);

            output.Write(wasApprovedByHost);
            output.Write(id);
        }
    }

    internal struct GamerIdResponseReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.WriteLine("Warning: Received GamerIdResponse from non-host!");
                return;
            }
            if (!currentMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.WriteLine("Warning: Received GamerIdResponse when not fully connected!");
                return;
            }

            bool wasApprovedByHost = input.ReadBoolean();
            byte id = input.ReadByte();

            if (currentMachine.Session.FindGamerById(id) != null)
            {
                // TODO: SuspiciousGamerIdCollision
                Debug.WriteLine("Warning: GamerIdResponse received with colliding id!");
                return;
            }
            if (currentMachine.Session.joiningSignedInGamers.Count == 0)
            {
                Debug.WriteLine("Warning: GamerIdResponse received but no signed in gamers waiting to join!");
                return;
            }

            // Host approved request, now possible to create network gamer
            SignedInGamer signedInGamer = currentMachine.Session.joiningSignedInGamers[0];
            currentMachine.Session.joiningSignedInGamers.RemoveAt(0);

            if (!wasApprovedByHost)
            {
                Debug.WriteLine("Warning: GamerIdResponse received, GamerIdRequest declined by host!");
                return;
            }

            LocalNetworkGamer localGamer = new LocalNetworkGamer(currentMachine, signedInGamer, id, false);
            currentMachine.Session.AddGamer(localGamer);
            currentMachine.Session.Send(new GamerJoinedSender(localGamer));
        }
    }
}
