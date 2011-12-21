// FIXME: Add appropriate license
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

