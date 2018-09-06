using System;

namespace Microsoft.Xna.Framework.Net
{
    public class GamerLeftEventArgs : EventArgs
    {
        public GamerLeftEventArgs(NetworkGamer gamer)
        {
            this.Gamer = gamer;
        }

        public NetworkGamer Gamer { get; }
    }
}