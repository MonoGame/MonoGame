using System;
using System.Collections.Generic;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkMachine
    {
        internal bool pending;
        internal List<NetworkGamer> localGamers = new List<NetworkGamer>();

        internal NetworkMachine(bool pending)
        {
            this.pending = pending;
            this.Gamers = new GamerCollection<NetworkGamer>(localGamers);
        }

        public GamerCollection<NetworkGamer> Gamers { get; }

        public void RemoveFromSession()
        {
            // ObjectDisposedException if no gamers or gamer is no longer valid
            // InvalidOperationException if not called by host or the local machine (self)
            throw new NotImplementedException();
        }
    }
}
