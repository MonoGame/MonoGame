using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal enum InternalMessageType
    {
        ConnectionAcknowledged,
        ConnectToAllRequest,
        FullyConnected,
        GameEnded,
        GamerIdRequest,
        GamerIdResponse,
        GamerJoined,
        GamerLeft,
        GamerStateChanged,
        GameStarted,
        RemoveMachine,
        UserMessage
    }

    internal class InternalMessages
    {
        public ConnectionAcknowledgedSender ConnectionAcknowledged = new ConnectionAcknowledgedSender();
        public ConnectToAllRequestSender ConnectToAllRequest = new ConnectToAllRequestSender();
        public FullyConnectedSender FullyConnected = new FullyConnectedSender();
        public GameEndedSender GameEnded = new GameEndedSender();
        public GamerIdRequestSender GamerIdRequest = new GamerIdRequestSender();
        public GamerIdResponseSender GamerIdResponse = new GamerIdResponseSender();
        public GamerJoinedSender GamerJoined = new GamerJoinedSender();
        public GamerLeftSender GamerLeft = new GamerLeftSender();
        public GamerStateChangedSender GamerStateChanged = new GamerStateChangedSender();
        public GameStartedSender GameStarted = new GameStartedSender();
        public RemoveMachineSender RemoveMachine = new RemoveMachineSender();
        public UserMessageSender UserMessage = new UserMessageSender();

        public IInternalMessage[] ByType;

        public InternalMessages(IBackend backend, IMessageQueue queue, NetworkMachine currentMachine)
        {
            ByType = new IInternalMessage[]
            {
                ConnectionAcknowledged,
                ConnectToAllRequest,
                FullyConnected,
                GameEnded,
                GamerIdRequest,
                GamerIdResponse,
                GamerJoined,
                GamerLeft,
                GamerStateChanged,
                GameStarted,
                RemoveMachine,
                UserMessage
            };

            foreach (IInternalMessage internalMessage in ByType)
            {
                internalMessage.Backend = backend;
                internalMessage.Queue = queue;
                internalMessage.CurrentMachine = currentMachine;
            }
        }
    }

    internal interface IMessageQueue
    {
        void Place(IOutgoingMessage msg);
    }

    internal interface IInternalMessage
    {
        IBackend Backend { get; set; }
        IMessageQueue Queue { get; set; }
        NetworkMachine CurrentMachine { get; set; }

        void Receive(IIncomingMessage input, NetworkMachine senderMachine);
    }
}
