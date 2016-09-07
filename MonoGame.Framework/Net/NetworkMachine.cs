using System;
using System.Collections.Generic;

using Lidgren.Network;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkMachine
    {
        internal NetConnection connection;
        private IList<LocalNetworkGamer> localGamers;
        private IList<NetworkGamer> gamers;

        internal NetworkMachine(NetworkSession session, NetConnection connection, bool isHost)
        {
            this.Session = session;

            this.HasLeftSession = false;
            this.IsFullyConnected = false;
            this.HasAcknowledgedLocalMachine = false;
            this.IsLocal = connection == null;
            this.IsHost = isHost;

            this.connection = connection;
            this.localGamers = this.IsLocal ? new List<LocalNetworkGamer>() : null;
            this.gamers = new List<NetworkGamer>();

            this.LocalGamers = this.IsLocal ? new GamerCollection<LocalNetworkGamer>(localGamers) : null;
            this.Gamers = new GamerCollection<NetworkGamer>(gamers);

            if (this.connection != null)
            {
                this.connection.Tag = this;
            }
        }

        internal NetworkSession Session { get; }
        internal bool HasLeftSession { get; set; }
        internal bool IsFullyConnected { get; set; }
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
                if (!Session.forceRemovedMachines.Contains(this))
                {
                    Session.forceRemovedMachines.Add(this);
                }
            }
        }
    }
}
