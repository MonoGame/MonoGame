using Microsoft.Xna.Framework.GamerServices;
using System;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkGamer : Gamer
    {
        protected bool isReady = false;

        internal NetworkGamer(string displayName, string gamertag, byte id, bool isGuest, bool isHost, bool isLocal, bool isPrivateSlot, NetworkMachine machine, NetworkSession session) : base()
        {
            this.DisplayName = displayName;
            this.Gamertag = gamertag;

            this.HasLeftSession = false;
            this.Id = id;
            this.IsGuest = isGuest;
            this.IsHost = isHost;
            this.IsLocal = isLocal;
            this.IsPrivateSlot = isPrivateSlot;
            this.Machine = machine;
            this.RoundtripTime = TimeSpan.Zero;
            this.Session = session;
        }

        public bool HasLeftSession { get; }
        public bool HasVoice { get { return false; } }
        public byte Id { get; }
        public bool IsGuest { get; internal set; }
        public bool IsHost { get; internal set; }
        public bool IsLocal { get; }
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

                return isReady;
            }

            set { throw new InvalidOperationException("Gamer is not local"); }
        }

        internal void SetReadyState(bool state)
        {
            if (IsLocal)
            {
                throw new InvalidOperationException("Gamer is not remote");
            }

            isReady = state;
        }

        public bool IsTalking { get { return false; } }
        public NetworkMachine Machine { get; }
        public TimeSpan RoundtripTime { get; }
        public NetworkSession Session { get; }
    }
}