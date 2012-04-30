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



using System;
using System.Collections.Generic;

using Sce.Pss.Core.Input;
using Sce.Pss.Core.Graphics;

using PssGamePad = Sce.Pss.Core.Input.GamePad;
using PssGamePadButtons = Sce.Pss.Core.Input.GamePadButtons;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



﻿namespace Microsoft.Xna.Framework.Input
{
    public class GamePad
    {
		private static GamePad _instance;
		private int _buttons;
		private Vector2 _leftStick, _rightStick;
		
		#region PssButtons -> Buttons Map
		private static Dictionary<PssGamePadButtons, Buttons> _buttonsMap = new Dictionary<PssGamePadButtons, Buttons>{
			{ PssGamePadButtons.Cross, Buttons.A },
			{ PssGamePadButtons.Circle, Buttons.B },
			{ PssGamePadButtons.Square, Buttons.X },
			{ PssGamePadButtons.Triangle, Buttons.Y },
			
			{ PssGamePadButtons.Left, Buttons.DPadLeft },
			{ PssGamePadButtons.Right, Buttons.DPadRight },
			{ PssGamePadButtons.Up, Buttons.DPadUp },
			{ PssGamePadButtons.Down, Buttons.DPadDown },
			
			{ PssGamePadButtons.Select, Buttons.Back },
			{ PssGamePadButtons.Start, Buttons.Start },
			{ PssGamePadButtons.L, Buttons.LeftShoulder },
			{ PssGamePadButtons.R, Buttons.RightShoulder }
		};
		#endregion
		
		protected GamePad()
		{
			Reset();
		}
		
		internal static GamePad Instance 
		{
			get 
			{
				if (_instance == null) 
				{
					_instance = new GamePad();
				}
				return _instance;
			}
		}
		
		public void Reset()
		{
			_buttons = 0;
			_leftStick = Vector2.Zero;
			_rightStick = Vector2.Zero;
		}
		
        public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            GamePadCapabilities capabilities = new GamePadCapabilities();
			capabilities.IsConnected = (playerIndex == PlayerIndex.One);
			
			capabilities.HasAButton = true;
			capabilities.HasBButton = true;
			capabilities.HasXButton = true;
			capabilities.HasYButton = true;
			
			capabilities.HasBackButton = true;
			capabilities.HasStartButton = true;
			
			capabilities.HasDPadUpButton = true;
			capabilities.HasDPadDownButton = true;
			capabilities.HasDPadLeftButton = true;
			capabilities.HasDPadRightButton = true;
			
			capabilities.HasLeftXThumbStick = true;
			capabilities.HasLeftYThumbStick = true;
			capabilities.HasRightXThumbStick = true;
			capabilities.HasRightYThumbStick = true;
			
			capabilities.HasLeftShoulderButton = true;
			capabilities.HasRightShoulderButton = true;
			
			return capabilities;
        }

        internal void Update()
        {
			//See PSS GamePadSample for details
			
			var gamePadData = PssGamePad.GetData(0);
			
			//map the buttons
			foreach (var kvp in _buttonsMap)
			{
				if ((gamePadData.Buttons & kvp.Key) != 0)
					_buttons |= (int)kvp.Value;
			}
			
			//Analog sticks
			_leftStick = new Vector2(gamePadData.AnalogLeftX, gamePadData.AnalogLeftY);
			_rightStick = new Vector2(gamePadData.AnalogRightX, gamePadData.AnalogRightY);
		}

		public static GamePadState GetState(PlayerIndex playerIndex)
        {
            var instance = GamePad.Instance;
			instance.Update();
            var state = new GamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons((Buttons)instance._buttons), new GamePadDPad((Buttons)instance._buttons));
            instance.Reset();
            return state;
        }

        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
			return false; //No support on the Vita
        }
	}
	
}
