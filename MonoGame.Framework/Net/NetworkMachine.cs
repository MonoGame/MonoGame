using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class NetworkMachine
    {
        internal readonly NetworkSession session;
        internal readonly bool isLocal;
        internal readonly bool isHost;
        internal readonly byte id;

        internal readonly List<NetworkGamer> gamers = new List<NetworkGamer>();

        private bool beingRemoved = false;
        internal TimeSpan roundtripTime = TimeSpan.Zero;
        internal int currentGamerIdRequests = 0;

        internal NetworkMachine(NetworkSession session, bool isLocal, bool isHost, byte id)
        {
            this.session = session;
            this.isLocal = isLocal;
            this.isHost = isHost;
            this.id = id;
            this.Gamers = new GamerCollection<NetworkGamer>(new List<NetworkGamer>(), gamers);
        }

        public GamerCollection<NetworkGamer> Gamers { get; }

        public void RemoveFromSession()
        {
            if (session.IsDisposed) throw new ObjectDisposedException(nameof(NetworkSession));
            if (beingRemoved) throw new ObjectDisposedException(nameof(NetworkMachine));
            if (!isLocal && !session.IsHost)
            {
                throw new InvalidOperationException("Can only be called by the host or the owner of the machine");
            }

            if (isLocal)
            {
                session.End(NetworkSessionEndReason.Disconnected);
            }
            else
            {
                session.DisconnectMachine(this, NetworkSessionEndReason.RemovedByHost);
            }
            beingRemoved = true;
        }
    }
}
