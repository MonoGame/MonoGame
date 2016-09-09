using Lidgren.Network;
using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerIdResponseSender : IInternalMessageContent
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerIdResponse; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(NetBuffer output, NetworkMachine currentMachine)
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

    internal class GamerIdResponseReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }
            if (!currentMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            bool wasApprovedByHost = input.ReadBoolean();
            byte id = input.ReadByte();

            if (currentMachine.Session.FindGamerById(id) != null)
            {
                // TODO: SuspiciousGamerIdCollision
                Debug.Assert(false);
                return;
            }

            if (currentMachine.Session.pendingSignedInGamers.Count == 0)
            {
                Debug.WriteLine("Warning: GamerIdResponse received but there are no pending signed in gamers!");
                return;
            }

            // Host approved request, now possible to create network gamer
            SignedInGamer signedInGamer = currentMachine.Session.pendingSignedInGamers[0];
            currentMachine.Session.pendingSignedInGamers.RemoveAt(0);

            if (!wasApprovedByHost)
            {
                Debug.WriteLine("Warning: GamerIdResponse received, GamerIdRequest declined by host!");
                return;
            }

            LocalNetworkGamer localGamer = new LocalNetworkGamer(currentMachine, signedInGamer, id, false);
            currentMachine.Session.AddGamer(localGamer);
            currentMachine.Session.QueueMessage(new GamerJoinedSender(localGamer));
        }
    }
}
