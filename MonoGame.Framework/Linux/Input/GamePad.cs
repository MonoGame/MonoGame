using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GamepadConfigLib;
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

		static void AutoConfig () {
						Init();
				if (!sdl)
					return;
				Console.WriteLine("Number of joysticks: " + Sdl.SDL_NumJoysticks());
					int numSticks = Sdl.SDL_NumJoysticks();
					for (int x = 0; x < numSticks; x++) {

						PadConfig pc = new PadConfig(Sdl.SDL_JoystickName(x), 0);
						devices[x] = Sdl.SDL_JoystickOpen (pc.ID);

						int numbuttons = Sdl.SDL_JoystickNumButtons(devices[x]);
						Console.WriteLine("Number of buttons for joystick: " + x + " - " + numbuttons);

						for (int b = 0; b < numbuttons; b++) {
							//pc
						}
						
						if (Sdl.SDL_JoystickName(x).Contains("Microsoft") &&
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
						
						else if (Sdl.SDL_JoystickName(x).Contains("Sony PLAYSTATION(R)3"))
						{
							pc.Button_A.ID = 14;
							pc.Button_A.Type = InputType.Button;

							pc.Button_B.ID = 13;
							pc.Button_B.Type = InputType.Button;

							pc.Button_X.ID = 15;
							pc.Button_X.Type = InputType.Button;

							pc.Button_Y.ID = 12;
							pc.Button_Y.Type = InputType.Button;

							pc.Button_Back.ID = 0;
							pc.Button_Back.Type = InputType.Button;

							pc.Button_Start.ID = 3;
							pc.Button_Start.Type = InputType.Button;

							pc.Button_LB.ID = 10;
							pc.Button_LB.Type = InputType.Button;

							pc.Button_RB.ID = 11;
							pc.Button_RB.Type = InputType.Button;

							pc.LeftStick.X.Negative.ID = 0;
							pc.LeftStick.X.Negative.Type = InputType.Axis;
							pc.LeftStick.X.Negative.Negative = true;

							pc.LeftStick.X.Positive.ID = 0;
							pc.LeftStick.X.Positive.Type = InputType.Axis;
							pc.LeftStick.X.Positive.Negative = false;

							pc.LeftStick.Y.Negative.ID = 1;
							pc.LeftStick.Y.Negative.Type = InputType.Axis;
							pc.LeftStick.Y.Negative.Negative = true;

							pc.LeftStick.Y.Positive.ID = 1;
							pc.LeftStick.Y.Positive.Type = InputType.Axis;
							pc.LeftStick.Y.Positive.Negative = false;

							pc.RightStick.X.Negative.ID = 2;
							pc.RightStick.X.Negative.Type = InputType.Axis;
							pc.RightStick.X.Negative.Negative = true;

							pc.RightStick.X.Positive.ID = 2;
							pc.RightStick.X.Positive.Type = InputType.Axis;
							pc.RightStick.X.Positive.Negative = false;

							pc.RightStick.Y.Negative.ID = 3;
							pc.RightStick.Y.Negative.Type = InputType.Axis;
							pc.RightStick.Y.Negative.Negative = true;

							pc.RightStick.Y.Positive.ID = 3;
							pc.RightStick.Y.Positive.Type = InputType.Axis;
							pc.RightStick.Y.Positive.Negative = false;

							pc.Dpad.Up.ID = 8;
							pc.Dpad.Up.Type = InputType.PovUp;

							pc.Dpad.Down.ID = 10;
							pc.Dpad.Down.Type = InputType.PovDown;

							pc.Dpad.Left.ID = 11;
							pc.Dpad.Left.Type = InputType.PovLeft;

							pc.Dpad.Right.ID = 9;
							pc.Dpad.Right.Type = InputType.PovRight;

							pc.LeftTrigger.ID = 12;
							pc.LeftTrigger.Type = InputType.Axis;
							// Only positive value

							pc.RightTrigger.ID = 13;
							pc.RightTrigger.Type = InputType.Axis;
							// Only positive value
							
							//pc.BigButton.ID = 16;
							//pc.BigButton.Type = InputType.Button;

							pc.LeftStick.Press.ID = 1;
							pc.LeftStick.Press.Type = InputType.Button;

							pc.RightStick.Press.ID = 2;
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
                settings = LoadConfigs("Settings.xml");
                if (settings == null) {
                    settings = new Settings();
					AutoConfig();
		}
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
        static Settings LoadConfigs(string filename)
        {
            Settings e;
            try
            {
                using (Stream s = File.OpenRead(filename))
                {
                    XmlSerializer x = new XmlSerializer(typeof(Settings));
                    e = (Settings)x.Deserialize(s);
                }
            }
            catch
            {
                return null;
            }
            //for (int i = 0; i < 4; i++)
              //  if (e[i] == null)
                //    e[i] = new PadConfig(true);
            return e;
        }

        static IntPtr[] devices = new IntPtr[4];
        //Inits SDL and grabs the sticks
        static void Init ()
        {
        	running = true;
		try {
        	Joystick.Init ();
				sdl = true;
			}
			catch (Exception exc) {

			}

        	for (int i = 0; i < 4; i++)
            	{
        		PadConfig pc = settings[i];
        		if (pc != null)
                	{
        			devices[i] = Sdl.SDL_JoystickOpen (pc.ID);
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

        static Buttons StickToButtons(Vector2 stick, float DeadZoneSize)
        {
            Buttons b = (Buttons)0;

            if (stick.X > DeadZoneSize)
                b |= Buttons.LeftThumbstickRight;
            if (stick.X < -DeadZoneSize)
                b |= Buttons.LeftThumbstickLeft;
            if (stick.Y > DeadZoneSize)
                b |= Buttons.LeftThumbstickUp;
            if (stick.Y < -DeadZoneSize)
                b |= Buttons.LeftThumbstickDown;

            return b;
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
        static Buttons ReadButtons(IntPtr device, PadConfig c, float deadZoneSize, Vector2 leftStick, Vector2 rightStick)
        {
            Buttons b = ReadButtons(device, c, deadZoneSize);

            b |= StickToButtons(leftStick, deadZoneSize);
            b |= StickToButtons(rightStick, deadZoneSize);

            return b;
        }

        static GamePadState ReadState(PlayerIndex index, GamePadDeadZone deadZone)
        {
            const float DeadZoneSize = 0.27f;
            IntPtr device = GetDevice(index);
            PadConfig c = GetConfig(index);
            if (device == IntPtr.Zero || c == null)
                return GamePadState.InitializedState;

            GamePadThumbSticks sticks = new GamePadThumbSticks(new Vector2(c.LeftStick.ReadAxisPair(device)), new Vector2(c.RightStick.ReadAxisPair(device)));
            sticks.ApplyDeadZone(deadZone, DeadZoneSize);
            GamePadTriggers triggers = new GamePadTriggers(c.LeftTrigger.ReadFloat(device), c.RightTrigger.ReadFloat(device));
            GamePadButtons buttons = new GamePadButtons(ReadButtons(device, c, DeadZoneSize));
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
