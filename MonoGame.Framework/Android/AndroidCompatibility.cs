using System.Linq;
using Android.OS;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Properties that change from how XNA works by default
    /// </summary>
    public static class AndroidCompatibility
    {
		/// <summary>
		/// Becaue the Kindle Fire devices default orientation is fliped by 180 degrees from all the other android devices
		/// on the market we need to do some special processing to make sure that LandscapeLeft is the correct way round.
		/// This list contains all the Build.Model strings of the effected devices, it should be added to if and when
		/// more devices exxhibit the same issues
		/// </summary>
		private static readonly string[] Kindles = new[] { "KFTT", "KFJWI", "KFJWA" };

        public enum ESVersions
        {
            v1_1,
            v2_0
        }

        static AndroidCompatibility()
        {
            ScaleImageToPowerOf2 = true;
            ESVersion = ESVersions.v2_0;
			FlipLandscape = Kindles.Contains(Build.Model);
        }

        public static bool ScaleImageToPowerOf2 { get; set; }
        public static ESVersions ESVersion { get; set; }

		public static bool FlipLandscape { get; private set;} 

    }
}
