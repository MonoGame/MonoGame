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

            _window.Initialize(gdm.PreferredBackBufferWidth, gdm.PreferredBackBufferHeight);

            base.BeforeInitialize();

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
            if (_alreadyInFullScreenMode)
                return;

            if (Game.graphicsDeviceManager.HardwareModeSwitch)
            {
                Game.GraphicsDevice.PresentationParameters.IsFullScreen = true;
                Game.GraphicsDevice.SetHardwareFullscreen();

                _window._form.WindowState = FormWindowState.Maximized;
            }
            else
            {
                _window.IsBorderless = true;
                _window._form.WindowState = FormWindowState.Maximized;
            }

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

                _window._form.WindowState = FormWindowState.Normal;

                Game.GraphicsDevice.PresentationParameters.BackBufferWidth = Game.graphicsDeviceManager.PreferredBackBufferWidth;
                Game.GraphicsDevice.PresentationParameters.BackBufferHeight = Game.graphicsDeviceManager.PreferredBackBufferHeight;

                Game.GraphicsDevice.OnPresentationChanged();
            }
            else
            {
                _window._form.WindowState = FormWindowState.Normal;
                _window.IsBorderless = false;
            }

            _alreadyInWindowedMode = true;
            _alreadyInFullScreenMode = false;
        }

        internal override void OnPresentationChanged()
        {
            var presentationParameters = Game.GraphicsDevice.PresentationParameters;
            
            if (presentationParameters.IsFullScreen)
                EnterFullScreen();
            else
                ExitFullScreen();                
            
            _window.ChangeClientSize(new Size(presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight));
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
