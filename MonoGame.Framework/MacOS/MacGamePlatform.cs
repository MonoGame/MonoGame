using System;
using System.Drawing;
using System.IO;

using Microsoft.Xna.Framework.Graphics;

using MonoMac.AppKit;
using MonoMac.Foundation;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    class MacGamePlatform : GamePlatform
    {
        private MacGameNSWindow _mainWindow;
        private GameWindow _gameWindow;
        private bool _wasResizeable;

        public MacGamePlatform(Game game) :
            base(game)
        {
            RectangleF frame = new RectangleF(
                0, 0,
                PresentationParameters._defaultBackBufferWidth,
                PresentationParameters._defaultBackBufferHeight);

            //Create a window
            _mainWindow = new MacGameNSWindow(
                frame, NSWindowStyle.Titled | NSWindowStyle.Closable,
                NSBackingStore.Buffered, true);

            // Perform any other window configuration you desire
            _mainWindow.IsOpaque = true;
            _mainWindow.EnableCursorRects();

            _gameWindow = new GameWindow(game, frame);
            Window = _gameWindow;

            _mainWindow.ContentView.AddSubview(_gameWindow);
            _mainWindow.AcceptsMouseMovedEvents = false;
            _mainWindow.Center();

            // We set the current directory to the ResourcePath on Mac
            Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);

            // Leave these here for when we implement the Activate and Deactivated
            _mainWindow.DidBecomeKey += delegate(object sender, EventArgs e) {
                //if (!IsMouseVisible)
                //    _gameWindow.HideCursor();
                //Console.WriteLine("BecomeKey");
                IsActive = true;
            };

            _mainWindow.DidResignKey += delegate(object sender, EventArgs e) {
                //if (!IsMouseVisible)
                //    _gameWindow.UnHideCursor();
                //Console.WriteLine("ResignKey");
                IsActive = false;
            };

            _mainWindow.DidBecomeMain += delegate(object sender, EventArgs e) {
                //if (!IsMouseVisible)
                //    _gameWindow.HideCursor();
                ////Console.WriteLine("BecomeMain");
            };

            _mainWindow.DidResignMain += delegate(object sender, EventArgs e) {
                //if (!IsMouseVisible)
                //    _gameWindow.UnHideCursor();
                //Console.WriteLine("ResignMain");
            };
        }

        ~MacGamePlatform()
        {
            // FIXME: Does this really apply on OS X
            // TODO NSDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
        }

        #region GamePlatform Implementation

        public override void Exit()
        {
            // FIXME: Should we not simply terminate our run loop, rather than
            //        forcefully destroying the whole application?
            NSApplication.SharedApplication.Terminate(new NSObject());
        }

        public override void BeforeInitialize()
        {
            ResetWindowBounds();
            _mainWindow.MakeKeyAndOrderFront(_mainWindow);
        }

        public override void RunLoop()
        {
            throw new NotImplementedException();
        }

        public override void StartRunLoop()
        {
            _gameWindow.Run(1 / Game.TargetElapsedTime.TotalSeconds);
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
            bool wasActive = IsActive;
            IsActive = false;

            // I will leave this here just in case someone can figure out how
            // to do a full screen with this and still get Alt + Tab to friggin
            // work.
            //_mainWindow.ContentView.EnterFullscreenModeWithOptions(NSScreen.MainScreen,new NSDictionary());

            _wasResizeable = AllowUserResizing;

            string oldTitle = _gameWindow.Title;

            NSMenu.MenuBarVisible = false;
            _mainWindow.StyleMask = NSWindowStyle.Borderless;

            // Set the level here to normal
            _mainWindow.Level = NSWindowLevel.Floating;

            if (oldTitle != null)
                _gameWindow.Title = oldTitle;

            Window.Window.IsVisible = false;
            Window.Window.MakeKeyAndOrderFront(Window);
            ResetWindowBounds();
            _mainWindow.HidesOnDeactivate = true;
            Window.Window.HidesOnDeactivate = true;
            Mouse.ResetMouse();

            IsActive = wasActive;
        }

        public override void ExitFullScreen()
        {
            _wasResizeable = AllowUserResizing;

            // Changing window style forces a redraw. Some games
            // have fail-logic and toggle fullscreen in their draw function,
            // so temporarily become inactive so it won't execute.
            bool wasActive = IsActive;
            IsActive = false;

            // I will leave this here just in case someone can figure out how to do
            //  a full screen with this and still get Alt + Tab to friggin work.
//            _mainWindow.ContentView.ExitFullscreenModeWithOptions(new NSDictionary());

            //Changing window style resets the title. Save it.
            string oldTitle = _gameWindow.Title;

            NSMenu.MenuBarVisible = true;
            _mainWindow.StyleMask = NSWindowStyle.Titled | NSWindowStyle.Closable;
            if (_wasResizeable) _mainWindow.StyleMask |= NSWindowStyle.Resizable;

            if (oldTitle != null)
                _gameWindow.Title = oldTitle;


            // Set the level here to normal
            _mainWindow.Level = NSWindowLevel.Normal;

            Window.Window.IsVisible = false;
            Window.Window.MakeKeyAndOrderFront(Window);
            ResetWindowBounds();
            _mainWindow.HidesOnDeactivate = false;
            Mouse.ResetMouse();

            IsActive = wasActive;
        }

        public override void EnterForeground()
        {
            throw new NotImplementedException();
        }

        public override void EnterBackground()
        {
            throw new NotImplementedException();
        }

        public override void IsMouseVisibleChanged()
        {
            _mainWindow.InvalidateCursorRectsForView(_gameWindow);
        }

        #endregion

        internal bool AllowUserResizing
        {
            get { return (_mainWindow.StyleMask & NSWindowStyle.Resizable) != 0; }
            set { _mainWindow.StyleMask ^= NSWindowStyle.Resizable; }
        }


        private void ResetWindowBounds ()
        {
            RectangleF frame;
            RectangleF content;

            // FIXME: Obvious nonsense.
            var graphicsDeviceManager = (GraphicsDeviceManager)null;

            if (graphicsDeviceManager.IsFullScreen) {
                frame = NSScreen.MainScreen.Frame;
                content = NSScreen.MainScreen.Frame;
            } else {
                content = _gameWindow.Bounds;
                content.Width = Math.Min(
                    graphicsDeviceManager.PreferredBackBufferWidth,
                    NSScreen.MainScreen.VisibleFrame.Width);
                content.Height = Math.Min(
                    graphicsDeviceManager.PreferredBackBufferHeight,
                    NSScreen.MainScreen.VisibleFrame.Height-GetTitleBarHeight());

                frame = _mainWindow.Frame;
                frame.X = Math.Max(frame.X, NSScreen.MainScreen.VisibleFrame.X);
                frame.Y = Math.Max(frame.Y, NSScreen.MainScreen.VisibleFrame.Y);
                frame.Width = content.Width;
                frame.Height = content.Height + GetTitleBarHeight();
            }
            _mainWindow.SetFrame (frame, true);

            _gameWindow.Bounds = content;
            _gameWindow.Size = content.Size.ToSize();

            // FIXME: Obvious nonsense
            var GraphicsDevice = (GraphicsDevice)null;
            // Now we set our Presentaion Parameters
            PresentationParameters parms = GraphicsDevice.PresentationParameters;
            parms.BackBufferHeight = (int)content.Size.Height;
            parms.BackBufferWidth = (int)content.Size.Width;
        }

        private float GetTitleBarHeight()
        {
            RectangleF contentRect = NSWindow.ContentRectFor(
                _mainWindow.Frame, _mainWindow.StyleMask);

            return _mainWindow.Frame.Height - contentRect.Height;
        }
    }
}
