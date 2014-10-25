// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Microsoft.Xna.Framework
{
    class iOSGameViewController : UIViewController
    {
        iOSGamePlatform _platform;

        public iOSGameViewController(iOSGamePlatform platform)
        {
            if (platform == null)
                throw new ArgumentNullException("platform");
            _platform = platform;
            SupportedOrientations = 
                DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight 
                | DisplayOrientation.Portrait | DisplayOrientation.PortraitDown;
        }

        public event EventHandler<EventArgs> InterfaceOrientationChanged;

        public DisplayOrientation SupportedOrientations { get; set; }

        public override void LoadView()
        {
            RectangleF frame;
            if (ParentViewController != null && ParentViewController.View != null)
            {
                frame = new RectangleF(PointF.Empty, ParentViewController.View.Frame.Size);
            }
            else
            {
                UIScreen screen = UIScreen.MainScreen;

                // iOS 7 and older reverses width/height in landscape mode when reporting resolution,
                // iOS 8+ reports resolution correctly in all cases
                if (InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
                {
                    frame = new RectangleF(0, 0, Math.Max(screen.Bounds.Width, screen.Bounds.Height), Math.Min(screen.Bounds.Width, screen.Bounds.Height));
                }
                else
                {
                    frame = new RectangleF(0, 0, screen.Bounds.Width, screen.Bounds.Height);
                }
            }

            base.View = new iOSGameView(_platform, frame);

            // Need to set resize mask to ensure a view resize (which in iOS 8+ corresponds with a rotation) adjusts
            // the view and underlying CALayer correctly
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
        }

        public new iOSGameView View
        {
            get { return (iOSGameView)base.View; }
        }

        #region Autorotation for iOS 5 or older
        [Obsolete]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            DisplayOrientation supportedOrientations = OrientationConverter.Normalize(SupportedOrientations);
            var toOrientation = OrientationConverter.ToDisplayOrientation(toInterfaceOrientation);
            return (toOrientation & supportedOrientations) == toOrientation;
        }
        #endregion

        #region Autorotation for iOS 6 or newer
        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return OrientationConverter.ToUIInterfaceOrientationMask(this.SupportedOrientations);
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }
        #endregion

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            var handler = InterfaceOrientationChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #region Hide statusbar for iOS 7 or newer
        public override bool PrefersStatusBarHidden()
        {
            return _platform.Game.graphicsDeviceManager.IsFullScreen;
        }
        #endregion


        #region iOS 8 or newer

        bool _orientationChanged;
        UIInterfaceOrientation _prevOrientation;

        public override void ViewWillTransitionToSize(SizeF toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            SizeF oldSize = View.Bounds.Size;

            if (oldSize != toSize)
            {
                _orientationChanged = true;

                // At this point, the new orientation hasn't been set
                _prevOrientation = InterfaceOrientation;
            }

            base.ViewWillTransitionToSize(toSize, coordinator);
        } 

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            if (previousTraitCollection != null)
            {
                base.TraitCollectionDidChange(previousTraitCollection);

                // Not every trait change is related to rotation, so avoid unnecessarily updating
                if(_orientationChanged)
                {
                    // In iOS 8+ DidRotate is no longer called after a rotation
                    // But we need to notify iOSGamePlatform to update back buffer so we explicitly call it 
                    DidRotate(_prevOrientation);

                    _orientationChanged = false;
                }
            }
        }

        #endregion
    }
}
