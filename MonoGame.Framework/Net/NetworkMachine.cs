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

            this.IsPending = true;
            this.IsLocal = connection == null;
            this.IsHost = isHost;

            this.connection = connection;
            this.localGamers = this.IsLocal ? new List<LocalNetworkGamer>() : null;
            this.gamers = new List<NetworkGamer>();

            this.LocalGamers = this.IsLocal ? new GamerCollection<LocalNetworkGamer>(localGamers) : null;
            this.Gamers = new GamerCollection<NetworkGamer>(gamers);
        }

        internal NetworkSession Session { get; }
        internal bool IsPending { get; set; }
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

        internal void RemoveGamersLocally()
        {
            for (int i = gamers.Count - 1; i >= 0; i--)
            {
                Session.RemoveGamer(gamers[i]);
            }
        }

        public void RemoveFromSession()
        {
            // ObjectDisposedException if no gamers or gamer is no longer valid
            // InvalidOperationException if not called by host or the local machine (self)
            throw new NotImplementedException();
        }
    }
}
