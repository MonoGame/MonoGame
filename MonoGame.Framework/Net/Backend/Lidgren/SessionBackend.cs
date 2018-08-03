using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class SessionBackend : BaseSessionBackend
    {
        private LocalPeer localPeer;
        private List<RemotePeer> remotePeers = new List<RemotePeer>();
        private List<NetConnection> remoteConnections = new List<NetConnection>();

        private GenericPool<OutgoingMessage> outgoingMessagePool = new GenericPool<OutgoingMessage>();
        private GenericPool<IncomingMessage> incomingMessagePool = new GenericPool<IncomingMessage>();

        private DateTime lastMasterServerRegistration = DateTime.MinValue;
        private DateTime lastStatisticsUpdate = DateTime.Now;
        private int lastReceivedBytes = 0;
        private int lastSentBytes = 0;

        public SessionBackend(NetPeer peer)
        {
            localPeer = new LocalPeer(this, peer);

            RemotePeers = new ReadOnlyCollection<RemotePeer>(remotePeers);
            RemoteConnections = new ReadOnlyCollection<NetConnection>(remoteConnections);
        }

        public ReadOnlyCollection<RemotePeer> RemotePeers { get; }
        public ReadOnlyCollection<NetConnection> RemoteConnections { get; }

        public void AddRemotePeer(RemotePeer peer)
        {
            remotePeers.Add(peer);
            remoteConnections.Add(peer.Connection);
        }

        public void RemoveRemotePeer(RemotePeer peer)
        {
            remotePeers.Remove(peer);
            remoteConnections.Remove(peer.Connection);
        }

        public override bool HasShutdown { get { return localPeer.HasShutdown; } }

        public override ISessionBackendListener Listener
        {
            get { return localPeer.Listener; }
            set { localPeer.Listener = value; }
        }

        public override Peer LocalPeer { get { return localPeer; } }

        public override TimeSpan SimulatedLatency
        {
            get { return localPeer.SimulatedLatency; }
            set { localPeer.SimulatedLatency = value; }
        }

        public override float SimulatedPacketLoss
        {
            get { return localPeer.SimulatedPacketLoss; }
            set { localPeer.SimulatedPacketLoss = value; }
        }

        public override int BytesPerSecondReceived { get; set; }
        public override int BytesPerSecondSent { get; set; }

        public override void Introduce(Peer client, Peer target)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var remoteClient = client as RemotePeer;
            var remoteTarget = target as RemotePeer;

            if (remoteClient == null || remoteTarget == null)
            {
                throw new InvalidOperationException("Both client and target must be remote");
            }

            localPeer.Introduce(remoteClient, remoteTarget);
        }

        public LidgrenPeer FindPeerById(long id)
        {
            if (localPeer.SessionId == id)
            {
                return localPeer;
            }

            foreach (var remotePeer in remotePeers)
            {
                if (remotePeer.SessionId == id)
                {
                    return remotePeer;
                }
            }

            return null;
        }

        public override Peer FindRemotePeerByEndPoint(BasePeerEndPoint endPoint)
        {
            foreach (var remotePeer in remotePeers)
            {
                if (remotePeer.EndPoint.Equals(endPoint))
                {
                    return remotePeer;
                }
            }

            return null;
        }

        public override bool IsConnectedToEndPoint(BasePeerEndPoint endPoint)
        {
            return FindRemotePeerByEndPoint(endPoint) != null;
        }

        public override BaseOutgoingMessage GetMessage(Peer recipient, SendDataOptions options, int channel)
        {
            var msg = outgoingMessagePool.Get();
            msg.recipient = recipient;
            msg.options = options;
            msg.channel = channel;
            return msg;
        }

        public override void SendMessage(BaseOutgoingMessage message)
        {
            var msg = message as OutgoingMessage;
            if (msg == null)
            {
                throw new NetworkException("Not possible to mix backends");
            }

            // Send to remote peer(s) if recipient is not the local peer only
            if (msg.Recipient != localPeer)
            {
                localPeer.SendMessage(msg);
            }

            // Send to self if recipient is null or the local peer
            if (msg.Recipient == null || msg.Recipient == localPeer)
            {
                msg.Buffer.Position = 0;

                // Pretend that the message was sent to the local peer over the network
                var incomingMsg = incomingMessagePool.Get();
                incomingMsg.Set(this, msg.Buffer);
                Listener.ReceiveMessage(incomingMsg, localPeer);
                incomingMessagePool.Recycle(incomingMsg);
            }

            outgoingMessagePool.Recycle(msg);
        }

        public override void Update()
        {
            localPeer.ReceiveMessages(outgoingMessagePool, incomingMessagePool);

            if (localPeer.HasShutdown)
            {
                return;
            }

            UpdateMasterServerRegistration();

            UpdateStatistics();
        }

        protected void UpdateMasterServerRegistration()
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            var currentTime = DateTime.Now;
            var elapsedTime = currentTime - lastMasterServerRegistration;

            if (elapsedTime >= NetworkSessionSettings.MasterServerRegistrationInterval)
            {
                localPeer.RegisterWithMasterServer(outgoingMessagePool);

                lastMasterServerRegistration = currentTime;
            }
        }

        protected void UpdateStatistics()
        {
            var currentTime = DateTime.Now;
            int receivedBytes = localPeer.TotalReceivedBytes;
            int sentBytes = localPeer.TotalSentBytes;
            double elapsedSeconds = (currentTime - lastStatisticsUpdate).TotalSeconds;

            if (elapsedSeconds >= 1.0)
            {
                BytesPerSecondReceived = (int)Math.Round((receivedBytes - lastReceivedBytes) / elapsedSeconds);
                BytesPerSecondSent = (int)Math.Round((sentBytes - lastSentBytes) / elapsedSeconds);

                lastStatisticsUpdate = currentTime;
                lastReceivedBytes = receivedBytes;
                lastSentBytes = sentBytes;
            }
        }

        public override void Shutdown(string byeMessage)
        {
            if (localPeer.HasShutdown)
            {
                return;
            }

            localPeer.Disconnect(byeMessage);
        }
    }
}
