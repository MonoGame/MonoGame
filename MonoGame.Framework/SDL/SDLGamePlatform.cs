// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework
{
    internal class SdlGamePlatform : GamePlatform
    {
        public override GameRunBehavior DefaultRunBehavior
        {
            get { return GameRunBehavior.Synchronous; }
        }

        private readonly Game _game;
        private readonly OpenALSoundController _soundControllerInstance;
        private readonly List<Keys> _keys;

        private int _isExiting;
        private SdlGameWindow _view;

        public SdlGamePlatform(Game game)
            : base(game)
        {
            // if we're on Windows, we need to detect the CPU arch and load the correct dlls
            // on other system, the MonoGame.Framework.dll.config handles this
            if (PlatformParameters.DetectWindowsArchitecture)
                NativeHelper.InitDllDirectory();

            _game = game;
            _keys = new List<Keys>();
            Keyboard.SetKeys(_keys);

            Sdl.Version sversion;
            Sdl.GetVersion(out sversion);

            Sdl.Major = sversion.Major;
            Sdl.Minor = sversion.Minor;
            Sdl.Patch = sversion.Patch;

            try
            {
                // HACK: The current development version of SDL
                // returns 2.0.4, to check SDL version we simply
                // need to try and execute a function that's only
                // available in the newer version of it.
                Sdl.Window.SetResizable(IntPtr.Zero, false);
                Sdl.Patch = 5;
            }
            catch { }

            var version = 100 * Sdl.Major + 10 * Sdl.Minor + Sdl.Patch;

            if (version <= 204)
                Debug.WriteLine ("Please use SDL 2.0.5 or higher.");

            Sdl.Init((int)(
                Sdl.InitFlags.Video |
                Sdl.InitFlags.Joystick |
                Sdl.InitFlags.GameController |
                Sdl.InitFlags.Haptic
            ));

            Sdl.DisableScreenSaver();

            Window = _view = new SdlGameWindow(_game);

            try
            {
                _soundControllerInstance = OpenALSoundController.GetInstance;
            }
            catch (DllNotFoundException ex)
            {
                throw (new NoAudioHardwareException("Failed to init OpenALSoundController", ex));
            }
        }

        public override void BeforeInitialize ()
        {
            GamePad.InitDatabase();
            _view.CreateWindow();
            SdlRunLoop();

            base.BeforeInitialize ();
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _view.SetCursorVisible(_game.IsMouseVisible);
        }

        public override void RunLoop()
        {
            Sdl.Window.Show(Window.Handle);

            while (true)
            {
                SdlRunLoop();
                Game.Tick();

                if (_isExiting > 0)
                    break;
            }
        }

        private void SdlRunLoop()
        {
            Sdl.Event ev;

            while (Sdl.PollEvent(out ev) == 1)
            {
                if (ev.Type == Sdl.EventType.Quit)
                    _isExiting++;
                else if (ev.Type == Sdl.EventType.JoyDeviceAdded)
                    Joystick.AddDevice(ev.JoystickDevice.Which);
                else if (ev.Type == Sdl.EventType.ControllerDeviceRemoved)
                    GamePad.RemoveDevice(ev.ControllerDevice.Which);
                else if (ev.Type == Sdl.EventType.JoyDeviceRemoved)
                    Joystick.RemoveDevice(ev.JoystickDevice.Which);                
                else if (ev.Type == Sdl.EventType.MouseWheel)
                    Mouse.ScrollY += ev.Wheel.Y * 120;
                else if (ev.Type == Sdl.EventType.KeyDown) {
                    var key = KeyboardUtil.ToXna (ev.Key.Keysym.Sym);
                    if (!_keys.Contains (key))
                        _keys.Add (key);
                    char character = (char)ev.Key.Keysym.Sym;
                    if (char.IsControl (character))
                        _view.CallTextInput (character, key);
                }
                else if (ev.Type == Sdl.EventType.KeyUp)
                {
                    var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                    _keys.Remove(key);
                }
                else if (ev.Type == Sdl.EventType.TextInput)
                {
                    string text;
                    unsafe
                    {
                        text = new string((char*)ev.Text.Text);
                    }
                    if (text.Length == 0)
                        continue;
                    foreach (var c in text)
                    {
                        var key = KeyboardUtil.ToXna ((int)c);
                        _view.CallTextInput (c, key);
                    }
                }
                else if (ev.Type == Sdl.EventType.WindowEvent)
                {
                    if (ev.Window.EventID == Sdl.Window.EventId.Resized || ev.Window.EventID == Sdl.Window.EventId.SizeChanged)
                        _view.ClientResize(ev.Window.Data1, ev.Window.Data2);
                    else if (ev.Window.EventID == Sdl.Window.EventId.FocusGained)
                        IsActive = true;
                    else if (ev.Window.EventID == Sdl.Window.EventId.FocusLost)
                        IsActive = false;
                }
            }
        }

        public override void StartRunLoop()
        {
            throw new NotSupportedException("The desktop platform does not support asynchronous run loops");
        }

        public override void Exit()
        {
            Interlocked.Increment(ref _isExiting);
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

        public override void Log(string message)
        {
            Console.WriteLine(message);
        }

        public override void Present()
        {
            if (Game.GraphicsDevice != null)
                Game.GraphicsDevice.Present();
        }

        protected override void Dispose(bool disposing)
        {
            if (_view != null)
            {
                _view.Dispose();
                _view = null;

                Joystick.CloseDevices();

                Sdl.Quit();
            }

            base.Dispose(disposing);
        }
    }
}
