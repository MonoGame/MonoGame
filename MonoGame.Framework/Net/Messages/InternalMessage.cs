using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal enum InternalMessageType
    {
        ConnectToAllRequest,
        ConnectToAllSuccessful,
        GamerJoinRequest,
        GamerJoinResponse,
        GamerJoined,
        GamerLeft,
        User
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