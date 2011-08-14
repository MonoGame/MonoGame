#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using Microsoft.Xna.Framework;
using System;

namespace Microsoft.Xna.Framework.Input
{
	public struct GamePadState
    {
        private GamePadThumbSticks _thumbs;
        private Buttons _buttons;
		private GamePadDPad _dPad;
		private GamePadTriggers _triggers;

		internal GamePadState(Buttons buttons, Vector2 LeftStick, Vector2 RightStick)
		{
			_buttons = buttons;
			_thumbs = new GamePadThumbSticks(LeftStick,RightStick);
		}

        public GamePadState(GamePadThumbSticks thumbs, GamePadTriggers triggers, GamePadButtons gamePadButtons, GamePadDPad dPad)
        {
            _thumbs = thumbs;
            _triggers = triggers;
            ConvertGamePadButtonsToButtons(ref gamePadButtons, out _buttons);
            _dPad = dPad;
        }
		
        public GamePadButtons Buttons
        {
            get
            {
                return new GamePadButtons(_buttons);
            }
        }
 
        public bool IsConnected
        {
            get
            {
                return true;
            }
        }
       
        public GamePadThumbSticks ThumbSticks
        {
            get
            {
                return this._thumbs;
            }
        }
     
        public bool IsButtonDown(Microsoft.Xna.Framework.Input.Buttons button)
        {
            return ((_buttons & button) == button);
        }

        public bool IsButtonUp(Microsoft.Xna.Framework.Input.Buttons button)
        {
            return !this.IsButtonDown(button);
        }
			
		public GamePadDPad DPad 
		{ 
			get
			{
				return _dPad;
			}
		}
		
		public GamePadTriggers Triggers 
		{ 
			get
			{
				return _triggers;
			}
		}

        internal static void ConvertGamePadButtonsToButtons(ref GamePadButtons gamePadButtons, out Buttons buttons)
        {
            buttons = new Buttons();
            if (gamePadButtons.A == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.A;
            }
            if (gamePadButtons.B == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.B;
            }
            if (gamePadButtons.X == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.X;
            }
            if (gamePadButtons.Y == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.Y;
            }
            if (gamePadButtons.Back == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.Back;
            }
            if (gamePadButtons.Start == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.Start;
            }
            if (gamePadButtons.BigButton == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.BigButton;
            }
            if (gamePadButtons.LeftShoulder == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.LeftShoulder;
            }
            if (gamePadButtons.RightShoulder == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.RightShoulder;
            }
            if (gamePadButtons.LeftStick == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.LeftStick;
            }
            if (gamePadButtons.RightStick == ButtonState.Pressed)
            {
                buttons |= Microsoft.Xna.Framework.Input.Buttons.RightStick;
            }
        }
    }

}

