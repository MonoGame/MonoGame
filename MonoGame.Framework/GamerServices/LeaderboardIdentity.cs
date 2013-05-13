using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.GamerServices
{
    [DataContract]
    public struct LeaderboardIdentity
    {
        [DataMember]
        public int GameMode { get; set; }

        [DataMember]
        public LeaderboardKey Key { get; set; }

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

