using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct StartGameMessageSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.StartGame; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send StartGame");
            }
        }
    }

    internal struct StartGameMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                return;
            }

            // Reset ready state for esthetic reasons only, the code that matters is in EndGameMessage!
            foreach (LocalNetworkGamer localGamer in currentMachine.LocalGamers)
            {
                localGamer.IsReady = false;
            }

            NetworkSession.Session.SessionState = NetworkSessionState.Playing;
            NetworkSession.Session.InvokeGameStartedEvent(new GameStartedEventArgs());
        }
    }
}