using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal enum InternalMessageType
    {
        ConnectToAllRequest,
        NoLongerPending,
        GamerJoinRequest,
        GamerJoinResponse,
        GamerJoined,
        GamerLeft,
        GamerStateChange,
        StartGame,
        EndGame,
        User
    }

    internal static class InternalMessage
    {
        public static Type[] MessageToReceiverTypeMap = new Type[]
        {
            typeof(ConnectToAllRequestMessageReceiver),
            typeof(NoLongerPendingMessageReceiver),
            typeof(GamerJoinRequestMessageReceiver),
            typeof(GamerJoinResponseMessageReceiver),
            typeof(GamerJoinedMessageReceiver),
            typeof(GamerLeftMessageReceiver),
            typeof(GamerStateChangeMessageReceiver),
            typeof(StartGameMessageReceiver),
            typeof(EndGameMessageReceiver),
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