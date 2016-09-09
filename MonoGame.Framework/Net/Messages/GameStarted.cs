using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GameStartedSender : IInternalMessageContent
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GameStarted; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send StartGame");
            }
        }
    }

    internal class GameStartedReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            // Reset state after exiting lobby
            currentMachine.Session.SessionState = NetworkSessionState.Playing;
            currentMachine.Session.InvokeGameStartedEvent(new GameStartedEventArgs());
        }
    }
}