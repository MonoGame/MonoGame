// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Pss.Core.Input;

namespace Microsoft.Xna.Framework.Input
{
	public static class Keyboard
	{
        private static List<Keys> keys = new List<Keys>();
		/* TODO
        private static readonly IDictionary<Keycode, Keys> KeyMap = LoadKeyMap();

        public static void KeyDown(Keycode keyCode)
        {
            Keys key = KeyMap[keyCode];
            if (!keys.Contains(key)) keys.Add(key);            
        }

        public static void KeyUp(Keycode keyCode)
        {
            Keys key = KeyMap[keyCode];
            if (keys.Contains(key)) keys.Remove(key);            
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
			maps[Keycode.A] = Keys.A;
			maps[Keycode.Y] = Keys.Y;
			maps[Keycode.X] = Keys.X;
			maps[Keycode.B] = Keys.B;
			maps[Keycode.S] = Keys.S;
			maps[Keycode.W] = Keys.W;
			maps[Keycode.D] = Keys.D;
			maps[Keycode.Back] = Keys.Escape;
            maps[Keycode.Back] = Keys.Back;
            maps[Keycode.Home] = Keys.Home;
			maps[Keycode.Enter] = Keys.Enter;
            // TODO: put in all the other mappings
            maps[Keycode.Menu] = Keys.Help;
            maps[Keycode.Search] = Keys.BrowserSearch;
            maps[Keycode.VolumeUp] = Keys.VolumeUp;
            maps[Keycode.VolumeDown] = Keys.VolumeDown;
            return maps;
        }
		 */
	    public static KeyboardState GetState()
		{
			return new KeyboardState(keys.ToArray()); // TODO Not used on iPhone or Zune
		}
		
		public static KeyboardState GetState(PlayerIndex playerIndex)
		{
            return new KeyboardState(keys.ToArray());  // TODO Not used on iPhone or Zune
		}
	}
}
