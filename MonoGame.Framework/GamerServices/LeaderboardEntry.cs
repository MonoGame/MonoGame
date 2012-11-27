using System;

namespace Microsoft.Xna.Framework.GamerServices
{
    public sealed class LeaderboardEntry
    {

        public long Rating  {get; set;}
        public PropertyDictionary Columns {get; internal set;}
        public Gamer Gamer {get; internal set;}

        public LeaderboardEntry ()
        {
        }
    }
}

