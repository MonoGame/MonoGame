using System;

namespace Microsoft.Xna.Framework.GamerServices
{
    public struct LeaderboardIdentity
    {
        public int GameMode {get; set;}
        public LeaderboardKey Key {get; set;}

        public static LeaderboardIdentity Create(LeaderboardKey aKey)
        {
            return new LeaderboardIdentity() { Key = aKey};
        }

        public static LeaderboardIdentity Create(LeaderboardKey aKey, int aGameMode)
        {
            return new LeaderboardIdentity() { Key = aKey, GameMode = aGameMode};
        }
    }
}

