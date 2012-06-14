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
using System.Linq;
using System.Text;
using System.IO;
using Tao.Sdl;
using System.Xml.Serialization;

namespace Microsoft.Xna.Framework.Input
{
    //
    // Summary:
    //     Allows retrieval of user interaction with an Xbox 360 Controller and setting
    //     of controller vibration motors. Reference page contains links to related
    //     code samples.
    public static class GamePad
    {
		static bool running;		
        static bool sdl;

        static Settings settings;
        static Settings Settings
        {
        	get
            {
                return PrepSettings();
            }
        }

		static void AutoConfig () 
        {
		        Init();
				if (!sdl) return;
				Console.WriteLine("Number of joysticks: " + Sdl.SDL_NumJoysticks());
					int numSticks = Sdl.SDL_NumJoysticks();
					for (int x = 0; x < numSticks; x++) {

						PadConfig pc = new PadConfig(Sdl.SDL_JoystickName(x), 0);
						devices[x] = Sdl.SDL_JoystickOpen (pc.Index);

						int numbuttons = Sdl.SDL_JoystickNumButtons(devices[x]);
						Console.WriteLine("Number of buttons for joystick: " + x + " - " + numbuttons);

						for (int b = 0; b < numbuttons; b++) {
							//pc
						}
						
						if (Sdl.SDL_JoystickName(x).Contains("Microsoft") ||
							(Sdl.SDL_JoystickName(x).Contains("X-Box") || Sdl.SDL_JoystickName(x).Contains("Xbox")))
						{
							pc.Button_A.ID = 0;
							pc.Button_A.Type = InputType.Button;

							pc.Button_B.ID = 1;
							pc.Button_B.Type = InputType.Button;

							pc.Button_X.ID = 2;
							pc.Button_X.Type = InputType.Button;

							pc.Button_Y.ID = 3;
							pc.Button_Y.Type = InputType.Button;

							pc.Button_Back.ID = 6;
							pc.Button_Back.Type = InputType.Button;

							pc.Button_Start.ID = 7;
							pc.Button_Start.Type = InputType.Button;

							pc.Button_LB.ID = 4;
							pc.Button_LB.Type = InputType.Button;

							pc.Button_RB.ID = 5;
							pc.Button_RB.Type = InputType.Button;

							pc.LeftStick.X.Negative.Type = InputType.Axis;
							pc.LeftStick.X.Negative.Negative = true;
							pc.LeftStick.X.Positive.Type = InputType.Axis;
							pc.LeftStick.X.Positive.Negative = false;

							pc.LeftStick.Y.Negative.ID = 1;
							pc.LeftStick.Y.Negative.Type = InputType.Axis;
							pc.LeftStick.Y.Negative.Negative = true;

							pc.LeftStick.Y.Positive.ID = 1;
							pc.LeftStick.Y.Positive.Type = InputType.Axis;
							pc.LeftStick.Y.Positive.Negative = false;

							pc.RightStick.X.Negative.ID = 3;
							pc.RightStick.X.Negative.Type = InputType.Axis;
							pc.RightStick.X.Negative.Negative = true;

							pc.RightStick.X.Positive.ID = 3;
							pc.RightStick.X.Positive.Type = InputType.Axis;
							pc.RightStick.X.Positive.Negative = false;

							pc.RightStick.Y.Negative.ID = 4;
							pc.RightStick.Y.Negative.Type = InputType.Axis;
							pc.RightStick.Y.Negative.Negative = true;

							pc.RightStick.Y.Positive.ID = 4;
							pc.RightStick.Y.Positive.Type = InputType.Axis;
							pc.RightStick.Y.Positive.Negative = false;

							pc.Dpad.Up.ID = 0;
							pc.Dpad.Up.Type = InputType.PovUp;

							pc.Dpad.Down.ID = 0;
							pc.Dpad.Down.Type = InputType.PovDown;

							pc.Dpad.Left.ID = 0;
							pc.Dpad.Left.Type = InputType.PovLeft;

							pc.Dpad.Right.ID = 0;
							pc.Dpad.Right.Type = InputType.PovRight;

							pc.LeftTrigger.ID = 2;
							pc.LeftTrigger.Type = InputType.Axis;
							// Only positive value

							pc.RightTrigger.ID = 5;
							pc.RightTrigger.Type = InputType.Axis;
							// Only positive value
							
							// Suggestion: Xbox Guide button <=> BigButton
							//pc.BigButton.ID = 8;
							//pc.BigButton.Type = InputType.Button;

							pc.LeftStick.Press.ID = 9;
							pc.LeftStick.Press.Type = InputType.Button;

							pc.RightStick.Press.ID = 10;
							pc.RightStick.Press.Type = InputType.Button;
						}
						
						else
						{
							//pc.Button_A = new Input();
							pc.Button_A.ID = 0;
							pc.Button_A.Type = InputType.Button;

							pc.Button_B.ID = 1;
							pc.Button_B.Type = InputType.Button;

							pc.Button_X.ID = 2;
							pc.Button_X.Type = InputType.Button;

							pc.Button_Y.ID = 3;
							pc.Button_Y.Type = InputType.Button;

							pc.Button_Back.ID = 8;
							pc.Button_Back.Type = InputType.Button;

							pc.Button_Start.ID = 9;
							pc.Button_Start.Type = InputType.Button;

							pc.Button_LB.ID = 4;
							pc.Button_LB.Type = InputType.Button;

							pc.Button_RB.ID = 5;
							pc.Button_RB.Type = InputType.Button;

							pc.LeftStick.X.Negative.Type = InputType.Axis;
							pc.LeftStick.X.Negative.Negative = true;
							pc.LeftStick.X.Positive.Type = InputType.Axis;
							pc.LeftStick.X.Positive.Negative = false;

							pc.LeftStick.Y.Negative.ID = 1;
							pc.LeftStick.Y.Negative.Type = InputType.Axis;
							pc.LeftStick.Y.Negative.Negative = true;

							pc.LeftStick.Y.Positive.ID = 1;
							pc.LeftStick.Y.Positive.Type = InputType.Axis;
							pc.LeftStick.Y.Positive.Negative = false;

							//pc.RightStick.X.Negative.Type = InputType.Axis;
							//pc.RightStick.X.Negative.Negative = true;
							//pc.RightStick.X.Positive.Type = InputType.Axis;
							//pc.RightStick.X.Positive.Negative = false;

							//pc.RightStick.Y.Negative.ID = 1;
							//pc.RightStick.Y.Negative.Type = InputType.Axis;
							//pc.RightStick.Y.Negative.Negative = true;

							//pc.RightStick.Y.Positive.ID = 1;
							//pc.RightStick.Y.Positive.Type = InputType.Axis;
							//pc.RightStick.Y.Positive.Negative = false;

							pc.Dpad.Up.ID = 0;
							pc.Dpad.Up.Type = InputType.PovUp;

							pc.Dpad.Down.ID = 0;
							pc.Dpad.Down.Type = InputType.PovDown;

							pc.Dpad.Left.ID = 0;
							pc.Dpad.Left.Type = InputType.PovLeft;

							pc.Dpad.Right.ID = 0;
							pc.Dpad.Right.Type = InputType.PovRight;

							//pc.LeftTrigger.ID = 6;
							//pc.LeftTrigger.Type = InputType.Button;

							pc.RightTrigger.ID = 7;
							pc.RightTrigger.Type = InputType.Button;
						}

						int numaxes = Sdl.SDL_JoystickNumAxes(devices[x]);
						Console.WriteLine("Number of axes for joystick: " + x + " - " + numaxes);

						for (int a = 0; a < numaxes; a++) {
							//pc.LeftStick = new Stick();
						}

						int numhats = Sdl.SDL_JoystickNumHats(devices[x]);
						Console.WriteLine("Number of PovHats for joystick: " + x + " - " + numhats);

						for (int h = 0; h < numhats; h++) {
							//pc
						}
						settings[x] = pc;
			}
		}

        static Settings PrepSettings()
        {
            if (settings == null)
            {
                    settings = new Settings();
					AutoConfig();		
            }
            else if (!running)
            {
                Init();
                return settings;
            }
            if (!running)
                Init();
            return settings;
        }
        

        static IntPtr[] devices = new IntPtr[4];
        //Inits SDL and grabs the sticks
        static void Init ()
        {
        	running = true;
		    try 
            {
         	    Joystick.Init ();
				sdl = true;
			}
			catch (Exception) 
            {

			}
        	for (int i = 0; i < 4; i++)
            {
        		PadConfig pc = settings[i];
        		if (pc != null)
                {
        			devices[i] = Sdl.SDL_JoystickOpen (pc.Index);
			    }
		    }


        }
        //Disposes of SDL
        static void Cleanup()
        {
            Joystick.Cleanup();
            running = false;
        }

        static IntPtr GetDevice(PlayerIndex index)
        {
            return devices[(int)index];
        }

        static PadConfig GetConfig(PlayerIndex index)
        {
            return Settings[(int)index];
        }

        static Buttons ReadButtons(IntPtr device, PadConfig c, float deadZoneSize)
        {
            short DeadZone = (short)(deadZoneSize * short.MaxValue);
            Buttons b = (Buttons)0;

            if (c.Button_A.ReadBool(device, DeadZone))
                b |= Buttons.A;
            if (c.Button_B.ReadBool(device, DeadZone))
                b |= Buttons.B;
            if (c.Button_X.ReadBool(device, DeadZone))
                b |= Buttons.X;
            if (c.Button_Y.ReadBool(device, DeadZone))
                b |= Buttons.Y;

            if (c.Button_LB.ReadBool(device, DeadZone))
                b |= Buttons.LeftShoulder;
            if (c.Button_RB.ReadBool(device, DeadZone))
                b |= Buttons.RightShoulder;

            if (c.Button_Back.ReadBool(device, DeadZone))
                b |= Buttons.Back;
            if (c.Button_Start.ReadBool(device, DeadZone))
                b |= Buttons.Start;

            if (c.LeftStick.Press.ReadBool(device, DeadZone))
                b |= Buttons.LeftStick;
            if (c.RightStick.Press.ReadBool(device, DeadZone))
                b |= Buttons.RightStick;

            if (c.Dpad.Up.ReadBool(device, DeadZone))
                b |= Buttons.DPadUp;
            if (c.Dpad.Down.ReadBool(device, DeadZone))
                b |= Buttons.DPadDown;
            if (c.Dpad.Left.ReadBool(device, DeadZone))
                b |= Buttons.DPadLeft;
            if (c.Dpad.Right.ReadBool(device, DeadZone))
                b |= Buttons.DPadRight;

            return b;
        }
		
		static Buttons StickToButtons( Vector2 stick, Buttons left, Buttons right, Buttons up , Buttons down, float DeadZoneSize )
		{
			Buttons b = (Buttons)0;

			if ( stick.X > DeadZoneSize )
				b |= right;
			if ( stick.X < -DeadZoneSize )
				b |= left;
			if ( stick.Y > DeadZoneSize )
				b |= up;
			if ( stick.Y < -DeadZoneSize )
				b |= down;
			
			return b;
		}
		
		static Buttons TriggerToButton( float trigger, Buttons button, float DeadZoneSize )
		{
			Buttons b = (Buttons)0;

			if ( trigger > DeadZoneSize )
				b |= button;

			return b;
		}
		
        static GamePadState ReadState(PlayerIndex index, GamePadDeadZone deadZone)
        {
            const float DeadZoneSize = 0.27f;
            IntPtr device = GetDevice(index);
            PadConfig c = GetConfig(index);
            if (device == IntPtr.Zero || c == null)
                return GamePadState.InitializedState;

            var leftStick = c.LeftStick.ReadAxisPair(device);
            var rightStick = c.RightStick.ReadAxisPair(device);
            GamePadThumbSticks sticks = new GamePadThumbSticks(new Vector2(leftStick.X, leftStick.Y), new Vector2(rightStick.X, rightStick.Y));
            sticks.ApplyDeadZone(deadZone, DeadZoneSize);
            GamePadTriggers triggers = new GamePadTriggers(c.LeftTrigger.ReadFloat(device), c.RightTrigger.ReadFloat(device));
			Buttons buttonState = ReadButtons(device, c, DeadZoneSize);
			buttonState |= StickToButtons(sticks.Left, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown, DeadZoneSize);
			buttonState |= StickToButtons(sticks.Right, Buttons.RightThumbstickLeft, Buttons.RightThumbstickRight, Buttons.RightThumbstickUp, Buttons.RightThumbstickDown, DeadZoneSize);
			buttonState |= TriggerToButton(triggers.Left, Buttons.LeftTrigger, DeadZoneSize);
			buttonState |= TriggerToButton(triggers.Right, Buttons.RightTrigger, DeadZoneSize);
            GamePadButtons buttons = new GamePadButtons(buttonState);
            GamePadDPad dpad = new GamePadDPad(buttons.buttons);

            GamePadState g = new GamePadState(sticks, triggers, buttons, dpad);
            return g;
        }

        //
        // Summary:
        //     Retrieves the capabilities of an Xbox 360 Controller.
        //
        // Parameters:
        //   playerIndex:
        //     Index of the controller to query.
        public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            IntPtr d = GetDevice(playerIndex);
            PadConfig c = GetConfig(playerIndex);

            if (c == null || ((c.JoystickName == null || c.JoystickName == string.Empty) && d == IntPtr.Zero))
                return new GamePadCapabilities();

            return new GamePadCapabilities()
            {
                IsConnected = d != IntPtr.Zero,
                HasAButton = c.Button_A.Type != InputType.None,
                HasBButton = c.Button_B.Type != InputType.None,
                HasXButton = c.Button_X.Type != InputType.None,
                HasYButton = c.Button_Y.Type != InputType.None,
                HasBackButton = c.Button_Back.Type != InputType.None,
                HasStartButton = c.Button_Start.Type != InputType.None,
                HasDPadDownButton = c.Dpad.Down.Type != InputType.None,
                HasDPadLeftButton = c.Dpad.Left.Type != InputType.None,
                HasDPadRightButton = c.Dpad.Right.Type != InputType.None,
                HasDPadUpButton = c.Dpad.Up.Type != InputType.None,
                HasLeftShoulderButton = c.Button_LB.Type != InputType.None,
                HasRightShoulderButton = c.Button_RB.Type != InputType.None,
                HasLeftStickButton = c.LeftStick.Press.Type != InputType.None,
                HasRightStickButton = c.RightStick.Press.Type != InputType.None,
                HasLeftTrigger = c.LeftTrigger.Type != InputType.None,
                HasRightTrigger = c.RightTrigger.Type != InputType.None,
                HasLeftXThumbStick = c.LeftStick.X.Type != InputType.None,
                HasLeftYThumbStick = c.LeftStick.Y.Type != InputType.None,
                HasRightXThumbStick = c.RightStick.X.Type != InputType.None,
                HasRightYThumbStick = c.RightStick.Y.Type != InputType.None,

                HasLeftVibrationMotor = false,
                HasRightVibrationMotor = false,
                HasVoiceSupport = false,
                HasBigButton = false
            };
        }
        //
        // Summary:
        //     Gets the current state of a game pad controller. Reference page contains
        //     links to related code samples.
        //
        // Parameters:
        //   playerIndex:
        //     Player index for the controller you want to query.
        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            return GetState(playerIndex, GamePadDeadZone.IndependentAxes);
        }
        //
        // Summary:
        //     Gets the current state of a game pad controller, using a specified dead zone
        //     on analog stick positions. Reference page contains links to related code
        //     samples.
        //
        // Parameters:
        //   playerIndex:
        //     Player index for the controller you want to query.
        //
        //   deadZoneMode:
        //     Enumerated value that specifies what dead zone type to use.
        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode)
        {
            PrepSettings();
            if (sdl)
				Sdl.SDL_JoystickUpdate();
            return ReadState(playerIndex, deadZoneMode);
        }
        //
        // Summary:
        //     Sets the vibration motor speeds on an Xbox 360 Controller. Reference page
        //     contains links to related code samples.
        //
        // Parameters:
        //   playerIndex:
        //     Player index that identifies the controller to set.
        //
        //   leftMotor:
        //     The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency
        //     motor.
        //
        //   rightMotor:
        //     The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency
        //     motor.
        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            return false;
        }
    }
}