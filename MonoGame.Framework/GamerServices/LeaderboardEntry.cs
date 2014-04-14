using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.GamerServices
{
    [DataContract]
    public sealed class LeaderboardEntry
    {

        [DataMember]
        public long Rating { get; set; }

        [DataMember]
        public PropertyDictionary Columns { get; internal set; }

        [DataMember]
        public Gamer Gamer { get; internal set; }

        public LeaderboardEntry ()
        {
        }
    }
}

