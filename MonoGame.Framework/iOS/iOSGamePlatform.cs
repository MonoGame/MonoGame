// FIXME: Add appropriate license
using System;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
    class iOSGamePlatform : GamePlatform
    {
        // FIXME: Previously components were being initialized on a background thread, apparently.  We made need to
        //        express this concept somewhere within the Game-GamePlatform contract.  Not sure where yet.
//        EAGLContext.SetCurrentContext(_window.BackgroundContext);
//        Initialize all components...
//        EAGLContext.SetCurrentContext(_window.MainContext);


        private GameWindow _window;
        private UIWindow _mainWindow;
        private NSObject _rotationObserver;

        public iOSGamePlatform(Game game) :
            base(game)
        {
            // Create a full-screen window
            _mainWindow = new UIWindow(UIScreen.MainScreen.Bounds);
            _window = new GameWindow(game, this);
            _mainWindow.Add(_window);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _mainWindow.Dispose();
                _window.Dispose();
            }
        }

        public override void RunLoop()
        {
            throw new NotSupportedException("The iOS platform does not support synchronous run loops");
        }

        public override void StartRunLoop()
        {
            _window.MainContext = _window.EAGLContext;
            _window.ShareGroup = _window.MainContext.ShareGroup;
            _window.BackgroundContext = new EAGLContext(_window.ContextRenderingApi, _window.ShareGroup);

            // Show the window
            _mainWindow.MakeKeyAndVisible();

            Accelerometer.SetupAccelerometer();
            BeginObservingDeviceRotation();

            _window.Run(1 / Game.TargetElapsedTime.TotalSeconds);
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
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

        public override void EnterForeground()
        {
            // Do nothing: iOS games are always in the foreground when they are active.
        }

        public override void EnterBackground()
        {
            // Do nothing: iOS games are always in the foreground when they are active.
        }

        public override void Exit()
        {
            StopObservingDeviceRotation();
            // FIXME: Need to terminate the run loop.  On iOS, this probably means
            //        destroying _mainWindow.  But should async platforms try to kill the
            //        whole program?
            throw new NotImplementedException();
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

        private void Device_OrientationDidChange(NSNotification notification)
        {
            var orientation = UIDeviceOrientationToDisplayOrientation(UIDevice.CurrentDevice.Orientation);

            // Calculate supported orientations if it has been left as "default"
            // FIXME: Obviously casting null is not going to work.  I think maybe a platform-specific
            //        GraphicsDeviceManager should be provided and owned by the specific GamePlatform implementations.
            //        If that intuition is right, then this problem will evaporate.
            var gdm = (GraphicsDeviceManager)null;
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
                _window.CurrentOrientation = orientation;
                presentParams.DisplayOrientation = orientation;
                TouchPanel.DisplayOrientation = orientation;
            }
        }

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

