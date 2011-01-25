#region Using Statements
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using XnaTouch;
using XnaTouch.Framework.Media;
#endregion

namespace MemoryMadness
{
    [Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		public override void FinishedLaunching (UIApplication app)
		{
			using (MemoryMadnessGame game = new MemoryMadnessGame())
            {
                game.Run();
            }
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}
