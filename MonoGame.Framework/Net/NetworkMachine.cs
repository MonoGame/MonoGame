using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkMachine
    {
        internal bool isPending;
        internal bool isLocal;
        internal List<LocalNetworkGamer> localGamers;
        internal List<NetworkGamer> gamers;

        internal NetworkMachine(bool isPending, bool isLocal)
        {
            this.isPending = isPending;
            this.isLocal = isLocal;
            this.localGamers = isLocal ? new List<LocalNetworkGamer>() : null;
            this.gamers = new List<NetworkGamer>();

            this.Gamers = new GamerCollection<NetworkGamer>(gamers);
        }

        public GamerCollection<NetworkGamer> Gamers { get; }

        internal void AddLocalGamer(LocalNetworkGamer localGamer)
        {
            if (!isLocal)
            {
                throw new InvalidOperationException("This NetworkMachine is remote");
            }

            localGamers.Add(localGamer);
            gamers.Add(localGamer);
        }

        internal void AddRemoteGamer(NetworkGamer remoteGamer)
        {
            if (isLocal)
            {
                throw new InvalidOperationException("This NetworkMachine is local");
            }

            gamers.Add(remoteGamer);
        }

        public void RemoveFromSession()
        {
            // ObjectDisposedException if no gamers or gamer is no longer valid
            // InvalidOperationException if not called by host or the local machine (self)
            throw new NotImplementedException();
        }
    }
}
