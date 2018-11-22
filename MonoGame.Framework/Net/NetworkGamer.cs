using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkGamerIdComparer : IComparer<NetworkGamer>
    {
        public int Compare(NetworkGamer x, NetworkGamer y)
        {
            return x.Id.CompareTo(y.Id);
        }

        internal static IComparer<NetworkGamer> Instance = new NetworkGamerIdComparer();
    }

    public class NetworkGamer : Gamer
    {
        protected readonly NetworkSession session;
        protected readonly NetworkMachine machine;
        protected readonly byte id;
        protected readonly bool isPrivateSlot;

        protected bool ready;

        internal NetworkGamer(NetworkMachine machine, byte id, bool isPrivateSlot, string displayName, string gamertag, bool isReady)
            : base()
        {
            this.session = machine.session;
            this.machine = machine;
            this.id = id;
            this.isPrivateSlot = isPrivateSlot;

            this.DisplayName = displayName;
            this.Gamertag = gamertag;

            this.ready = isReady;

        }

#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public NetworkMachine Machine { get { return machine; } }
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public byte Id { get { return id; } }
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public bool IsPrivateSlot { get { return isPrivateSlot; } }

        public bool HasLeftSession { get; internal set; } = false;
        public bool HasVoice { get; } = false;
        public bool IsTalking { get; } = false;
        public bool IsMutedByLocalUser { get; } = false;

        public bool IsGuest { get { return Machine.Gamers[0] != this; } }
        public bool IsHost { get { return Machine.isHost && id == 0; } }
        public bool IsLocal { get { return Machine.isLocal; } }
        public TimeSpan RoundtripTime { get { return Machine.roundtripTime; } }
        public NetworkSession Session { get { return session; } }

        public virtual bool IsReady
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(NetworkGamer));
                return ready;
            }
            set
            {
                throw new InvalidOperationException("Gamer is not local");
            }
        }

        internal void SetReadyState(bool state)
        {
            ready = state;
        }
    }
}
