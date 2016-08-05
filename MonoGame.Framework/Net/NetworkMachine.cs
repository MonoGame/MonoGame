using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkMachine
    {
        internal IList<LocalNetworkGamer> localGamers;
        internal IList<NetworkGamer> gamers;

        internal NetworkMachine(bool isLocal, bool isHost)
        {
            this.IsPending = true;
            this.IsLocal = isLocal;
            this.IsHost = IsHost;
            this.localGamers = isLocal ? new List<LocalNetworkGamer>() : null;
            this.gamers = new List<NetworkGamer>();

            this.Gamers = new GamerCollection<NetworkGamer>(gamers);
        }

        internal bool IsPending { get; set; }
        internal bool IsLocal { get; }
        internal bool IsHost { get; }
        public GamerCollection<NetworkGamer> Gamers { get; }

        internal void AddLocalGamer(LocalNetworkGamer localGamer)
        {
            if (!IsLocal)
            {
                throw new InvalidOperationException("This NetworkMachine is remote");
            }

            localGamers.Add(localGamer);
            gamers.Add(localGamer);
        }

        internal void RemoveLocalGamer(LocalNetworkGamer localGamer)
        {
            if (!IsLocal)
            {
                throw new InvalidOperationException("This NetworkMachine is remote");
            }

            localGamers.Remove(localGamer);
            gamers.Remove(localGamer);
        }

        internal void AddRemoteGamer(NetworkGamer remoteGamer)
        {
            if (IsLocal)
            {
                throw new InvalidOperationException("This NetworkMachine is local");
            }

            gamers.Add(remoteGamer);
        }

        internal void RemoveRemoteGamer(NetworkGamer remoteGamer)
        {
            if (IsLocal)
            {
                throw new InvalidOperationException("This NetworkMachine is local");
            }

            gamers.Remove(remoteGamer);
        }

        public void RemoveFromSession()
        {
            // ObjectDisposedException if no gamers or gamer is no longer valid
            // InvalidOperationException if not called by host or the local machine (self)
            throw new NotImplementedException();
        }
    }
}
