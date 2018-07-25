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
    }

    public class NetworkGamer : Gamer
    {
        internal static IComparer<NetworkGamer> Comparer = new NetworkGamerIdComparer();

        protected bool ready;

        internal NetworkGamer(NetworkMachine machine, string displayName, string gamertag, byte id, bool isPrivateSlot, bool isReady) : base()
        {
            this.Machine = machine;
            this.DisplayName = displayName;
            this.Gamertag = gamertag;
            this.HasLeftSession = false;
            this.Id = id;
            this.IsPrivateSlot = isPrivateSlot;

            this.ready = isReady;
        }

        public NetworkMachine Machine { get; }
        public bool HasLeftSession { get; internal set; }
        public bool HasVoice { get { return false; } }
        public byte Id { get; }
        public bool IsGuest { get { return Machine.gamers[0] != this; } }
        public bool IsHost { get { return Machine.IsHost && Machine.gamers[0] == this; } }
        public bool IsLocal { get { return Machine.IsLocal; } }
        public bool IsMutedByLocalUser { get { return false; } }
        public bool IsPrivateSlot { get; internal set; }

        public virtual bool IsReady
        {
            get
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Gamer disposed");
                }

                return ready;
            }

            set { throw new InvalidOperationException("Gamer is not local"); }
        }

        internal void SetReadyState(bool state)
        {
            ready = state;
        }

        public bool IsTalking { get { return false; } }

        public TimeSpan RoundtripTime
        {
            get { return Machine.peer.RoundtripTime; }
        }

        public NetworkSession Session { get { return Machine.Session; } }
    }
}
