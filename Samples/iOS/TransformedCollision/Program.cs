using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Microsoft.Xna;

namespace TransformedCollision
{
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{

		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			using (TransformedCollisionGame game = new TransformedCollisionGame()) {
				game.Run ();
			}
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}
