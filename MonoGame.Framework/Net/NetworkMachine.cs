using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Messages;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkMachine
    {
        internal IPeer peer;
        private IList<LocalNetworkGamer> localGamers;
        private IList<NetworkGamer> gamers;
        private bool beingRemovedThisFrame;

        internal ISet<NetworkMachine> hostPendingConnections;
        internal ISet<IPeerEndPoint> hostPendingAllowlistInsertions;

        internal NetworkMachine(NetworkSession session, IPeer peer, bool isLocal, bool isHost)
        {
            this.peer = peer;
            this.peer.Tag = this;
            this.localGamers = isLocal ? new List<LocalNetworkGamer>() : null;
            this.gamers = new List<NetworkGamer>();
            this.beingRemovedThisFrame = false;

            this.hostPendingConnections = null;
            this.hostPendingAllowlistInsertions = null;

            this.Session = session;
            this.HasLeftSession = false;
            this.IsFullyConnected = false;
            this.HasSentSessionStateToLocalMachine = false;
            this.HasAcknowledgedLocalMachine = false;
            this.IsLocal = isLocal;
            this.IsHost = isHost;
            this.LocalGamers = isLocal ? new GamerCollection<LocalNetworkGamer>(localGamers) : null;
            this.Gamers = new GamerCollection<NetworkGamer>(gamers);
        }

        internal NetworkSession Session { get; }
        internal bool HasLeftSession { get; set; }
        internal bool IsFullyConnected { get; set; }
        internal bool HasSentSessionStateToLocalMachine { get; set; }
        internal bool HasAcknowledgedLocalMachine { get; set; }
        internal bool IsLocal { get; }
        internal bool IsHost { get; }
        internal GamerCollection<LocalNetworkGamer> LocalGamers { get; }
        public GamerCollection<NetworkGamer> Gamers { get; }

        internal LocalNetworkGamer FindLocalGamerBySignedInGamer(SignedInGamer signedInGamer)
        {
            if (!IsLocal)
            {
                return null;
            }

            foreach (LocalNetworkGamer localGamer in localGamers)
            {
                if (localGamer.SignedInGamer == signedInGamer)
                {
                    return localGamer;
                }
            }

            return null;
        }

        internal void AddGamer(NetworkGamer gamer)
        {
            if (IsLocal != gamer.IsLocal)
            {
                throw new InvalidOperationException("Local NetworkMachine can not add remote gamer or vice versa");
            }

            if (IsLocal)
            {
                LocalNetworkGamer localGamer = gamer as LocalNetworkGamer;

                if (localGamer == null)
                {
                    throw new InvalidOperationException("Non-remote gamer can not be cast to LocalNetworkGamer");
                }

                localGamers.Add(localGamer);
            }

            gamers.Add(gamer);
        }

        internal void RemoveGamer(NetworkGamer gamer)
        {
            if (IsLocal != gamer.IsLocal)
            {
                throw new InvalidOperationException("Local NetworkMachine can not remove remote gamer or vice versa");
            }

            if (IsLocal)
            {
                localGamers.Remove(gamer as LocalNetworkGamer);
            }

            gamers.Remove(gamer);
        }

        public void RemoveFromSession()
        {
            if (!IsLocal && !Session.IsHost)
            {
                throw new InvalidOperationException("Can only be called by the host or the owner of the machine");
            }
            if (HasLeftSession)
            {
                throw new ObjectDisposedException("NetworkMachine");
            }

            if (IsLocal)
            {
                Session.End(NetworkSessionEndReason.Disconnected);
            }
            else
            {
                if (!beingRemovedThisFrame)
                {
                    Session.InternalMessages.RemoveMachine.Create(this, null);

                    beingRemovedThisFrame = true;
                }
            }
        }
    }
}
