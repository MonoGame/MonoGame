
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
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
#else
using AppKit;
#endif

using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
	public static class KeyUtil
	{
		private static IDictionary keyNames;
		private static IDictionary modifiers;
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
			keyNames = new Hashtable ();
			modifiers = new Hashtable ();

			keyNames.Add (NSKey.Backslash, Keys.OemBackslash);
			keyNames.Add (NSKey.CapsLock, Keys.CapsLock);
			keyNames.Add (NSKey.Comma, Keys.OemComma);
			keyNames.Add (NSKey.Command, Keys.LeftWindows);
			keyNames.Add (NSKey.Delete, Keys.Back);
			keyNames.Add (NSKey.DownArrow, Keys.Down);
			keyNames.Add (NSKey.Equal, Keys.OemPlus);
			keyNames.Add (NSKey.ForwardDelete, Keys.Delete);
			keyNames.Add (NSKey.Keypad0, Keys.NumPad0);
			keyNames.Add (NSKey.Keypad1, Keys.NumPad1);
			keyNames.Add (NSKey.Keypad2, Keys.NumPad2);
			keyNames.Add (NSKey.Keypad3, Keys.NumPad3);
			keyNames.Add (NSKey.Keypad4, Keys.NumPad4);
			keyNames.Add (NSKey.Keypad5, Keys.NumPad5);
			keyNames.Add (NSKey.Keypad6, Keys.NumPad6);
			keyNames.Add (NSKey.Keypad7, Keys.NumPad7);
			keyNames.Add (NSKey.Keypad8, Keys.NumPad8);
			keyNames.Add (NSKey.Keypad9, Keys.NumPad9);
			keyNames.Add (NSKey.KeypadDecimal, Keys.Decimal);
			keyNames.Add (NSKey.KeypadDivide, Keys.Divide);
			keyNames.Add (NSKey.KeypadEnter, Keys.Enter);
			keyNames.Add (NSKey.KeypadEquals, Keys.OemPlus);
			keyNames.Add (NSKey.KeypadMinus, Keys.OemMinus);
			keyNames.Add (NSKey.KeypadMultiply, Keys.Multiply);
			keyNames.Add (NSKey.KeypadPlus, Keys.OemPlus);
			keyNames.Add (NSKey.LeftArrow, Keys.Left);
			keyNames.Add (NSKey.LeftBracket, Keys.OemOpenBrackets);
			keyNames.Add (NSKey.Minus, Keys.OemMinus);
			keyNames.Add (NSKey.Mute, Keys.VolumeMute);
#if PLATFORM_MACOS_LEGACY
            keyNames.Add (NSKey.Next, Keys.MediaNextTrack);
#endif
			keyNames.Add (NSKey.Option, Keys.LeftAlt);
#if PLATFORM_MACOS_LEGACY
			keyNames.Add (NSKey.Pause, Keys.MediaPlayPause);
			keyNames.Add (NSKey.Prev, Keys.MediaPreviousTrack);
#endif
			keyNames.Add (NSKey.Quote, Keys.OemQuotes);
			keyNames.Add (NSKey.RightArrow, Keys.Right);
			keyNames.Add (NSKey.RightBracket, Keys.OemCloseBrackets);
			keyNames.Add (NSKey.RightControl, Keys.RightControl);
			keyNames.Add (NSKey.RightOption, Keys.RightAlt);
			keyNames.Add (NSKey.RightShift, Keys.RightShift);
#if PLATFORM_MACOS_LEGACY
			keyNames.Add (NSKey.ScrollLock, Keys.Scroll);
#endif
			keyNames.Add (NSKey.Semicolon, Keys.OemSemicolon);
			keyNames.Add (NSKey.Slash, Keys.OemQuestion);
			keyNames.Add (NSKey.UpArrow, Keys.Up);
			keyNames.Add (NSKey.Period, Keys.OemPeriod);
			keyNames.Add (NSKey.Return, Keys.Enter);
			keyNames.Add (NSKey.Grave, Keys.OemTilde);
			
			// Modifiers
			modifiers.Add ("524576", Keys.LeftAlt);
			modifiers.Add ("65792", Keys.CapsLock);			
			modifiers.Add ("524608", Keys.LeftWindows);
			modifiers.Add ("262401", Keys.LeftControl);
			modifiers.Add ("131332", Keys.RightShift);
			modifiers.Add ("131330", Keys.LeftShift);
			modifiers.Add ("655650", Keys.RightShift);
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
			var nskey = Enum.ToObject (typeof(NSKey), theEvent.KeyCode);
			if ((theEvent.ModifierFlags & NSEventModifierMask.FunctionKeyMask) > 0) {
				var chars = theEvent.Characters.ToCharArray ();
				var thekey = chars [0];
				if (theEvent.KeyCode != (char)NSKey.ForwardDelete) {

					nskey = (NSKey)Enum.ToObject (typeof(NSKey), thekey);
				}
			}

			try {
				var key = (Keys)keyNames [nskey];
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