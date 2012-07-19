#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

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
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework
{
    class iOSGamePlatform : GamePlatform
    {
        private iOSGameViewController _viewController;
        private UIWindow _mainWindow;
        private List<NSObject> _applicationObservers;
		private OpenALSoundController soundControllerInstance = null;
        private NSTimer _runTimer;
        private bool _isExitPending;

        public iOSGamePlatform(Game game) :
            base(game)
        {
            game.Services.AddService(typeof(iOSGamePlatform), this);
			
			// Setup our OpenALSoundController to handle our SoundBuffer pools
			soundControllerInstance = OpenALSoundController.GetInstance;
			
            Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);

            _applicationObservers = new List<NSObject>();

            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);

            // Create a full-screen window
            _mainWindow = new UIWindow (UIScreen.MainScreen.Bounds);
			//_mainWindow.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			
            game.Services.AddService (typeof(UIWindow), _mainWindow);

            _viewController = new iOSGameViewController(this);
            game.Services.AddService (typeof(UIViewController), _viewController);
            Window = new iOSGameWindow (_viewController);

            _mainWindow.RootViewController = _viewController;
            _mainWindow.Add (_viewController.View);

            _viewController.InterfaceOrientationChanged += ViewController_InterfaceOrientationChanged;

            Guide.Initialise(game);
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Asynchronous; }
        }

        [Obsolete(
            "iOSGamePlatform.IsPlayingVideo must be removed when MonoGame " +
            "fully implements the XNA VideoPlayer contract.")]
        public bool IsPlayingVideo { get; set; }

        // FIXME: VideoPlayer 'needs' this to set up its own movie player view
        //        controller.
        public iOSGameViewController ViewController
        {
            get { return _viewController; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_viewController != null)
                {
                    _viewController.View.RemoveFromSuperview ();
                    _viewController.RemoveFromParentViewController ();
                    _viewController.Dispose();
                    _viewController = null;
                }

                if (_mainWindow != null)
                {
                    _mainWindow.Dispose();
                    _mainWindow = null;
                }
            }
        }

        public override void BeforeInitialize()
        {
            base.BeforeInitialize ();

            _viewController.View.LayoutSubviews();
        }

        public override void RunLoop()
        {
            throw new NotSupportedException("The iOS platform does not support synchronous run loops");
        }

        public override void StartRunLoop()
        {
            // Show the window
            _mainWindow.MakeKeyAndVisible();

            Accelerometer.SetupAccelerometer();
            BeginObservingUIApplication();

            _viewController.View.BecomeFirstResponder();
            _runTimer = NSTimer.CreateRepeatingScheduledTimer(Game.TargetElapsedTime, Tick);
        }

        private void Tick()
        {
            if (!Game.IsActive)
                return;

            try {
                if (PerformPendingExit())
                    return;
                if (IsPlayingVideo)
                    return;

                // FIXME: Remove this call, and the whole Tick method, once
                //        GraphicsDevice is where platform-specific Present
                //        functionality is actually implemented.  At that
                //        point, it should be possible to pass Game.Tick
                //        directly to NSTimer.CreateRepeatingTimer.
                _viewController.View.MakeCurrent();
                Game.Tick ();

                if (!IsPlayingVideo)
                    _viewController.View.Present ();

                PerformPendingExit();
            } catch (Exception ex) {
#if DEBUG				
                Console.WriteLine(
                    "Error while processing the main game loop: {0}\n{1}",
                    ex.Message, ex.StackTrace);
#endif
                Game.Exit ();
            }
        }

        private bool PerformPendingExit()
        {
            if (!_isExitPending)
                return false;

            _isExitPending = false;
            if (_runTimer != null) {
                _runTimer.Invalidate ();
                _runTimer.Dispose ();
                _runTimer = null;
            }
            UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
            StopObservingUIApplication ();
            RaiseAsyncRunLoopEnded ();
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
		// Update our OpenAL sound buffer pools
		soundControllerInstance.Update();
            if (IsPlayingVideo)
                return false;
            return true;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (IsPlayingVideo)
                return false;

            // Let the touch panel update states.
            TouchPanel.UpdateState();
			
            return true;
        }

        public override void EnterFullScreen()
        {
            // Do nothing: iOS games are always full screen
        }

        public override void ExitFullScreen()
        {
            // Do nothing: iOS games are always full screen
        }

        public override void Exit()
        {
            _isExitPending = true;
        }

        private void BeginObservingUIApplication()
        {
            var events = new Tuple<NSString, Action<NSNotification>>[]
            {
                Tuple.Create(
                    UIApplication.WillEnterForegroundNotification,
                    new Action<NSNotification>(Application_WillEnterForeground)),
                Tuple.Create(
                    UIApplication.DidEnterBackgroundNotification,
                    new Action<NSNotification>(Application_DidEnterBackground)),
                Tuple.Create(
                    UIApplication.DidBecomeActiveNotification,
                    new Action<NSNotification>(Application_DidBecomeActive)),
                Tuple.Create(
                    UIApplication.WillResignActiveNotification,
                    new Action<NSNotification>(Application_WillResignActive)),
                Tuple.Create(
                    UIApplication.WillTerminateNotification,
                    new Action<NSNotification>(Application_WillTerminate)),
                Tuple.Create(
                    UIApplication.DidReceiveMemoryWarningNotification,
                    new Action<NSNotification>(Application_DidReceiveMemoryWarning))
             };

            foreach (var entry in events)
                _applicationObservers.Add(NSNotificationCenter.DefaultCenter.AddObserver(entry.Item1, entry.Item2));
        }

        private void StopObservingUIApplication()
        {
            NSNotificationCenter.DefaultCenter.RemoveObservers(_applicationObservers);
            _applicationObservers.Clear();
        }

        #region Notification Handling

        private void Application_WillEnterForeground(NSNotification notification)
        {
			// Already handled in Application_DidBecomeActive. See below for IsActive state change.	
        }

        private void Application_DidEnterBackground(NSNotification notification)
        {
			// Already handled in Application_WillResignActive. See below for IsActive state change.
        }

        private void Application_DidBecomeActive(NSNotification notification)
        {
            IsActive = true;
            //TouchPanel.Reset();
        }

        private void Application_WillResignActive(NSNotification notification)
        {
            IsActive = false;
        }

        private void Application_WillTerminate(NSNotification notification)
        {
            // FIXME: Cleanly end the run loop.
			if ( Game != null )
			{
				// TODO MonoGameGame.Terminate();
			}
        }

        private void Application_DidReceiveMemoryWarning(NSNotification notification)
        {
            // FIXME: Possibly add some more sophisticated behavior here.  It's
            //        also possible that this is not iOSGamePlatform's job.
            GC.Collect();
        }

        #endregion Notification Handling

		private void ViewController_InterfaceOrientationChanged (object sender, EventArgs e)
		{
			var orientation = OrientationConverter.ToDisplayOrientation (
				_viewController.InterfaceOrientation);

			// FIXME: The presentation parameters for the GraphicsDevice should
			//        be managed by the GraphicsDevice itself.  Not by
			//        iOSGamePlatform.
			var gdm = (GraphicsDeviceManager) Game.Services.GetService (typeof (IGraphicsDeviceManager));

			if (gdm != null)
			{	
				var presentParams = gdm.GraphicsDevice.PresentationParameters;
				presentParams.BackBufferWidth = gdm.PreferredBackBufferWidth;
				presentParams.BackBufferHeight = gdm.PreferredBackBufferHeight;
				presentParams.DisplayOrientation = orientation;

				// Recalculate our views.
				ViewController.View.LayoutSubviews();
			}
			TouchPanel.DisplayOrientation = orientation;
		}

		public override void BeginScreenDeviceChange (bool willBeFullScreen)
		{
		}

		public override void EndScreenDeviceChange (string screenDeviceName, int clientWidth,int clientHeight)
		{
		}
	}
}
