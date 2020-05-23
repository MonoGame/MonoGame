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

        private SdlImeHandler _imeHandler;

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
                Debug.WriteLine("Please use SDL 2.0.5 or higher.");

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
            _imeHandler = new SdlImeHandler(game);
            Window.ImmService = _imeHandler;
        }

        public override void BeforeInitialize()
        {
            SdlRunLoop();

            base.BeforeInitialize();
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
                switch (ev.Type)
                {
                    case Sdl.EventType.Quit:
                        _isExiting++;
                        break;
                    case Sdl.EventType.JoyDeviceAdded:
                        Joystick.AddDevice(ev.JoystickDevice.Which);
                        break;
                    case Sdl.EventType.JoyDeviceRemoved:
                        Joystick.RemoveDevice(ev.JoystickDevice.Which);
                        break;
                    case Sdl.EventType.ControllerDeviceRemoved:
                        GamePad.RemoveDevice(ev.ControllerDevice.Which);
                        break;
                    case Sdl.EventType.ControllerButtonUp:
                    case Sdl.EventType.ControllerButtonDown:
                    case Sdl.EventType.ControllerAxisMotion:
                        GamePad.UpdatePacketInfo(ev.ControllerDevice.Which, ev.ControllerDevice.TimeStamp);
                        break;
                    case Sdl.EventType.MouseWheel:
                        const int wheelDelta = 120;
                        Mouse.ScrollY += ev.Wheel.Y * wheelDelta;
                        Mouse.ScrollX += ev.Wheel.X * wheelDelta;
                        break;
                    case Sdl.EventType.MouseMotion:
                        Window.MouseState.X = ev.Motion.X;
                        Window.MouseState.Y = ev.Motion.Y;
                        break;
                    case Sdl.EventType.KeyDown:
                    {
                        var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                        if (!_keys.Contains(key))
                            _keys.Add(key);
                        char character = (char)ev.Key.Keysym.Sym;
                        _view.OnKeyDown(new InputKeyEventArgs(key));
                        if (char.IsControl(character))
                            _view.OnTextInput(new TextInputEventArgs(character, key));
                        break;
                    }
                    case Sdl.EventType.KeyUp:
                    {
                        var key = KeyboardUtil.ToXna(ev.Key.Keysym.Sym);
                        _keys.Remove(key);
                        _view.OnKeyUp(new InputKeyEventArgs(key));
                        break;
                    }
                    case Sdl.EventType.TextEditing:
                    {
                        unsafe
                        {
                            var cursorPosition = ev.Edit.Start;
                            var compositionString = SDLBufferToString(ev.Edit.Text);
                            _imeHandler.OnTextComposition(compositionString, cursorPosition);
                        }
                        break;
                    }
                    case Sdl.EventType.TextInput:
                        // Mimic a CompositionEnd event
                        _imeHandler.OnTextComposition(null, 0);

                        unsafe
                        {
                            var text = SDLBufferToString(ev.Text.Text); // This way to support emoji.
                            foreach (var c in text)
                            {
                                _imeHandler.OnTextInput(c, KeyboardUtil.ToXna(c));

                                // Forward text input event to GameWindow.TextInput for backward compability.
                                if (_view.IsTextInputHandled)
                                    _view.OnTextInput(new TextInputEventArgs((char)c, KeyboardUtil.ToXna(c)));
                            }
                        }
                        break;
                    case Sdl.EventType.WindowEvent:

                        switch (ev.Window.EventID)
                        {
                            case Sdl.Window.EventId.Resized:
                            case Sdl.Window.EventId.SizeChanged:
                                _view.ClientResize(ev.Window.Data1, ev.Window.Data2);
                                break;
                            case Sdl.Window.EventId.FocusGained:
                                IsActive = true;
                                break;
                            case Sdl.Window.EventId.FocusLost:
                                IsActive = false;
                                break;
                            case Sdl.Window.EventId.Moved:
                                _view.Moved();
                                break;
                            case Sdl.Window.EventId.Close:
                                _isExiting++;
                                break;
                        }
                        break;
                }
            }
        }

        private int UTF8ToUnicode(int utf8)
        {
            int
                byte4 = utf8 & 0xFF,
                byte3 = (utf8 >> 8) & 0xFF,
                byte2 = (utf8 >> 16) & 0xFF,
                byte1 = (utf8 >> 24) & 0xFF;

            if (byte1 < 0x80)
                return byte1;
            else if (byte1 < 0xC0)
                return -1;
            else if (byte1 < 0xE0 && byte2 >= 0x80 && byte2 < 0xC0)
                return (byte1 % 0x20) * 0x40 + (byte2 % 0x40);
            else if (byte1 < 0xF0 && byte2 >= 0x80 && byte2 < 0xC0 && byte3 >= 0x80 && byte3 < 0xC0)
                return (byte1 % 0x10) * 0x40 * 0x40 + (byte2 % 0x40) * 0x40 + (byte3 % 0x40);
            else if (byte1 < 0xF8 && byte2 >= 0x80 && byte2 < 0xC0 && byte3 >= 0x80 && byte3 < 0xC0 && byte4 >= 0x80 && byte4 < 0xC0)
                return (byte1 % 0x8) * 0x40 * 0x40 * 0x40 + (byte2 % 0x40) * 0x40 * 0x40 + (byte3 % 0x40) * 0x40 + (byte4 % 0x40);
            else
                return -1;
        }

        private unsafe string SDLBufferToString(byte* text, int size = 32)
        {
            byte[] sourceBytes = new byte[size];
            int length = 0;

            for (int i = 0; i < size; i++)
            {
                if (text[i] == 0)
                    break;

                sourceBytes[i] = text[i];
                length++;
            }

            return System.Text.Encoding.UTF8.GetString(sourceBytes, 0, length);
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
