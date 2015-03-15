// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Sce.PlayStation.Core.Input;
using Sce.PlayStation.Core.Graphics;
using PssGamePad = Sce.PlayStation.Core.Input.GamePad;
using PssGamePadButtons = Sce.PlayStation.Core.Input.GamePadButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


﻿namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
		#region PSMButtons -> Buttons Map
		private static readonly Dictionary<PssGamePadButtons, Buttons> _buttonsMap = new Dictionary<PssGamePadButtons, Buttons>{
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

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            GamePadCapabilities capabilities = new GamePadCapabilities();
            capabilities.IsConnected = (index == 0);
			
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

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            // PSM only has a single player game pad.
            if (index != 0)
                return new GamePadState();

            //See PSS GamePadSample for details

            var buttons = 0;
            var leftStick = Vector2.Zero;
            var rightStick = Vector2.Zero;
            try
            {
                var gamePadData = PssGamePad.GetData(0);

                //map the buttons
                foreach (var kvp in _buttonsMap)
                {
                    if ((gamePadData.Buttons & kvp.Key) != 0)
                        buttons |= (int)kvp.Value;
                }

                //Analog sticks
                leftStick = new Vector2(gamePadData.AnalogLeftX, -gamePadData.AnalogLeftY);
                rightStick = new Vector2(gamePadData.AnalogRightX, -gamePadData.AnalogRightY);
            }
            catch (Sce.PlayStation.Core.InputSystemException exc)
            {
                if (exc.Message.ToLowerInvariant().Trim() == "native function returned error.")
                    throw new InvalidOperationException("GamePad must be listed in your features list in app.xml in order to use the GamePad API on PlayStation Mobile.", exc);
                else
                    throw;
            }

            var state = new GamePadState(new GamePadThumbSticks(leftStick, rightStick, deadZoneMode), new GamePadTriggers(), new GamePadButtons((Buttons)buttons), new GamePadDPad((Buttons)buttons));

            return state;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
			return false; //No support on the Vita
        }
	}	
}
