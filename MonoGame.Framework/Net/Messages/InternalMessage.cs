using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal enum InternalMessageType
    {
        ConnectionAcknowledged,
        ConnectToAllRequest,
        FullyConnected,
        GamerIdRequest,
        GamerIdResponse,
        GamerJoined,
        GamerLeft,
        GamerStateChanged,
        GameStarted,
        GameEnded,
        UserMessage,
        RemoveMachine
    }

    internal static class InternalMessageReceivers
    {
        public static IInternalMessageReceiver[] FromType =
        {
            new ConnectionAcknowledgedReceiver(),
            new ConnectToAllRequestReceiver(),
            new FullyConnectedReceiver(),
            new GamerIdRequestReceiver(),
            new GamerIdResponseReceiver(),
            new GamerJoinedReceiver(),
            new GamerLeftReceiver(),
            new GamerStateChangedReceiver(),
            new GameStartedReceiver(),
            new GameEndedReceiver(),
            new UserMessageReceiver(),
            new RemoveMachineReceiver()
        };
    }

    internal interface IInternalMessageContent
    {
        InternalMessageType MessageType { get; }
        int SequenceChannel { get; }
        SendDataOptions Options { get; }
        void Write(NetBuffer output, NetworkMachine currentMachine);
    }

    internal interface IInternalMessageReceiver
    {
        void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine);
    }

    internal struct InternalMessage
    {
        public IInternalMessageContent content;
        public NetworkMachine recipient;

        public InternalMessage(IInternalMessageContent content, NetworkMachine recipient)
        {
            this.content = content;
            this.recipient = recipient;
        }
    }
}
