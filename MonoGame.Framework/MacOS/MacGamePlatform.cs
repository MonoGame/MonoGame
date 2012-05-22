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

using MonoMac.AppKit;
using MonoMac.Foundation;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework
{
    partial class MacGamePlatform : GamePlatform
    {
        private enum RunState
        {
            NotStarted,
            Running,
            Exiting,
            Exited
        }

        private MacGameNSWindow _mainWindow;
        private GameWindow _gameWindow;
        private bool _wasResizeable;
        private OpenALSoundController soundControllerInstance = null;

        public MacGamePlatform(Game game) :
            base(game)
        {
            _state = RunState.NotStarted;
            game.Services.AddService(typeof(MacGamePlatform), this);

            // Setup our OpenALSoundController to handle our SoundBuffer pools
            soundControllerInstance = OpenALSoundController.GetInstance;

            InitializeMainWindow();

            // We set the current directory to the ResourcePath on Mac
            Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);
        }

        private void InitializeMainWindow()
        {
            RectangleF frame = new RectangleF(
                0, 0,
                PresentationParameters._defaultBackBufferWidth,
                PresentationParameters._defaultBackBufferHeight);

            _mainWindow = new MacGameNSWindow(
                frame, NSWindowStyle.Titled | NSWindowStyle.Closable,
                NSBackingStore.Buffered, true);

            _mainWindow.WindowController = new NSWindowController(_mainWindow);
            _mainWindow.Delegate = new MainWindowDelegate(this);

            _mainWindow.IsOpaque = true;
            _mainWindow.EnableCursorRects();
            _mainWindow.AcceptsMouseMovedEvents = false;
            _mainWindow.Center();

            _gameWindow = new GameWindow(Game, frame);
            Window = _gameWindow;
            _mainWindow.ContentView.AddSubview(_gameWindow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No need to dispose _gameWindow.  It will be released by the
                // nearest NSAutoreleasePool.
                if (_gameWindow != null)
                    _gameWindow = null;

                // No need to dispose _mainWindow.  It will be released by the
                // nearest NSAutoreleasePool.
                if (_mainWindow != null)
                    _mainWindow = null;
            }

            base.Dispose(disposing);
        }

        private RunState _state;
        private RunState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public bool IsRunning
        {
            get { return _state == RunState.Running; }
        }

        public bool IsExiting
        {
            get { return _state == RunState.Exiting; }
        }

        public bool HasExited
        {
            get { return _state == RunState.Exited; }
        }

        private int _suspendUpdatingAndDrawingLevel;
        private bool AreUpdatingAndDrawingSuspended
        {
            get { return _suspendUpdatingAndDrawingLevel > 0; }
        }

        public TimeSpan InactiveSleepTime
        {
            get { return _inactiveSleepTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "Value cannot be negative");
                _inactiveSleepTime = value;
            }
        }

        [Obsolete("IsPlayingVideo must be removed once VideoPlayer is implemented according to the XNA contract")]
        public bool IsPlayingVideo { get; set; }

        private void SuspendUpdatingAndDrawing()
        {
            _suspendUpdatingAndDrawingLevel++;
        }

        private void ResumeUpdatingAndDrawing()
        {
            if (_suspendUpdatingAndDrawingLevel <= 0)
                throw new InvalidOperationException("Too many calls to ResumeUpdateAndDraw");
            _suspendUpdatingAndDrawingLevel--;
        }

        #region GamePlatform Implementation

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Asynchronous; }
        }

        public override bool BeforeRun()
        {
            State = RunState.Running;
            return true;
        }

        public override void Exit()
        {
            if (State != RunState.Running)
                return;

            State = RunState.Exiting;

            if (_mainWindow != null)
            {
                var windowController = (NSWindowController)_mainWindow.WindowController;
                windowController.Close();
            }
        }

        public override void StartRunLoop()
        {
            _mainWindow.MakeKeyAndOrderFront(_mainWindow);
            ResetWindowBounds();
            _gameWindow.StartRunLoop(1 / Game.TargetElapsedTime.TotalSeconds);

            // This is a hack and it should go in MonoMacGameView.  Until that
            // is done we will set the SwapInterval ourselves
            // Using DisplayLink does not play nicely with background thread
            // loading.
            var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            _gameWindow.OpenGLContext.SwapInterval = graphicsDeviceManager.SynchronizeWithVerticalRetrace;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            // Update our OpenAL sound buffer pools
            soundControllerInstance.Update();
            if (_needsToResetElapsedTime)
                _needsToResetElapsedTime = false;

            if (AreUpdatingAndDrawingSuspended || IsPlayingVideo || Guide.isVisible)
                return false;

            // Let the touch panel update states.
            TouchPanel.UpdateState();

            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            if (AreUpdatingAndDrawingSuspended || IsPlayingVideo || Guide.isVisible)
                return false;
            return true;
        }

        public override void EnterFullScreen()
        {
            // Changing window style forces a redraw. Some games
            // have fail-logic and toggle fullscreen in their draw function,
            // so temporarily become inactive so it won't execute.
            SuspendUpdatingAndDrawing();
            try
            {
                // I will leave this here just in case someone can figure out
                // how to do a full screen with this and still get Alt + Tab to
                // friggin work.
                //_mainWindow.ContentView.EnterFullscreenModeWithOptions(NSScreen.MainScreen,new NSDictionary());

                _wasResizeable = AllowUserResizing;

                string oldTitle = _gameWindow.Title;

                NSMenu.MenuBarVisible = false;
                _mainWindow.StyleMask = NSWindowStyle.Borderless;

                // Set the level here to normal
                _mainWindow.Level = NSWindowLevel.Floating;

                if (oldTitle != null)
                    _gameWindow.Title = oldTitle;

                _mainWindow.IsVisible = false;
                // FIXME: EnterFullScreen gets called very early and interferes
                //        with Synchronous mode, so disabling this for now.
                //        Hopefully this does not cause excessive havoc.
                //_mainWindow.MakeKeyAndOrderFront(Window);
                ResetWindowBounds();
                _mainWindow.HidesOnDeactivate = true;
                Mouse.ResetMouse();
            }
            finally { ResumeUpdatingAndDrawing(); }
        }

        public override void ExitFullScreen()
        {
            // Changing window style forces a redraw. Some games
            // have fail-logic and toggle fullscreen in their draw function,
            // so temporarily become inactive so it won't execute.
            SuspendUpdatingAndDrawing();
            try
            {
                _wasResizeable = AllowUserResizing;

                // I will leave this here just in case someone can figure out
                // how to do a full screen with this and still get Alt + Tab to
                // friggin work.
                // _mainWindow.ContentView.ExitFullscreenModeWithOptions(new NSDictionary());

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

                _mainWindow.IsVisible = false;
                // FIXME: EnterFullScreen gets called very early and interferes
                //        with Synchronous mode, so disabling this for now.
                //        Hopefully this does not cause excessive havoc.
                //_mainWindow.MakeKeyAndOrderFront(Window);
                ResetWindowBounds();
                _mainWindow.HidesOnDeactivate = false;
                Mouse.ResetMouse();
            }
            finally { ResumeUpdatingAndDrawing(); }
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _mainWindow.InvalidateCursorRectsForView(_gameWindow);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            // This is a hack and it should go in MonoMacGameView.  Until that
            // is done we will set the SwapInterval ourselves
            // Using DisplayLink does not play nicely with background thread
            // loading.
           var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            _gameWindow.OpenGLContext.SwapInterval = graphicsDeviceManager.SynchronizeWithVerticalRetrace;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void ResetElapsedTime()
        {
            _gameWindow.ResetElapsedTime();
        }

        #endregion

        private bool AllowUserResizing
        {
            get { return (_mainWindow.StyleMask & NSWindowStyle.Resizable) != 0; }
            set { _mainWindow.StyleMask ^= NSWindowStyle.Resizable; }
        }

        public void EnterBackground()
        {
            SuspendUpdatingAndDrawing();
            IsActive = false;
        }

        public void EnterForeground()
        {
            ResumeUpdatingAndDrawing();
            IsActive = true;
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
            // FIXME: Eliminate the need for null checks by only calling
            //        ResetWindowBounds after the device is ready.  Or,
            //        possibly break this method into smaller methods.
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


        private class MainWindowDelegate : NSWindowDelegate
        {
            private readonly MacGamePlatform _owner;
            public MainWindowDelegate(MacGamePlatform owner)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");
                _owner = owner;
            }

            public override void DidBecomeKey(NSNotification notification)
            {
                //if (!IsMouseVisible)
                //    _gameWindow.HideCursor();
                _owner.IsActive = true;
            }

            public override void DidResignKey(NSNotification notification)
            {
                //if (!IsMouseVisible)
                //    _gameWindow.UnHideCursor();
                _owner.IsActive = false;
            }

            public override void DidBecomeMain(NSNotification notification)
            {
                //if (!IsMouseVisible)
                //    _gameWindow.HideCursor();
            }

            public override void DidResignMain(NSNotification notification)
            {
                //if (!IsMouseVisible)
                //    _gameWindow.UnHideCursor();
            }

            public override void WillClose(NSNotification notification)
            {
                NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
                    _owner.State = MacGamePlatform.RunState.Exited);
            }
        }
    }
}
