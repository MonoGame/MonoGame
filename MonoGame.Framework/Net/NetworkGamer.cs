using Microsoft.Xna.Framework.GamerServices;
using System;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkGamer : Gamer
    {
        internal NetworkGamer(bool isLocal, byte id) : base()
        {
            this.IsLocal = isLocal;
            this.Id = id;
        }

        public bool HasLeftSession { get; }
        public bool HasVoice { get { return false; } }
        public byte Id { get; }
        public bool IsGuest { get { return false; } }
        public bool IsHost { get; }
        public bool IsLocal { get; }
        public bool IsMutedByLocalUser { get { return false; } }
        public bool IsPrivateSlot { get; }
        public bool IsReady { get; }
        public bool IsTalking { get { return false; } }
        public NetworkMachine Machine { get; }
        public TimeSpan RoundtripTime { get; }
        public NetworkSession Session { get; }
    }
}