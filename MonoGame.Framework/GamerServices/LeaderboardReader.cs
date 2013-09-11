using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.GamerServices
{
    public sealed class LeaderboardReader : IDisposable
    {
        public LeaderboardReader ()
        {
        }

        /*
        public IAsyncResult BeginPageDown(AsyncCallback aAsyncCallback, object aAsyncState)
        {
            throw new NotImplementedException ();
        }

        public IAsyncResult BeginPageUp(AsyncCallback aAsyncCallback, object aAsyncState)
        {
            throw new NotImplementedException ();
        }

        public LeaderboardReader EndPageDown(IAsyncResult result)
        {
            throw new NotImplementedException ();
        }

        public LeaderboardReader EndPageUp(IAsyncResult result)
        {
            throw new NotImplementedException ();
        }

        public static void BeginRead (LeaderboardIdentity id, SignedInGamer gamer, int leaderboardPageSize, AsyncCallback leaderboardReadCallback, SignedInGamer gamer2)
        {
            throw new NotImplementedException ();
        }

        public static LeaderboardReader EndRead(IAsyncResult result)
        {
            throw new NotImplementedException ();
        }

        public void PageDown()
        {
            throw new NotImplementedException ();
        }

        public void PageUp()
        {
            throw new NotImplementedException ();
        }
        */

        public bool CanPageDown {
            get;
            set;
        }

        public bool CanPageUp {
            get;
            set;
        }

        public IEnumerable<LeaderboardEntry> Entries {
            get;
            set;
        }

        #region IDisposable implementation

        public void Dispose ()
        {
            throw new NotImplementedException ();
        }

        #endregion
    }
}

