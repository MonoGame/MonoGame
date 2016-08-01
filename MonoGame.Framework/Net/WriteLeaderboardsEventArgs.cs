using System;

namespace Microsoft.Xna.Framework.Net
{
    public class WriteLeaderboardsEventArgs : EventArgs
    {
        internal WriteLeaderboardsEventArgs(NetworkGamer gamer, bool isLeaving)
        {
            this.Gamer = gamer;
            this.IsLeaving = isLeaving;
        }

        public NetworkGamer Gamer { get; }
        public bool IsLeaving { get; }
    }
}