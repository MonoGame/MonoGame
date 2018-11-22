using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkGamerIdComparer : IComparer<NetworkGamer>
    {
        public int Compare(NetworkGamer x, NetworkGamer y)
        {
            return x.id.CompareTo(y.id);
        }

        internal static IComparer<NetworkGamer> Instance = new NetworkGamerIdComparer();
    }

    public class NetworkGamer : Gamer
    {
        protected readonly NetworkSession session;
        internal readonly NetworkMachine machine;
        internal readonly byte id;
        internal bool isPrivateSlot;
        internal bool isReady;
        internal bool hasLeftSession;

        internal NetworkGamer(NetworkMachine machine, byte id, bool isPrivateSlot, bool isReady, string displayName, string gamertag)
            : base()
        {
            this.session = machine.session;
            this.machine = machine;
            this.id = id;
            this.isPrivateSlot = isPrivateSlot;
            this.isReady = isReady;

            this.DisplayName = displayName;
            this.Gamertag = gamertag;
        }

#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public NetworkMachine Machine { get { return machine; } }
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public byte Id { get { return id; } }
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public bool IsPrivateSlot { get { return isPrivateSlot; } }
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public bool HasLeftSession { get { return hasLeftSession; } }

        public bool HasVoice { get; } = false;
        public bool IsTalking { get; } = false;
        public bool IsMutedByLocalUser { get; } = false;

        public bool IsGuest { get { return machine.gamers[0] != this; } }
        public bool IsHost { get { return machine.isHost && id == 0; } }
        public bool IsLocal { get { return machine.isLocal; } }
        public TimeSpan RoundtripTime { get { return machine.roundtripTime; } }
        public NetworkSession Session { get { return session; } }

        public virtual bool IsReady
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(NetworkGamer));
                return isReady;
            }
            set
            {
                throw new InvalidOperationException("Gamer is not local");
            }
        }
    }
}
