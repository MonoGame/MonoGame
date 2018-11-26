using System;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class WriteLeaderboardsEventArgs : EventArgs
    {
        internal WriteLeaderboardsEventArgs(NetworkGamer gamer, bool isLeaving)
        {
            this.Gamer = gamer;
            this.IsLeaving = isLeaving;
        }

        public NetworkGamer Gamer { get; private set; }
        public bool IsLeaving { get; private set; }
    }
}
