// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using OpenTK;

namespace Microsoft.Xna.Framework
{
    class OpenTKGamePlatform : GamePlatform
    {
        private OpenTKGameWindow _view;
        private OpenALSoundController _soundControllerInstance;

        private int isExiting;

        //int windowDelay = 2;
        
		public OpenTKGamePlatform(Game game)
            : base(game)
        {
            this.Window = _view = new OpenTKGameWindow(game);

            try
            {
                _soundControllerInstance = OpenALSoundController.GetInstance;
            }
            catch (DllNotFoundException ex)
            {
                throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
            }
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _view.SetCursorVisible(IsMouseVisible);
        }

        public override void RunLoop()
        {
            while (true)
            {
                _view.ProcessEvents();
                Game.Tick();

                if (isExiting > 0)
                {
                    Game.Exit();
                    break;
                }
            }
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The desktop platform does not support asynchronous run loops");
        }

        public override void Exit()
        {
            Interlocked.Increment(ref isExiting);

            DisplayDevice.Default.RestoreResolution();
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            IsActive = _view.Window.Focused;

            if (_soundControllerInstance != null)
                _soundControllerInstance.Update();
            
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
            
        }

        public override void ExitFullScreen()
        {
            
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _view.BeginScreenDeviceChange(willBeFullScreen);
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _view.EndScreenDeviceChange(screenDeviceName, clientWidth, clientHeight);
        }
        
        public override void Log(string Message)
        {
            Console.WriteLine(Message);
        }

        public override void Present()
        {
            var device = Game.GraphicsDevice;
            if (device != null)
                device.Present();
        }	
    }
}
