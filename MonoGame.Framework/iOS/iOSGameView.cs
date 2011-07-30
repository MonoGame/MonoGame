using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
    public class iOSGameView : UIViewController
    {
		public iOSGameView()
		{
			
		}
		
        public override void ViewWillAppear(bool animated)
        {
			// Prepare to start the game appear
        	base.ViewWillAppear(animated);   
        }

        public override void ViewDidAppear(bool animated)
        {
			// Start the game loop
			base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
			// Prepare to pause the game
			base.ViewWillDisappear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
			// Pause the game loop
			base.ViewWillDisappear(animated);
        }

        // NSTimer will cause latency in rendering and may reduce frame rate.
        //iOS 4 onwards...
        // public CADisplaLink DisplayLinkWithTarget 

        // Called whenever the bounds of the view changes
       /* public override void LayoutSubviews()
        {
            // deleteFrameBuffer 

            // createFrameBuffer at the right size
        }*/
		
		// Copied from your game stuff Clancey
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{

			var manager = GameWindow.game.graphicsDeviceManager as GraphicsDeviceManager;
			Console.WriteLine(manager == null);
			if(manager == null)
				return true;
			DisplayOrientation supportedOrientations = manager.SupportedOrientations;
			switch(toInterfaceOrientation)
			{
			case UIInterfaceOrientation.LandscapeLeft :
				return (supportedOrientations & DisplayOrientation.LandscapeLeft) != 0;
			case UIInterfaceOrientation.LandscapeRight:
				return (supportedOrientations & DisplayOrientation.LandscapeRight) != 0;
			case UIInterfaceOrientation.Portrait:
				return (supportedOrientations & DisplayOrientation.Portrait) != 0;
			case UIInterfaceOrientation.PortraitUpsideDown :
				return (supportedOrientations & DisplayOrientation.PortraitUpsideDown) != 0;
			default :
				return false;
			}
			return true;
			
		}
    }
}
