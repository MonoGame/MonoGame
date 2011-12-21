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

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
    class iOSGamePlatform : GamePlatform
    {
        // FIXME: Previously components were being initialized on a background thread, apparently.  We may need to
        //        express this concept somewhere within the Game-GamePlatform contract.  Not sure where yet.
//        EAGLContext.SetCurrentContext(_window.BackgroundContext);
//        Initialize all components...
//        EAGLContext.SetCurrentContext(_window.MainContext);

        private GameWindow _gameWindow;
        private UIWindow _mainWindow;
        private NSObject _rotationObserver;
        private List<NSObject> _applicationObservers;

        public iOSGamePlatform(Game game) :
            base(game)
        {
            game.Services.AddService(typeof(iOSGamePlatform), this);
            Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);

            // Create a full-screen window
            _mainWindow = new UIWindow(UIScreen.MainScreen.Bounds);

            _gameWindow = new GameWindow(game, this);
            _gameWindow.Load += GameWindow_Load;
            _gameWindow.Unload += GameWindow_Unload;

            Window = _gameWindow;
            _mainWindow.Add(_gameWindow);
            _applicationObservers = new List<NSObject>();

            // FIXME: What is GameVc doing in GamerServices?
            //        It does silence iOS's warning that a root view controller
            //        is expected.
            _mainWindow.RootViewController = new Microsoft.Xna.Framework.GamerServices.GameVc();
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Asynchronous; }
        }

        public bool IsPlayingVideo { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _mainWindow.Dispose();
                _gameWindow.Dispose();
            }
        }

        public override void BeforeInitialize()
        {
            // HACK: Force the OpenGL context to be created before any
            //       components are initialized.  This hack could be eliminated
            //       by implementing a custom Initialize method in
            //       iOSGameWindow that calls CreateFrameBuffer and OnLoad.
            //       CreateFrameBuffer and OnLoad would both need to be changed
            //       to be idempotent per create-destroy cycle of the OpenGL
            //       context.
            _gameWindow.Run(1 / Game.TargetElapsedTime.TotalSeconds);
            _gameWindow.Pause();
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
            BeginObservingDeviceRotation();

            _gameWindow.Resume();
        }

        private void GameWindow_Load(object sender, EventArgs e)
        {
            _gameWindow.MainContext = _gameWindow.EAGLContext;
            _gameWindow.ShareGroup = _gameWindow.MainContext.ShareGroup;
            _gameWindow.BackgroundContext = new EAGLContext(_gameWindow.ContextRenderingApi, _gameWindow.ShareGroup);
        }

        private void GameWindow_Unload(object sender, EventArgs e)
        {
            _gameWindow.MainContext = null;
            _gameWindow.ShareGroup = null;
            // FIXME: Dispose BackgroundContext first?  We created it in
            //        GameWindow_Load.
            _gameWindow.BackgroundContext = null;
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
            StopObservingUIApplication();
            StopObservingDeviceRotation();
            // FIXME: Need to terminate the run loop.  On iOS, this probably means
            //        destroying _mainWindow.  But should async platforms try to kill the
            //        whole program?
            //throw new NotImplementedException();
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

        private void BeginObservingDeviceRotation()
        {
            _rotationObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIDeviceOrientationDidChangeNotification"),
                Device_OrientationDidChange);

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
        }

        private void StopObservingDeviceRotation()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(_rotationObserver);
            UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
        }

        #region Notification Handling

        private void Application_WillEnterForeground(NSNotification notification)
        {
            // FIXME: What needs to be done here?
            //throw new NotImplementedException();
        }

        private void Application_DidEnterBackground(NSNotification notification)
        {
            // FIXME: What needs to be done here?
            //throw new NotImplementedException();
        }

        private void Application_DidBecomeActive(NSNotification notification)
        {
            IsActive = true;
            TouchPanel.Reset();
        }

        private void Application_WillResignActive(NSNotification notification)
        {
            IsActive = false;
        }

        private void Application_WillTerminate(NSNotification notification)
        {
            // FIXME: Cleanly end the run loop.
        }

        private void Application_DidReceiveMemoryWarning(NSNotification notification)
        {
            // FIXME: Possibly add some more sophisticated behavior here.  It's
            //        also possible that this is not iOSGamePlatform's job.
            GC.Collect();
        }

        private void Device_OrientationDidChange(NSNotification notification)
        {
            var orientation = UIDeviceOrientationToDisplayOrientation(UIDevice.CurrentDevice.Orientation);

            // Calculate supported orientations if it has been left as "default"
            // FIXME: Obviously casting null is not going to work.  I think maybe a platform-specific
            //        GraphicsDeviceManager should be provided and owned by the specific GamePlatform implementations.
            //        If that intuition is right, then this problem will evaporate.
            var gdm = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            DisplayOrientation supportedOrientations = gdm.SupportedOrientations;

            var presentParams = gdm.GraphicsDevice.PresentationParameters;

            if (presentParams.BackBufferWidth != gdm.PreferredBackBufferWidth)
                presentParams.BackBufferWidth = gdm.PreferredBackBufferWidth;

            if (presentParams.BackBufferHeight != gdm.PreferredBackBufferHeight)
                presentParams.BackBufferHeight = gdm.PreferredBackBufferHeight;

            if ((supportedOrientations & DisplayOrientation.Default) != 0)
            {
                if (presentParams.BackBufferWidth > presentParams.BackBufferHeight)
                    supportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                else
                    supportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.PortraitUpsideDown;
            }

            if ((supportedOrientations & orientation) == orientation)
            {
                _gameWindow.CurrentOrientation = orientation;
                presentParams.DisplayOrientation = orientation;
                TouchPanel.DisplayOrientation = orientation;
            }
        }

        #endregion Notification Handling

        private static DisplayOrientation UIDeviceOrientationToDisplayOrientation(UIDeviceOrientation orientation)
        {
            switch (orientation)
            {
            case UIDeviceOrientation.FaceDown: return DisplayOrientation.FaceDown;
            case UIDeviceOrientation.FaceUp: return DisplayOrientation.FaceUp;
            default:
            case UIDeviceOrientation.LandscapeLeft: return DisplayOrientation.LandscapeLeft;
            case UIDeviceOrientation.LandscapeRight: return DisplayOrientation.LandscapeRight;
            case UIDeviceOrientation.Portrait: return DisplayOrientation.Portrait;
            case UIDeviceOrientation.PortraitUpsideDown: return DisplayOrientation.PortraitUpsideDown;
            }
        }
    }
}

