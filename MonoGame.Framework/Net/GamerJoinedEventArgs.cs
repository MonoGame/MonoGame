using System;

namespace Microsoft.Xna.Framework.Net
{
    public class GamerJoinedEventArgs : EventArgs
    {
        public GamerJoinedEventArgs(NetworkGamer gamer)
        {
            this.Gamer = gamer;
        }

        public NetworkGamer Gamer { get; }
    }
}