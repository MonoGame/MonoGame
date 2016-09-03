using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GameStartedSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GameStarted; } }
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

    internal struct GameStartedReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.WriteLine("Warning: Received GameStarted from non-host!");
                return;
            }

            // Reset state after exiting lobby
            currentMachine.Session.SessionState = NetworkSessionState.Playing;
            currentMachine.Session.InvokeGameStartedEvent(new GameStartedEventArgs());

            // Reset ready state for esthetic reasons only, the code that matters is in the GameEnded message!
            // TODO: Should we even do this?
            foreach (LocalNetworkGamer localGamer in currentMachine.LocalGamers)
            {
                localGamer.SetReadyState(false);

                currentMachine.Session.Send(new GamerStateChangedSender(localGamer, false, true));
            }
        }
    }
}