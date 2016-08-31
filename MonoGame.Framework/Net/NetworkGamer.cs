using Microsoft.Xna.Framework.GamerServices;
using System;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkGamer : Gamer
    {
        protected bool isReady;

        internal NetworkGamer(bool isReady, string displayName, string gamertag, byte id, bool isPrivateSlot, NetworkMachine machine) : base()
        {
            this.isReady = isReady;

            this.DisplayName = displayName;
            this.Gamertag = gamertag;
            this.HasLeftSession = false;
            this.Id = id;
            this.IsPrivateSlot = isPrivateSlot;
            this.Machine = machine;
        }

        public bool HasLeftSession { get; }
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
        public NetworkMachine Machine { get; }

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