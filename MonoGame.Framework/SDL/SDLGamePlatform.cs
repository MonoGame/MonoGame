// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
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
        private readonly List<Keys> _keys;

        private int _isExiting;
        private SdlGameWindow _view;

        public SdlGamePlatform(Game game)
            : base(game)
        {
            _game = game;
            _keys = new List<Keys>();
            Keyboard.SetKeys(_keys);

            Sdl.Version sversion;
            Sdl.GetVersion(out sversion);

            Sdl.Major = sversion.Major;
            Sdl.Minor = sversion.Minor;
            Sdl.Patch = sversion.Patch;

            var version = 100 * Sdl.Major + 10 * Sdl.Minor + Sdl.Patch;

            if (version <= 204)
                Debug.WriteLine ("Please use SDL 2.0.5 or higher.");

            // Needed so VS can debug the project on Windows
            if (version >= 205 && CurrentPlatform.OS == OS.Windows && Debugger.IsAttached)
                Sdl.SetHint("SDL_WINDOWS_DISABLE_THREAD_NAMING", "1");

            Sdl.Init((int)(
                Sdl.InitFlags.Video |
                Sdl.InitFlags.Joystick |
                Sdl.InitFlags.GameController |
                Sdl.InitFlags.Haptic
            ));

            Sdl.DisableScreenSaver();

            GamePad.InitDatabase();
            Window = _view = new SdlGameWindow(_game);
        }

        public override void BeforeInitialize ()
        {
            SdlRunLoop();

            base.BeforeInitialize ();
        }

        protected override void OnIsMouseVisibleChanged()
        {
            _view.SetCursorVisible(_game.IsMouseVisible);
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            var displayIndex = Sdl.Window.GetDisplayIndex(Window.Handle);
            var displayName = Sdl.Display.GetDisplayName(displayIndex);
            BeginScreenDeviceChange(pp.IsFullScreen);
            EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight);
        }

        public override void RunLoop()
        {
            Sdl.Window.Show(Window.Handle);

            while (true)
            {
                SdlRunLoop();
                Game.Tick();
                Threading.Run();
                GraphicsDevice.DisposeContexts();

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
                {
                    const int wheelDelta = 120;
                    Mouse.ScrollY += ev.Wheel.Y * wheelDelta;
                    Mouse.ScrollX += ev.Wheel.X * wheelDelta;
                }
                else if (ev.Type == Sdl.EventType.MouseMotion)
                {
                    Window.MouseState.X = ev.Motion.X;
                    Window.MouseState.Y = ev.Motion.Y;
                }
                else if (ev.Type == Sdl.EventType.KeyDown)
                {
                    var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                    if (!_keys.Contains(key))
                        _keys.Add(key);
                    char character = (char)ev.Key.Keysym.Sym;
                    if (char.IsControl(character))
                        _view.CallTextInput(character, key);
                }
                else if (ev.Type == Sdl.EventType.KeyUp)
                {
                    var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                    _keys.Remove(key);
                }
                else if (ev.Type == Sdl.EventType.TextInput)
                {
                    if (_view.IsTextInputHandled)
                    {
                        int len = 0;
                        int utf8character = 0; // using an int to encode multibyte characters longer than 2 bytes
                        byte currentByte = 0;
                        int charByteSize = 0; // UTF8 char lenght to decode
                        unsafe
                        {
                            while ((currentByte = Marshal.ReadByte((IntPtr)ev.Text.Text, len)) != 0)
                            {
                                // we're reading the first UTF8 byte, we need to check if it's multibyte
                                if (charByteSize == 0)
                                {
                                    if (currentByte < 192)
                                        charByteSize = 1;
                                    else if (currentByte < 224)
                                        charByteSize = 2;
                                    else if (currentByte < 240)
                                        charByteSize = 3;
                                    else
                                        charByteSize = 4;

                                    utf8character = 0;
                                }

                                // assembling the character
                                utf8character <<= 8;
                                utf8character |= currentByte;

                                charByteSize--;
                                if (charByteSize == 0) // finished decoding the current character
                                {
                                    var key = KeyboardUtil.ToXna(utf8character);
                                    // multibyte conversion might fail here, because the char type only handles UTF8 up to 2-byte characters
                                    // we should switch to a String parameter for this event to fully support UTF8
                                    // as-is, it won't support CJK characters and will produce wrong results
                                    _view.CallTextInput((char)utf8character, key);
                                }

                                len++;
                            }
                        }
                    }
                }
                else if (ev.Type == Sdl.EventType.WindowEvent)
                {
                    if (ev.Window.WindowID == _view.Id)
                    {
                        if (ev.Window.EventID == Sdl.Window.EventId.Resized || ev.Window.EventID == Sdl.Window.EventId.SizeChanged)
                            _view.ClientResize(ev.Window.Data1, ev.Window.Data2);
                        else if (ev.Window.EventID == Sdl.Window.EventId.FocusGained)
                            IsActive = true;
                        else if (ev.Window.EventID == Sdl.Window.EventId.FocusLost)
                            IsActive = false;
                        else if (ev.Window.EventID == Sdl.Window.EventId.Moved)
                            _view.Moved();
                        else if (ev.Window.EventID == Sdl.Window.EventId.Close)
                            _isExiting++;
                    }
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
