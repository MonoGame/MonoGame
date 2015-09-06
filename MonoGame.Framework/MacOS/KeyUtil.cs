
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2011 James Clancey
//
// Authors:
//	James Clancey
//
// Modified for use by:
//	Kenneth James Pouncey
//
//
using System;
using System.Linq;
using System.Collections;
using MonoMac.AppKit;

using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	public static class KeyUtil
	{
        private static Dictionary<NSKey, Keys> keyNames;
        private static Dictionary<string, Keys> modifiers;
		private static bool initialized;
		
		static KeyUtil() 
		{
			Initialize();
		}
		
		private static void Initialize ()
		{
			if (initialized)
				return;
			initialized = true;
            keyNames = new Dictionary<NSKey, Keys>
            {
                { NSKey.Backslash, Keys.OemBackslash },
                { NSKey.CapsLock, Keys.CapsLock },
                { NSKey.Comma, Keys.OemComma },
                { NSKey.Command, Keys.LeftWindows },
                { NSKey.Delete, Keys.Back },
                { NSKey.DownArrow, Keys.Down },
                { NSKey.Equal, Keys.OemPlus },
                { NSKey.ForwardDelete, Keys.Delete },
                { NSKey.Keypad0, Keys.NumPad0 },
                { NSKey.Keypad1, Keys.NumPad1 },
                { NSKey.Keypad2, Keys.NumPad2 },
                { NSKey.Keypad3, Keys.NumPad3 },
                { NSKey.Keypad4, Keys.NumPad4 },
                { NSKey.Keypad5, Keys.NumPad5 },
                { NSKey.Keypad6, Keys.NumPad6 },
                { NSKey.Keypad7, Keys.NumPad7 },
                { NSKey.Keypad8, Keys.NumPad8 },
                { NSKey.Keypad9, Keys.NumPad9 },
                { NSKey.KeypadDecimal, Keys.Decimal },
                { NSKey.KeypadDivide, Keys.Divide },
                { NSKey.KeypadEnter, Keys.Enter },
                { NSKey.KeypadEquals, Keys.OemPlus },
                { NSKey.KeypadMinus, Keys.OemMinus },
                { NSKey.KeypadMultiply, Keys.Multiply },
                { NSKey.KeypadPlus, Keys.OemPlus },
                { NSKey.LeftArrow, Keys.Left },
                { NSKey.LeftBracket, Keys.OemOpenBrackets },
                { NSKey.Minus, Keys.OemMinus },
                { NSKey.Mute, Keys.VolumeMute },
                { NSKey.Next, Keys.MediaNextTrack },
                { NSKey.Option, Keys.LeftAlt },
                { NSKey.Pause, Keys.MediaPlayPause },
                { NSKey.Prev, Keys.MediaPreviousTrack },
                { NSKey.Quote, Keys.OemQuotes },
                { NSKey.RightArrow, Keys.Right },
                { NSKey.RightBracket, Keys.OemCloseBrackets },
                { NSKey.RightControl, Keys.RightControl },
                { NSKey.RightOption, Keys.RightAlt },
                { NSKey.RightShift, Keys.RightShift },
                { NSKey.ScrollLock, Keys.Scroll },
                { NSKey.Semicolon, Keys.OemSemicolon },
                { NSKey.Slash, Keys.OemQuestion },
                { NSKey.UpArrow, Keys.Up },
                { NSKey.Period, Keys.OemPeriod },
                { NSKey.Return, Keys.Enter },
                { NSKey.Grave, Keys.OemTilde },
            };
            modifiers = new Dictionary<string, Keys>
            {
                { "524576", Keys.LeftAlt },
                { "65792", Keys.CapsLock },
                { "524608", Keys.LeftWindows },
                { "262401", Keys.LeftControl },
                { "131332", Keys.RightShift },
                { "131330", Keys.LeftShift },
                { "655650", Keys.RightShift },
            };

			
			/*
			keyNames.Add("+",Keys.Add);
			keyNames.Add("524576",Keys.Alt);
			keyNames.Add("65792",Keys.CapsLock);
			keyNames.Add("0", Keys.D0);
			keyNames.Add("1", Keys.D1);
			keyNames.Add("2", Keys.D2);
			keyNames.Add("3", Keys.D3);
			keyNames.Add("4", Keys.D4);
			keyNames.Add("5", Keys.D5);
			keyNames.Add("6", Keys.D6);
			keyNames.Add("7", Keys.D7);
			keyNames.Add("8", Keys.D8);
			keyNames.Add("9", Keys.D9);
			keyNames.Add("256",Keys.Back);
			keyNames.Add("." , Keys.Decimal);
			keyNames.Add(NSKey.Delete,Keys.Delete);			
			keyNames.Add(NSKey.Space,Keys.Delete);
			keyNames.Add("/",Keys.Divide);
			keyNames.Add(NSKey.DownArrow,Keys.Down);
			keyNames.Add("\r",Keys.Enter);

			keyNames.Add("524608",Keys.LWin);
			keyNames.Add("262401",Keys.ControlKey);
			//keyNames.Add(NSEventModifierMask.FunctionKeyMask,Keys.f
			keyNames.Add(NSEventModifierMask.HelpKeyMask,Keys.Help);
			keyNames.Add("131332",Keys.RShiftKey | Keys.Shift);
			keyNames.Add("131330",Keys.LShiftKey | Keys.Shift);
			keyNames.Add("655650",Keys.Shift | Keys.Alt);
			*/

		}

		[CLSCompliant(false)]
		public static Keys GetKeys (NSEvent theEvent)
		{
			//Initialize ();
            var nskey = (NSKey)Enum.ToObject (typeof(NSKey), theEvent.KeyCode);
			if ((theEvent.ModifierFlags & NSEventModifierMask.FunctionKeyMask) > 0) {
				var chars = theEvent.Characters.ToCharArray ();
				var thekey = chars [0];
				if (theEvent.KeyCode != (char)NSKey.ForwardDelete) {

					nskey = (NSKey)Enum.ToObject (typeof(NSKey), thekey);
				}
			}

			try {
				Keys key = keyNames [nskey];
				return key;
			} catch {
				try {
					// Works if the keys have the same name;
					var key = (Keys)Enum.Parse (typeof(Keys), nskey.ToString ());
					return key;
				} catch {
					// None found
					return Keys.None;	
				}
			}

			//Works based on Character
			/*
			//NSKey nskey =   (NSKey)theEvent.KeyCode;
			var foundMod = keyNames[theEvent.ModifierFlags.ToString()];
			Keys mod = foundMod == null ? 0 : (Keys)foundMod;

			var foundkey = keyNames[theEvent.Characters];
			Keys key = foundkey == null ? 0 : (Keys)foundkey;
			if (key == 0)
			{
				var keyName =  Enum.GetNames(typeof(Keys)).Where(x=> x == theEvent.Characters.ToUpper()).FirstOrDefault();
				if(string.IsNullOrEmpty(keyName))
				{
					return mod != 0 ? Keys.None | mod : Keys.None;
				}
				var theKey =  (Keys)Enum.Parse(typeof(Keys),keyName);
				return mod != 0 ? theKey | mod : theKey;
			}
			return mod != 0 ? key | mod : key;
			*/
		}


	}
}