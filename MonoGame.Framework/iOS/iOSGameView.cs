using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.UIKit;

using OpenTK;
using OpenTK.Platform;

namespace Microsoft.Xna.Framework
{
    public class iOSGameView : UIViewController//, IGameWindow
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

		#region IGameWindow implementation
		public event EventHandler<EventArgs> Load;

		public event EventHandler<EventArgs> Unload;

		public event EventHandler<FrameEventArgs> UpdateFrame;

		public event EventHandler<FrameEventArgs> RenderFrame;

		public void Run ()
		{
			throw new NotImplementedException ();
		}

		public void Run (double updateRate)
		{
			throw new NotImplementedException ();
		}

		public void MakeCurrent ()
		{
			throw new NotImplementedException ();
		}

		public void SwapBuffers ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region INativeWindow implementation
		public event EventHandler<EventArgs> Move;

		public event EventHandler<EventArgs> Resize;

		//public event EventHandler<CancelEventArgs> Closing;

		public event EventHandler<EventArgs> Closed;

		public event EventHandler<EventArgs> Disposed;

		public event EventHandler<EventArgs> TitleChanged;

		public event EventHandler<EventArgs> VisibleChanged;

		public event EventHandler<EventArgs> FocusedChanged;

		public event EventHandler<EventArgs> WindowBorderChanged;

		public event EventHandler<EventArgs> WindowStateChanged;

		public event EventHandler<KeyPressEventArgs> KeyPress;

		public void Close ()
		{
			throw new NotImplementedException ();
		}

		public void ProcessEvents ()
		{
			throw new NotImplementedException ();
		}

		public System.Drawing.Point PointToClient (System.Drawing.Point point)
		{
			throw new NotImplementedException ();
		}

		public System.Drawing.Point PointToScreen (System.Drawing.Point point)
		{
			throw new NotImplementedException ();
		}

		public bool Focused {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool Exists {
			get {
				throw new NotImplementedException ();
			}
		}

		public IWindowInfo WindowInfo {
			get {
				throw new NotImplementedException ();
			}
		}

		public OpenTK.WindowState WindowState {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public OpenTK.WindowBorder WindowBorder {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public System.Drawing.Rectangle Bounds {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public System.Drawing.Point Location {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public System.Drawing.Size Size {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int X {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int Y {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int Width {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int Height {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public System.Drawing.Rectangle ClientRectangle {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public System.Drawing.Size ClientSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion
    }
}
