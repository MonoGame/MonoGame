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
            base.BeforeInitialize();

            var gdm = Game.graphicsDeviceManager;
            var pp = Game.GraphicsDevice.PresentationParameters;
            _window.Initialize(pp.BackBufferWidth, pp.BackBufferHeight);

            if (gdm.IsFullScreen)
                EnterFullScreen();
            else
                ExitFullScreen();
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
            if (Game.graphicsDeviceManager.HardwareModeSwitch)
                Game.GraphicsDevice.SetHardwareFullscreen();
            else
                _window.IsBorderless = true;

            _window._form.WindowState = FormWindowState.Maximized;

            InFullScreenMode = true;
        }

        public override void ExitFullScreen()
        {
            if (Game.graphicsDeviceManager.HardwareModeSwitch)
                Game.GraphicsDevice.SetHardwareFullscreen();
            else
                _window.IsBorderless = false;

            _window._form.WindowState = FormWindowState.Normal;

            InFullScreenMode = false;
        }

        internal override void OnPresentationChanged()
        {
            if (Game.GraphicsDevice.PresentationParameters.IsFullScreen && !InFullScreenMode)
            {
                EnterFullScreen();
                _window.OnClientSizeChanged();
            }
            else if (!Game.GraphicsDevice.PresentationParameters.IsFullScreen && InFullScreenMode)
            {
                ExitFullScreen();
                _window.ChangeClientSize(new Size(Game.graphicsDeviceManager.PreferredBackBufferWidth, Game.graphicsDeviceManager.PreferredBackBufferHeight));
                _window.OnClientSizeChanged();
            }
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
