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
        UserMessage
    }

    internal static class InternalMessage
    {
        public static Type[] MessageToReceiverTypeMap = new Type[]
        {
            typeof(ConnectionAcknowledgedReceiver),
            typeof(ConnectToAllRequestReceiver),
            typeof(FullyConnectedReceiver),
            typeof(GamerIdRequestReceiver),
            typeof(GamerIdResponseReceiver),
            typeof(GamerJoinedReceiver),
            typeof(GamerLeftReceiver),
            typeof(GamerStateChangedReceiver),
            typeof(GameStartedReceiver),
            typeof(GameEndedReceiver),
            typeof(UserMessageReceiver)
        };
    }

    internal interface IInternalMessageSender
    {
        InternalMessageType MessageType { get; }
        int SequenceChannel { get; }
        SendDataOptions Options { get; }
        void Send(NetBuffer output, NetworkMachine currentMachine);
    }

    internal interface IInternalMessageReceiver
    {
        void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine);
    }
}