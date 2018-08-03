using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal enum InternalMessageIndex : byte
    {
        AllowEndPointRequest,
        AllowEndPointResponse,
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
        ResetReady,
        SessionStateChanged,
        UserMessage
    }

    internal class InternalMessages
    {
        public AllowEndPointRequest AllowEndPointRequest = new AllowEndPointRequest();
        public AllowEndPointResponse AllowEndPointResponse = new AllowEndPointResponse();
        public ConnectionAcknowledged ConnectionAcknowledged = new ConnectionAcknowledged();
        public ConnectToAllRequest ConnectToAllRequest = new ConnectToAllRequest();
        public FullyConnected FullyConnected = new FullyConnected();
        public GameEnded GameEnded = new GameEnded();
        public GamerIdRequest GamerIdRequest = new GamerIdRequest();
        public GamerIdResponse GamerIdResponse = new GamerIdResponse();
        public GamerJoined GamerJoined = new GamerJoined();
        public GamerLeft GamerLeft = new GamerLeft();
        public GamerStateChanged GamerStateChanged = new GamerStateChanged();
        public GameStarted GameStarted = new GameStarted();
        public RemoveMachine RemoveMachine = new RemoveMachine();
        public ResetReady ResetReady = new ResetReady();
        public SessionStateChanged SessionStateChanged = new SessionStateChanged();
        public UserMessage UserMessage = new UserMessage();

        public InternalMessage[] FromIndex;

        public InternalMessages(BaseSessionBackend backend, IMessageQueue queue, NetworkMachine currentMachine)
        {
            FromIndex = new InternalMessage[]
            {
                AllowEndPointRequest,
                AllowEndPointResponse,
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
                ResetReady,
                SessionStateChanged,
                UserMessage
            };

            foreach (InternalMessage internalMessage in FromIndex)
            {
                internalMessage.Initialize(backend, queue, currentMachine);
            }
        }
    }

    internal interface IMessageQueue
    {
        void Place(BaseOutgoingMessage msg);
    }

    internal abstract class InternalMessage
    {
        public BaseSessionBackend Backend { get; private set; }
        public IMessageQueue Queue { get; private set; }
        public NetworkMachine CurrentMachine { get; private set; }

        public void Initialize(BaseSessionBackend backend, IMessageQueue queue, NetworkMachine currentMachine)
        {
            this.Backend = backend;
            this.Queue = queue;
            this.CurrentMachine = currentMachine;
        }

        public abstract void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine);
    }
}
