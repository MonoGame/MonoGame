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
#endregion License

using System;
using System.Collections.Generic;
using System.Drawing;

using Foundation;
using UIKit;

namespace Microsoft.Xna.Framework {
	class KeyboardInputViewController : UIViewController {
		private readonly string _titleText;
		private readonly string _descriptionText;
		private readonly string _defaultText;
		private readonly bool _usePasswordMode;
        private UIViewController _gameViewController;

		public KeyboardInputViewController (
			string titleText, string descriptionText, string defaultText, bool usePasswordMode, UIViewController gameViewController)
		{
			_titleText = titleText;
			_descriptionText = descriptionText;
			_defaultText = defaultText;
			_usePasswordMode = usePasswordMode;
            _gameViewController = gameViewController;
		}

		private readonly List<NSObject> _keyboardObservers = new List<NSObject> ();
		public override void LoadView ()
		{
			var view = new KeyboardInputView (new RectangleF (0, 0, 240, 320));
			view.Title = _titleText;
			view.Description = _descriptionText;
			view.Text = _defaultText;
			view.UsePasswordMode = _usePasswordMode;

			view.ActivateFirstField ();

			base.View = view;

			_keyboardObservers.Add (
				NSNotificationCenter.DefaultCenter.AddObserver(
					UIKeyboard.DidShowNotification, Keyboard_DidShow));
			_keyboardObservers.Add (
				NSNotificationCenter.DefaultCenter.AddObserver(
					UIKeyboard.WillHideNotification, Keyboard_WillHide));
		}

		public new KeyboardInputView View {
			get { return (KeyboardInputView) base.View; }
		}

        [Obsolete]
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();

			NSNotificationCenter.DefaultCenter.RemoveObservers (_keyboardObservers);
			_keyboardObservers.Clear ();

            _gameViewController = null;
		}

		private void Keyboard_DidShow(NSNotification notification)
		{
			var keyboardSize = UIKeyboard.FrameBeginFromNotification (notification).Size;

			if (InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft ||
			    InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
            {
                var tmpkeyboardSize = keyboardSize;
				keyboardSize.Width = (nfloat)Math.Max(tmpkeyboardSize.Height, tmpkeyboardSize.Width);
				keyboardSize.Height = (nfloat)Math.Min(tmpkeyboardSize.Height, tmpkeyboardSize.Width);
			}

			var view = (KeyboardInputView)View;
			var contentInsets = new UIEdgeInsets(0f, 0f, keyboardSize.Height, 0f);
			view.ContentInset = contentInsets;
			view.ScrollIndicatorInsets = contentInsets;

			view.ScrollActiveFieldToVisible ();
		}

		private void Keyboard_WillHide(NSNotification notification)
		{
			var view = (KeyboardInputView)View;
			view.ContentInset = UIEdgeInsets.Zero;
			view.ScrollIndicatorInsets = UIEdgeInsets.Zero;
		}

        #region Autorotation for iOS 5 or older
        [Obsolete]
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
            var requestedOrientation = OrientationConverter.ToDisplayOrientation(toInterfaceOrientation);
            var supportedOrientations = (_gameViewController as iOSGameViewController).SupportedOrientations;

            return (supportedOrientations & requestedOrientation) != 0;
		}
        #endregion

        #region Autorotation for iOS 6 or newer
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
        {
            return OrientationConverter.ToUIInterfaceOrientationMask((_gameViewController as iOSGameViewController).SupportedOrientations);
        }
        
        public override bool ShouldAutorotate ()
        {
            return true;
        }
        
        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation ()
        {
            return _gameViewController.PreferredInterfaceOrientationForPresentation();
        }
        #endregion

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
			View.LayoutSubviews ();
		}
	}

	struct PaddingF {
		public float Left;
		public float Top;
		public float Right;
		public float Bottom;

		public float Horizontal {
			get { return Left + Right; }
		}

		public float Vertical {
			get { return Top + Bottom; }
		}

		public PaddingF (float all)
		{
			Left = Top = Right = Bottom = all;
		}

		public PaddingF (float left, float top, float right, float bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}
}

