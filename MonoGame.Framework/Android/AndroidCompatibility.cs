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
		/// Because the Kindle Fire devices default orientation is fliped by 180 degrees from all the other android devices
		/// on the market we need to do some special processing to make sure that LandscapeLeft is the correct way round.
		/// This list contains all the Build.Model strings of the effected devices, it should be added to if and when
		/// more devices exhibit the same issues.
		/// </summary>
        private static readonly string[] Kindles = new[] { "KFTT", "KFJWI", "KFJWA", "KFSOWI", "KFTHWA", "KFTHWI", "KFAPWA", "KFAPWI" };

        static AndroidCompatibility()
        {
			FlipLandscape = Kindles.Contains(Build.Model);
        }

		public static bool FlipLandscape { get; private set;} 

    }
}
