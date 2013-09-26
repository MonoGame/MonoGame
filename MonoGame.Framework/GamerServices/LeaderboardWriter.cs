using System;

namespace Microsoft.Xna.Framework.GamerServices
{
    public sealed class LeaderboardWriter : IDisposable
    {
        public LeaderboardWriter ()
        {
        }

        /*
        public LeaderboardEntry GetLeaderboard(LeaderboardIdentity aLeaderboardIdentity)
        {
            throw new NotImplementedException ();
        }
        */

        #region IDisposable implementation

        void IDisposable.Dispose ()
        {
            throw new NotImplementedException ();
        }

        #endregion
    }
}

