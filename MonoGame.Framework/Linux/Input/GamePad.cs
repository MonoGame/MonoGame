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
        static Settings settings;
        static Settings Settings
        {
        	get
            {
        		if (settings == null)
                {
        			settings = LoadConfigs ("Settings.xml");
                    if (settings == null)
                        settings = new Settings();
        		}
                else if (!running)
                {
        			Init ();
        			return settings;
				}
				return settings;
            }
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

        static void Init ()
        {
        	running = true;
        	Joystick.Init ();
        	for (int i = 0; i < 4; i++)
            {
        		PadConfig pc = settings[i];
        		if (pc != null)
                {
        			devices[i] = Sdl.SDL_JoystickOpen (pc.ID);
				}
            }
        }
        static void Cleanup()
        {
            Joystick.Cleanup();
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
                return new GamePadState();

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
