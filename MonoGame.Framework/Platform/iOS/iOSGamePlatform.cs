// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

using Foundation;
using OpenGLES;
using UIKit;
using CoreAnimation;
using ObjCRuntime;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
//using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework
{
    class iOSGamePlatform : GamePlatform
    {
        private iOSGameViewController _viewController;
        private UIWindow _mainWindow;
        private List<NSObject> _applicationObservers;
        private CADisplayLink _displayLink;

        public iOSGamePlatform(Game game) :
            base(game)
        {
            game.Services.AddService(typeof(iOSGamePlatform), this);

            //This also runs the TitleContainer static constructor, ensuring it is done on the main thread
            Directory.SetCurrentDirectory(TitleContainer.Location);

            _applicationObservers = new List<NSObject>();

            #if !TVOS
            UIApplication.SharedApplication.SetStatusBarHidden(true, UIStatusBarAnimation.Fade);
            #endif

            // Create a full-screen window
            _mainWindow = new UIWindow (UIScreen.MainScreen.Bounds);
			//_mainWindow.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
			
            game.Services.AddService (typeof(UIWindow), _mainWindow);

            _viewController = new iOSGameViewController(this);
            game.Services.AddService (typeof(UIViewController), _viewController);
            Window = new iOSGameWindow (_viewController);

            _mainWindow.Add (_viewController.View);

            _viewController.InterfaceOrientationChanged += ViewController_InterfaceOrientationChanged;

            //(SJ) Why is this called here when it's not in any other project
            //Guide.Initialise(game);
        }

        public override void TargetElapsedTimeChanged ()
        {
            CreateDisplayLink();
        }

        private void CreateDisplayLink()
        {
            if (_displayLink != null)
                _displayLink.RemoveFromRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);

            _displayLink = UIScreen.MainScreen.CreateDisplayLink(_viewController.View as iOSGameView, new Selector("doTick"));

            // FrameInterval represents how many frames must pass before the selector
            // is called again. We calculate this by dividing our target elapsed time by
            // the duration of a frame on iOS (Which is 1/60.0f at the time of writing this).
            _displayLink.FrameInterval = (int)Math.Round(60f * Game.TargetElapsedTime.TotalSeconds);

            _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
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

            // In iOS 8+ we need to set the root view controller *after* Window MakeKey
            // This ensures that the viewController's supported interface orientations
            // will be respected at launch
            _mainWindow.RootViewController = _viewController;

            BeginObservingUIApplication();

            _viewController.View.BecomeFirstResponder();
            CreateDisplayLink();
        }

        internal void Tick()
        {
            if (!Game.IsActive)
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
            Threading.Run();

            if (!IsPlayingVideo)
            {
                if (Game.GraphicsDevice != null)
                {
                    // GraphicsDevice.Present() takes care of actually 
                    // disposing resources disposed from a non-ui thread
                    Game.GraphicsDevice.Present();
                }
                _viewController.View.Present ();
            }
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            if (IsPlayingVideo)
                return false;

            return true;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (IsPlayingVideo)
                return false;

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
            // Do Nothing: iOS games do not "exit" or shut down.
        }

        private void BeginObservingUIApplication()
        {
            var events = new Tuple<NSString, Action<NSNotification>>[]
            {
                Tuple.Create(
                    UIApplication.DidBecomeActiveNotification,
                    new Action<NSNotification>(Application_DidBecomeActive)),
                Tuple.Create(
                    UIApplication.WillResignActiveNotification,
                    new Action<NSNotification>(Application_WillResignActive)),
                Tuple.Create(
                    UIApplication.WillTerminateNotification,
                    new Action<NSNotification>(Application_WillTerminate)),
             };

            foreach (var entry in events)
                _applicationObservers.Add(NSNotificationCenter.DefaultCenter.AddObserver(entry.Item1, entry.Item2));
        }

        #region Notification Handling

        private void Application_DidBecomeActive(NSNotification notification)
        {
            IsActive = true;
            #if TVOS
            _viewController.ControllerUserInteractionEnabled = false;
            #endif
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

        #endregion Notification Handling

        #region Helper Property

        private DisplayOrientation CurrentOrientation {
            get {
                #if TVOS
                return DisplayOrientation.LandscapeLeft;
                #else
                return OrientationConverter.ToDisplayOrientation(_viewController.InterfaceOrientation);
                #endif
            }
        }

        #endregion

		private void ViewController_InterfaceOrientationChanged (object sender, EventArgs e)
		{
			var orientation = CurrentOrientation;

			// FIXME: The presentation parameters for the GraphicsDevice should
			//        be managed by the GraphicsDevice itself.  Not by
			//        iOSGamePlatform.
			var gdm = (GraphicsDeviceManager) Game.Services.GetService (typeof (IGraphicsDeviceManager));

            TouchPanel.DisplayOrientation = orientation;

			if (gdm != null)
			{	

				var presentParams = gdm.GraphicsDevice.PresentationParameters;
				presentParams.BackBufferWidth = gdm.PreferredBackBufferWidth;
				presentParams.BackBufferHeight = gdm.PreferredBackBufferHeight;

				presentParams.DisplayOrientation = orientation;

                // Recalculate our views.
                ViewController.View.LayoutSubviews();
				
                gdm.ApplyChanges();
			}
			
		}

		public override void BeginScreenDeviceChange (bool willBeFullScreen)
		{
		}

		public override void EndScreenDeviceChange (string screenDeviceName, int clientWidth,int clientHeight)
		{
		}
	}
}
