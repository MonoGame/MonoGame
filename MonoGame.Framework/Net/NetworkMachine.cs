using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class NetworkMachine
    {
        private readonly NetworkSession session;
        private readonly bool isLocal;
        private readonly bool isHost;
        private readonly byte id;

        internal readonly List<NetworkGamer> gamers = new List<NetworkGamer>();

        private TimeSpan roundtripTime = TimeSpan.Zero; // TODO
        private bool beingRemoved = false;

        internal NetworkMachine(NetworkSession session, bool isLocal, bool isHost, byte id)
        {
            this.session = session;
            this.isLocal = isLocal;
            this.isHost = isHost;
            this.id = id;
            this.Gamers = new GamerCollection<NetworkGamer>(new List<NetworkGamer>(), gamers);
        }

        internal NetworkSession Session { get { return session; } }
        internal bool IsLocal { get { return isLocal; } }
        internal bool IsHost { get { return isHost; } }
        internal byte Id { get { return id; } }
        internal TimeSpan RoundtripTime { get { return roundtripTime; } }

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
