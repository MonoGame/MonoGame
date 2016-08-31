using Microsoft.Xna.Framework.GamerServices;
using System;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkGamer : Gamer
    {
        protected bool isReady;

        internal NetworkGamer(NetworkMachine machine, string displayName, string gamertag, byte id, bool isPrivateSlot, bool isReady) : base()
        {
            this.Machine = machine;
            this.DisplayName = displayName;
            this.Gamertag = gamertag;
            this.Id = id;
            this.IsPrivateSlot = isPrivateSlot;

            this.isReady = isReady;
        }

        public NetworkMachine Machine { get; }
        public bool HasLeftSession { get { return false; } }
        public bool HasVoice { get { return false; } }
        public byte Id { get; }
        public bool IsGuest { get { return Machine.Gamers[0] != this; } }
        public bool IsHost { get { return Machine.IsHost && Machine.Gamers[0] == this; } }
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

                return isReady;
            }

            set { throw new InvalidOperationException("Gamer is not local"); }
        }

        internal void SetReadyState(bool state)
        {
            isReady = state;
        }

        public bool IsTalking { get { return false; } }

        public TimeSpan RoundtripTime
        {
            get
            {
                return TimeSpan.FromSeconds(Machine.connection.AverageRoundtripTime);
            }
        }

        public NetworkSession Session { get { return Machine.Session; } }
    }
}