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
            game.Services.AddService(typeof(MacGamePlatform), this);

            RectangleF frame = new RectangleF(
                0, 0,
                PresentationParameters._defaultBackBufferWidth,
                PresentationParameters._defaultBackBufferHeight);

            // Create a window
            _mainWindow = new MacGameNSWindow(
                frame, NSWindowStyle.Titled | NSWindowStyle.Closable,
                NSBackingStore.Buffered, true);
            _mainWindow.IsOpaque = true;
            _mainWindow.EnableCursorRects();
            _mainWindow.AcceptsMouseMovedEvents = false;
            _mainWindow.Center();

            _gameWindow = new GameWindow(game, frame);
            Window = _gameWindow;
            _mainWindow.ContentView.AddSubview(_gameWindow);

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
            // FIXME: Does this really apply on OS X?
            // TODO NSDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Asynchronous; }
        }

        public bool IsPlayingVideo { get; set; }

        #region GamePlatform Implementation

        public override void Exit()
        {
            // FIXME: Should we not simply terminate our run loop, rather than
            //        forcefully destroying the whole application?
            NSApplication.SharedApplication.Terminate(new NSObject());
        }

        public override bool BeforeRun()
        {
            _mainWindow.MakeKeyAndOrderFront(_mainWindow);
            ResetWindowBounds();
            return true;
        }

        public override void RunLoop()
        {
            throw new NotSupportedException("Blocking run loops are not supported on this platform");
        }

        public override void StartRunLoop()
        {
            _gameWindow.Run(1 / Game.TargetElapsedTime.TotalSeconds);
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            if (IsPlayingVideo)
                return false;
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            if (IsPlayingVideo)
                return false;
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
            if (_wasResizeable)
                _mainWindow.StyleMask |= NSWindowStyle.Resizable;

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

        protected override void OnIsMouseVisibleChanged()
        {
            _mainWindow.InvalidateCursorRectsForView(_gameWindow);
        }

        #endregion

        internal bool AllowUserResizing
        {
            get { return (_mainWindow.StyleMask & NSWindowStyle.Resizable) != 0; }
            set { _mainWindow.StyleMask ^= NSWindowStyle.Resizable; }
        }

        private void ResetWindowBounds()
        {
            RectangleF frame;
            RectangleF content;

            var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));

            if (graphicsDeviceManager.IsFullScreen)
            {
                frame = NSScreen.MainScreen.Frame;
                content = NSScreen.MainScreen.Frame;
            }
            else
            {
                content = _gameWindow.Bounds;
                content.Width = Math.Min(
                    graphicsDeviceManager.PreferredBackBufferWidth,
                    NSScreen.MainScreen.VisibleFrame.Width);
                content.Height = Math.Min(
                    graphicsDeviceManager.PreferredBackBufferHeight,
                    NSScreen.MainScreen.VisibleFrame.Height - GetTitleBarHeight());

                frame = _mainWindow.Frame;
                frame.X = Math.Max(frame.X, NSScreen.MainScreen.VisibleFrame.X);
                frame.Y = Math.Max(frame.Y, NSScreen.MainScreen.VisibleFrame.Y);
                frame.Width = content.Width;
                frame.Height = content.Height + GetTitleBarHeight();
            }
            _mainWindow.SetFrame(frame, true);

            _gameWindow.Bounds = content;
            _gameWindow.Size = content.Size.ToSize();

            // Now we set our Presentation Parameters
            var device = (GraphicsDevice)graphicsDeviceManager.GraphicsDevice;
            // HACK: Eliminate the need for null checks by only calling
            //       ResetWindowBounds after the device is ready.  Or,
            //       possibly break this method into smaller methods.
            if (device != null)
            {
                PresentationParameters parms = device.PresentationParameters;
                parms.BackBufferHeight = (int)content.Size.Height;
                parms.BackBufferWidth = (int)content.Size.Width;
            }
        }

        private float GetTitleBarHeight()
        {
            RectangleF contentRect = NSWindow.ContentRectFor(
                _mainWindow.Frame, _mainWindow.StyleMask);

            return _mainWindow.Frame.Height - contentRect.Height;
        }
    }
}
