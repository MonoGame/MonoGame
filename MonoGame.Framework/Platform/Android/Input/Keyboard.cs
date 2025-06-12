// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;

namespace Microsoft.Xna.Framework.Input
{
	public static class Keyboard
	{
        private static List<Keys> keys = new List<Keys>();

        private static readonly IDictionary<Keycode, Keys> KeyMap = LoadKeyMap();

        internal static bool KeyDown(Keycode keyCode)
        {
            Keys key;
            if (KeyMap.TryGetValue(keyCode, out key) && key != Keys.None)
            {
                if (!keys.Contains(key))
                    keys.Add(key);
                return true;
            }
            return false;
        }

        internal static bool KeyUp(Keycode keyCode)
        {
            Keys key;
            if (KeyMap.TryGetValue(keyCode, out key) && key != Keys.None)
            {
                if (keys.Contains(key))
                    keys.Remove(key);
                return true;
            }
            return false;
        }

        private static IDictionary<Keycode, Keys> LoadKeyMap()
        {
            // create a map for every Keycode and default it to none so that every possible key is mapped
            var maps = Enum.GetValues(typeof (Keycode))
                .Cast<Keycode>()
                .ToDictionary(key => key, key => Keys.None);

            // then update it with the actual mappings
            maps[Keycode.DpadLeft] = Keys.Left;
            maps[Keycode.DpadRight] = Keys.Right;
            maps[Keycode.DpadUp] = Keys.Up;
            maps[Keycode.DpadDown] = Keys.Down;
            maps[Keycode.DpadCenter] = Keys.Enter;
            maps[Keycode.Num0] = Keys.D0;
            maps[Keycode.Num1] = Keys.D1;
            maps[Keycode.Num2] = Keys.D2;
            maps[Keycode.Num3] = Keys.D3;
            maps[Keycode.Num4] = Keys.D4;
            maps[Keycode.Num5] = Keys.D5;
            maps[Keycode.Num6] = Keys.D6;
            maps[Keycode.Num7] = Keys.D7;
            maps[Keycode.Num8] = Keys.D8;
            maps[Keycode.Num9] = Keys.D9;
            maps[Keycode.A] = Keys.A;
            maps[Keycode.B] = Keys.B;
            maps[Keycode.C] = Keys.C;
            maps[Keycode.D] = Keys.D;
            maps[Keycode.E] = Keys.E;
            maps[Keycode.F] = Keys.F;
            maps[Keycode.G] = Keys.G;
            maps[Keycode.H] = Keys.H;
            maps[Keycode.I] = Keys.I;
            maps[Keycode.J] = Keys.J;
            maps[Keycode.K] = Keys.K;
            maps[Keycode.L] = Keys.L;
            maps[Keycode.M] = Keys.M;
            maps[Keycode.N] = Keys.N;
            maps[Keycode.O] = Keys.O;
            maps[Keycode.P] = Keys.P;
            maps[Keycode.Q] = Keys.Q;
            maps[Keycode.R] = Keys.R;
            maps[Keycode.S] = Keys.S;
            maps[Keycode.T] = Keys.T;
            maps[Keycode.U] = Keys.U;
            maps[Keycode.V] = Keys.V;
            maps[Keycode.W] = Keys.W;
            maps[Keycode.X] = Keys.X;
            maps[Keycode.Y] = Keys.Y;
            maps[Keycode.Z] = Keys.Z;
            maps[Keycode.Space] = Keys.Space;
            maps[Keycode.Escape] = Keys.Escape;
            maps[Keycode.Back] = Keys.Back;
            maps[Keycode.Home] = Keys.Home;
            maps[Keycode.Enter] = Keys.Enter;
            maps[Keycode.Period] = Keys.OemPeriod;
            maps[Keycode.Comma] = Keys.OemComma;
            maps[Keycode.Menu] = Keys.Help;
            maps[Keycode.Search] = Keys.BrowserSearch;
            maps[Keycode.VolumeUp] = Keys.VolumeUp;
            maps[Keycode.VolumeDown] = Keys.VolumeDown;
            maps[Keycode.MediaPause] = Keys.Pause;
            maps[Keycode.MediaPlayPause] = Keys.MediaPlayPause;
            maps[Keycode.MediaStop] = Keys.MediaStop;
            maps[Keycode.MediaNext] = Keys.MediaNextTrack;
            maps[Keycode.MediaPrevious] = Keys.MediaPreviousTrack;
            maps[Keycode.Mute] = Keys.VolumeMute;
            maps[Keycode.AltLeft] = Keys.LeftAlt;
            maps[Keycode.AltRight] = Keys.RightAlt;
            maps[Keycode.ShiftLeft] = Keys.LeftShift;
            maps[Keycode.ShiftRight] = Keys.RightShift;
            maps[Keycode.Tab] = Keys.Tab;
            maps[Keycode.Del] = Keys.Delete;
            maps[Keycode.Minus] = Keys.OemMinus;
            maps[Keycode.LeftBracket] = Keys.OemOpenBrackets;
            maps[Keycode.RightBracket] = Keys.OemCloseBrackets;
            maps[Keycode.Backslash] = Keys.OemBackslash;
            maps[Keycode.Semicolon] = Keys.OemSemicolon;
            maps[Keycode.PageUp] = Keys.PageUp;
            maps[Keycode.PageDown] = Keys.PageDown;
            maps[Keycode.CtrlLeft] = Keys.LeftControl;
            maps[Keycode.CtrlRight] = Keys.RightControl;
            maps[Keycode.CapsLock] = Keys.CapsLock;
            maps[Keycode.ScrollLock] = Keys.Scroll;
            maps[Keycode.NumLock] = Keys.NumLock;
            maps[Keycode.Insert] = Keys.Insert;
            maps[Keycode.F1] = Keys.F1;
            maps[Keycode.F2] = Keys.F2;
            maps[Keycode.F3] = Keys.F3;
            maps[Keycode.F4] = Keys.F4;
            maps[Keycode.F5] = Keys.F5;
            maps[Keycode.F6] = Keys.F6;
            maps[Keycode.F7] = Keys.F7;
            maps[Keycode.F8] = Keys.F8;
            maps[Keycode.F9] = Keys.F9;
            maps[Keycode.F10] = Keys.F10;
            maps[Keycode.F11] = Keys.F11;
            maps[Keycode.F12] = Keys.F12;
            maps[Keycode.NumpadDivide] = Keys.Divide;
            maps[Keycode.NumpadMultiply] = Keys.Multiply;
            maps[Keycode.NumpadSubtract] = Keys.Subtract;
            maps[Keycode.NumpadAdd] = Keys.Add;

            return maps;
        }

	    public static KeyboardState GetState()
		{
			return new KeyboardState(keys);
		}
		
		public static KeyboardState GetState(PlayerIndex playerIndex)
		{
            return new KeyboardState(keys);
		}
	}
}
