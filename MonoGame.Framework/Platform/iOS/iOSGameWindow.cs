// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using UIKit;

namespace Microsoft.Xna.Framework {
	class iOSGameWindow : GameWindow {
		private readonly iOSGameViewController _viewController;

		public iOSGameWindow (iOSGameViewController viewController)
		{
			if (viewController == null)
				throw new ArgumentNullException("viewController");
			_viewController = viewController;
            _viewController.InterfaceOrientationChanged += HandleInterfaceOrientationChanged;
		}

        void HandleInterfaceOrientationChanged (object sender, EventArgs e)
        {
            OnOrientationChanged();
        }

		#region GameWindow Members

		public override bool AllowUserResizing {
			get { return false; }
			set { /* Do nothing. */ }
		}

		public override Rectangle ClientBounds {
			get {
				var bounds = _viewController.View.Bounds;
                var scale = _viewController.View.ContentScaleFactor;

                // TODO: Calculate this only when dirty.
                if (_viewController is iOSGameViewController)
                {

                    var currentOrientation = CurrentOrientation;

                    int width;
                    int height;

                    if (currentOrientation == DisplayOrientation.LandscapeLeft || 
                        currentOrientation == DisplayOrientation.LandscapeRight)
                    {
                        width = (int)Math.Max(bounds.Width, bounds.Height);
                        height = (int)Math.Min(bounds.Width, bounds.Height);

                    }
                    else
                    {
                        width = (int)Math.Min(bounds.Width, bounds.Height);
                        height = (int)Math.Max(bounds.Width, bounds.Height);
                    }

                    width *= (int)scale;
                    height *= (int)scale;

                    return new Rectangle( (int)(bounds.X * scale), (int)(bounds.Y * scale), width, height);
                }

				return new Rectangle(
                    (int)(bounds.X * scale), (int)(bounds.Y * scale),
                    (int)(bounds.Width * scale), (int)(bounds.Height * scale));
			}
		}

		public override DisplayOrientation CurrentOrientation {
			get {
                #if TVOS
                return DisplayOrientation.LandscapeLeft;
                #else
				return OrientationConverter.ToDisplayOrientation(_viewController.InterfaceOrientation);
                #endif
			}
		}

		public override IntPtr Handle {
			get {
				// TODO: Verify that View.Handle is a sensible
				//       value to return here.
				return _viewController.View.Handle;
			}
		}

		public override string ScreenDeviceName {
			get {
				var screen = _viewController.View.Window.Screen;
				if (screen == UIScreen.MainScreen)
					return "Main Display";
				else
					return "External Display";
			}
		}

		public override void BeginScreenDeviceChange (bool willBeFullScreen)
		{
			throw new NotImplementedException ();
		}

		public override void EndScreenDeviceChange (
			string screenDeviceName, int clientWidth, int clientHeight)
		{
			throw new NotImplementedException ();
		}

		internal protected override void SetSupportedOrientations (DisplayOrientation orientations)
		{
			_viewController.SupportedOrientations = orientations;
		}

		protected override void SetTitle (string title)
		{
			_viewController.Title = title;
		}

		#endregion GameWindow Members
	}
}