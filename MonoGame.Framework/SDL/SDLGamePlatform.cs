// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    class SDLGamePlatform : GamePlatform
    {
        private OpenALSoundController soundControllerInstance = null;
        private int isExiting;
        private SDLGameWindow _view;
        private List<Keys> _keys;
        private Game _game;

        public SDLGamePlatform(Game game)
            : base(game)
        {
            this._game = game;
            this._keys = new List<Keys>();
            Keyboard.SetKeys(_keys);

            var initsdl = SDL.SDL_Init((int)(
                SDL.SDL_INIT_VIDEO |
                SDL.SDL_INIT_JOYSTICK | 
                SDL.SDL_INIT_GAMECONTROLLER |
                SDL.SDL_INIT_HAPTIC
            ));

            if (initsdl < 0)
                throw new Exception("SDL could not initialize! SDL Error: " + SDL.SDL_GetError());

            this.Window = _view = new SDLGameWindow();

            try
            {
                soundControllerInstance = OpenALSoundController.GetInstance;
            }
            catch (DllNotFoundException ex)
            {
                throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
            }
        }

        public override bool BeforeRun()
        {
            _view.CreateWindow();

            return base.BeforeRun();
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _view.SetCursorVisible(_game.IsMouseVisible);
        }

        public override void RunLoop()
        {
            SDL.SDL_ShowWindow(Window.Handle);

            while (true)
            {
                SDL.SDL_Event ev;

                while (SDL.SDL_PollEvent(out ev) == 1)
                {
                    if (ev.type == SDL.SDL_EventType.SDL_QUIT)
                        isExiting++;
                    else if (ev.type == SDL.SDL_EventType.SDL_JOYDEVICEADDED)
                        Joystick.AddDevice(ev.jdevice.which);
                    else if (ev.type == SDL.SDL_EventType.SDL_JOYDEVICEREMOVED)
                        Joystick.RemoveDevice(ev.jdevice.which);
                    else if (ev.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
                        Mouse.ScrollY += ev.wheel.y * 120;
                    else if (ev.type == SDL.SDL_EventType.SDL_KEYDOWN)
                    {
                        var key = KeyboardUtil.ToXna(ev.key.keysym.sym);

                        if (!_keys.Contains(key))
                            _keys.Add(key);
                    }
                    else if (ev.type == SDL.SDL_EventType.SDL_KEYUP)
                    {
                        var key = KeyboardUtil.ToXna(ev.key.keysym.sym);
                        _keys.Remove(key);
                    }
                }

                Game.Tick();

                if (isExiting > 0)
                    break;
            }

            Dispose();
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The desktop platform does not support asynchronous run loops");
        }

        public override void Exit()
        {
            Interlocked.Increment(ref isExiting);
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            // Update our OpenAL sound buffer pools
            if (soundControllerInstance != null)
                soundControllerInstance.Update();
            
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

        protected override void Dispose(bool disposing)
        {
            if (_view != null)
            {
                _view.Dispose();
                _view = null;

                Joystick.CloseDevices();

                SDL.SDL_Quit();
            }

            base.Dispose(disposing);
        }
    }
}
