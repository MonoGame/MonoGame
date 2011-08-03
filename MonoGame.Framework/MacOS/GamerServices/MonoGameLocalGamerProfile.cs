using System;
using System.Globalization;

namespace Microsoft.Xna.Framework.GamerServices
{
	[Serializable()]
	internal class MonoGameLocalGamerProfile
	{
		Guid playerGuid;
		
		internal MonoGameLocalGamerProfile ()
		{
			playerGuid = Guid.NewGuid();
		}

		#region Properties
		
		internal Guid PlayerInternalIdentifier
		{
			get { return playerGuid; }
			set { playerGuid = value; }
		}
		
		internal string DisplayName {
			get;
			set;
		}

		internal string Gamertag {
			get;
			set;
		}


		internal byte[] GamerPicture {
			get;
			set;
		}
		
		internal int GamerScore {
			get;
			set;
		}

		internal GamerZone GamerZone {
			get;
			set;
		}

		internal string Motto {
			get;
			set;
		}

		internal RegionInfo Region {
			get;
			set;
		}

		internal float Reputation {
			get;
			set;
		}

		internal int TitlesPlayed {
			get;
			set;
		}

		internal int TotalAchievements {
			get;
			set;
		}		
		#endregion
	}
}

