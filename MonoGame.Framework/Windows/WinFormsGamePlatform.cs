// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.Framework
{
    class WinFormsGamePlatform : GamePlatform
    {
        //internal static string LaunchParameters;

        private WinFormsGameWindow _window;

        public WinFormsGamePlatform(Game game)
            : base(game)
        {
            _window = new WinFormsGameWindow(this);

            Mouse.Window = _window._form;

            Window = _window;
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _window.MouseVisibleToggled();
        }

        public override bool BeforeRun()
        {
            _window.UpdateWindows();
            return base.BeforeRun();
        }

        public override void BeforeInitialize()
        {
            var gdm = Game.graphicsDeviceManager;

            _window.EnableClientSizeChangedEvent(false); // Disable ClientSizeChanged event while the window is initialised

            _window.Initialize(gdm.PreferredBackBufferWidth, gdm.PreferredBackBufferHeight);

            base.BeforeInitialize();

            if (gdm.IsFullScreen)
                EnterFullScreen();
            else
                ExitFullScreen();

            _window.EnableClientSizeChangedEvent(true); // Re-enable (and trigger) ClientSizeChanged event
        }

        public override void RunLoop()
        {
            _window.RunLoop();
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The Windows platform does not support asynchronous run loops");
        }
        
        public override void Exit()
        {
            if (_window != null)
                _window.Dispose();
            _window = null;
            Window = null;
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
            if (_alreadyInFullScreenMode)
                return;

            if (Game.graphicsDeviceManager.HardwareModeSwitch)
            {
                Game.GraphicsDevice.PresentationParameters.IsFullScreen = true;
                Game.GraphicsDevice.SetHardwareFullscreen();
            }
            else
            {
                _window.IsBorderless = true;
            }

            _window._form.WindowState = FormWindowState.Maximized;

            _alreadyInWindowedMode = false;
            _alreadyInFullScreenMode = true;
        }

        public override void ExitFullScreen()
        {
            if (_alreadyInWindowedMode)
               return;

            if (Game.graphicsDeviceManager.HardwareModeSwitch)
            {
                Game.GraphicsDevice.PresentationParameters.IsFullScreen = false;
                Game.GraphicsDevice.SetHardwareFullscreen();
            }
            else
            {
                _window.IsBorderless = false;
            }

            _window._form.WindowState = FormWindowState.Normal;

            _alreadyInWindowedMode = true;
            _alreadyInFullScreenMode = false;
        }

        internal override void OnPresentationChanged()
        {
            _window.EnableClientSizeChangedEvent(false); // Disable ClientSizeChanged event while the window is resized

            if (Game.GraphicsDevice.PresentationParameters.IsFullScreen)
            {
                EnterFullScreen();
            }
            else
            {
                ExitFullScreen();
                _window.ChangeClientSize(new Size(Game.graphicsDeviceManager.PreferredBackBufferWidth, Game.graphicsDeviceManager.PreferredBackBufferHeight));
            }

            _window.EnableClientSizeChangedEvent(true); // Re-enable (and trigger) ClientSizeChanged event
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void Log(string message)
        {
            Debug.WriteLine(message);
        }

        public override void Present()
        {
            var device = Game.GraphicsDevice;
            if ( device != null )
                device.Present();
        }
		
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_window != null)
                {
                    _window.Dispose();
                    _window = null;
                    Window = null;
                }
                Microsoft.Xna.Framework.Media.MediaManagerState.CheckShutdown();
            }

            base.Dispose(disposing);
        }
    }
}
