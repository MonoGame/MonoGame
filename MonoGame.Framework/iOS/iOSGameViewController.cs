#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion
using System;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Microsoft.Xna.Framework {
	class iOSGameViewController : UIViewController {
		iOSGamePlatform _platform;

		public iOSGameViewController (iOSGamePlatform platform)
		{
			if (platform == null)
				throw new ArgumentNullException ("platform");
			_platform = platform;
			SupportedOrientations = DisplayOrientation.Default;
		}

		public event EventHandler<EventArgs> InterfaceOrientationChanged;

		public DisplayOrientation SupportedOrientations { get; set; }

		public override void LoadView ()
		{
			RectangleF frame;
			if (ParentViewController != null && ParentViewController.View != null) {
				frame = new RectangleF(PointF.Empty, ParentViewController.View.Frame.Size);
			} else {
				UIScreen screen = UIScreen.MainScreen;
				if (InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
				    InterfaceOrientation == UIInterfaceOrientation.LandscapeRight) {
					frame = new RectangleF(0, 0, screen.Bounds.Height, screen.Bounds.Width);
				} else {
					frame = new RectangleF(0, 0, screen.Bounds.Width, screen.Bounds.Height);
				}
			}

			base.View = new iOSGameView (_platform, frame);
		}

		public new iOSGameView View {
			get { return (iOSGameView) base.View; }
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			DisplayOrientation supportedOrientations;
			if (SupportedOrientations == DisplayOrientation.Default) {
				supportedOrientations = GetDefaultSupportedOrientations();
			} else {
				supportedOrientations = OrientationConverter.Normalize (SupportedOrientations);
			}
			var toOrientation = OrientationConverter.ToDisplayOrientation (toInterfaceOrientation);
			return (toOrientation & supportedOrientations) == toOrientation;
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);

			var handler = InterfaceOrientationChanged;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
		}

		private DisplayOrientation? _defaultSupportedOrientations;
		/// <summary>
		/// Gets the default supported orientations as specified in the
		/// Info.plist for the application.
		/// </summary>
		private DisplayOrientation GetDefaultSupportedOrientations ()
		{
			if (_defaultSupportedOrientations.HasValue)
				return _defaultSupportedOrientations.Value;

			var key = new NSString ("UISupportedInterfaceOrientations");
			NSObject arrayObj;
			if (!NSBundle.MainBundle.InfoDictionary.TryGetValue (key, out arrayObj)) {
				_defaultSupportedOrientations = OrientationConverter.Normalize (DisplayOrientation.Default);
				return _defaultSupportedOrientations.Value;
			}

			DisplayOrientation orientations = (DisplayOrientation)0;
			var supportedOrientationStrings = NSArray.ArrayFromHandle<NSString> (arrayObj.Handle);

			foreach (var orientationString in supportedOrientationStrings) {
				var s = (string)orientationString;
				if (!s.StartsWith("UIInterfaceOrientation"))
					continue;
				s = s.Substring ("UIInterfaceOrientation".Length);

				try {
					var supportedOrientation = (UIInterfaceOrientation)Enum.Parse(
						typeof(UIInterfaceOrientation), s);
					orientations |= OrientationConverter.ToDisplayOrientation (supportedOrientation);
				} catch {
				}
			}

			if (orientations == (DisplayOrientation)0)
				orientations = OrientationConverter.Normalize (DisplayOrientation.Default);

			_defaultSupportedOrientations = orientations;
			return _defaultSupportedOrientations.Value;
		}
	}
}
