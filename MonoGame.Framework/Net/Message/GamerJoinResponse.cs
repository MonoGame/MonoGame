using Lidgren.Network;
using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal struct GamerJoinResponseMessageSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GamerJoinResponse; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new InvalidOperationException("Only host can send GamerJoinResponse!");
            }

            byte id;
            bool wasApprovedByHost = NetworkSession.Session.GetNewUniqueId(out id);

            output.Write(wasApprovedByHost);
            output.Write(id);
        }
    }

    internal struct GamerJoinResponseMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            bool wasApprovedByHost = input.ReadBoolean();
            byte id = input.ReadByte();
            
            if (!wasApprovedByHost)
            {
                Debug.WriteLine("Our gamer join request was not accepted by the host!");
                return;
            }

            if (NetworkSession.Session.pendingSignedInGamers.Count == 0)
            {
                Debug.WriteLine("No pending signed in gamers but received gamer join response from host!");
                return;
            }

            // Host approved request, now possible to create network gamer
            bool isFirstSignedInGamer = NetworkSession.Session.pendingSignedInGamers.Count == NetworkSession.Session.initiallyPendingSignedInGamersCount;
            SignedInGamer signedInGamer = NetworkSession.Session.pendingSignedInGamers[0];
            NetworkSession.Session.pendingSignedInGamers.RemoveAt(0);

            bool isGuest = !isFirstSignedInGamer;
            bool isHost = currentMachine.IsHost && isFirstSignedInGamer;
            LocalNetworkGamer localGamer = new LocalNetworkGamer(id, isGuest, isHost, false, NetworkSession.Session, signedInGamer);

            NetworkSession.Session.AddGamer(localGamer);

            NetworkSession.Session.Send(new GamerJoinedMessageSender(localGamer));
        }
    }
}
