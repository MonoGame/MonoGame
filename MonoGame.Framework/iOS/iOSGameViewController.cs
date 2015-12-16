// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;

using UIKit;
using Foundation;
using CoreGraphics;

namespace Microsoft.Xna.Framework
{
    class iOSGameViewController : 
    #if TVOS
        GameController.GCEventViewController
    #else
        UIViewController
    #endif
    {
        iOSGamePlatform _platform;
        #if TVOS
        IPlatformBackButton platformBackButton;
        #endif

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
			CGRect frame;
            if (ParentViewController != null && ParentViewController.View != null)
            {
				frame = new CGRect(CGPoint.Empty, ParentViewController.View.Frame.Size);
            }
            else
            {
                UIScreen screen = UIScreen.MainScreen;

                #if !TVOS
                // iOS 7 and older reverses width/height in landscape mode when reporting resolution,
                // iOS 8+ reports resolution correctly in all cases
                if (InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight)
                {
					frame = new CGRect(0, 0, (nfloat)Math.Max(screen.Bounds.Width, screen.Bounds.Height), (nfloat)Math.Min(screen.Bounds.Width, screen.Bounds.Height));
                }
                else
                {
					frame = new CGRect(0, 0, screen.Bounds.Width, screen.Bounds.Height);
                }
                #else
                frame = new CGRect(0, 0, screen.Bounds.Width, screen.Bounds.Height);
                #endif
            }

            base.View = new iOSGameView(_platform, frame);

            // Need to set resize mask to ensure a view resize (which in iOS 8+ corresponds with a rotation) adjusts
            // the view and underlying CALayer correctly
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            #if TVOS
            ControllerUserInteractionEnabled = false;
            #endif
        }

        public new iOSGameView View
        {
            get { return (iOSGameView)base.View; }
        }
        #if !TVOS

        #region Autorotation for iOS 5 or older
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

		public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
			CGSize oldSize = View.Bounds.Size;

            if (oldSize != toSize)
            {
                UIInterfaceOrientation prevOrientation = InterfaceOrientation;

                // In iOS 8+ DidRotate is no longer called after a rotation
                // But we need to notify iOSGamePlatform to update back buffer so we explicitly call it 

                // We do this within the animateAlongside action, which at the point of calling
                // will have the new InterfaceOrientation set
                coordinator.AnimateAlongsideTransition((context) =>
                    {
                        DidRotate(prevOrientation);
                    }, (context) => 
                    {
                    });

            }

            base.ViewWillTransitionToSize(toSize, coordinator);
        }

        #endregion

        #endif

        #if TVOS

        public override UIView PreferredFocusedView
        {
            get
            {
                return this.View;
            }
        }

        public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt)
        {
            if (presses.Count == 0)
                return;
            foreach (UIPress press in presses)
            {
                if (press.Type == UIPressType.Menu)
                {
                    if (platformBackButton == null)
                        platformBackButton = _platform.Game.Services.GetService<IPlatformBackButton>();
                    if (platformBackButton != null)
                    {
                        if (!platformBackButton.Handled())
                        {
                            ControllerUserInteractionEnabled = true;
                        }
                        else
                        {
                            Microsoft.Xna.Framework.Input.GamePad.MenuPressed = true;
                        }
                    }
                    else
                    {
                        ControllerUserInteractionEnabled = true;
                    }
                }
            }
            if (ControllerUserInteractionEnabled)
                base.PressesBegan(presses, evt);
        }

        public override void PressesEnded(NSSet<UIPress> presses, UIPressesEvent evt)
        {
            if (ControllerUserInteractionEnabled)
                base.PressesEnded(presses, evt);
        }
        #endif
    }
}
