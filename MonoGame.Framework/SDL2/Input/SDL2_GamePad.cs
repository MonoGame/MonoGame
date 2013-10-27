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

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework.Input
{
    #region Internal Configuration Enumerations/Structures

    // FIXME: Basically everything in here is public when it shouldn't be.

    public enum InputType
    {
        PovUp =     1,
        PovRight =  1 << 1,
        PovDown =   1 << 2,
        PovLeft =   1 << 3,
        Button =    1 << 4,
        Axis =      1 << 5,
        None =      -1
    }

    [Serializable]
    public struct MonoGameJoystickValue
    {
        public InputType INPUT_TYPE;
        public int INPUT_ID;
        public bool INPUT_INVERT;
    }
    
    [Serializable]
    public struct MonoGameJoystickConfig
    {
        // public MonoGameJoystickValue BUTTON_GUIDE;
        public MonoGameJoystickValue BUTTON_START;
        public MonoGameJoystickValue BUTTON_BACK;
        public MonoGameJoystickValue BUTTON_A;
        public MonoGameJoystickValue BUTTON_B;
        public MonoGameJoystickValue BUTTON_X;
        public MonoGameJoystickValue BUTTON_Y;
        public MonoGameJoystickValue SHOULDER_LB;
        public MonoGameJoystickValue SHOULDER_RB;
        public MonoGameJoystickValue TRIGGER_RT;
        public MonoGameJoystickValue TRIGGER_LT;
        public MonoGameJoystickValue BUTTON_LSTICK;
        public MonoGameJoystickValue BUTTON_RSTICK;
        public MonoGameJoystickValue DPAD_UP;
        public MonoGameJoystickValue DPAD_DOWN;
        public MonoGameJoystickValue DPAD_LEFT;
        public MonoGameJoystickValue DPAD_RIGHT;
        public MonoGameJoystickValue AXIS_LX;
        public MonoGameJoystickValue AXIS_LY;
        public MonoGameJoystickValue AXIS_RX;
        public MonoGameJoystickValue AXIS_RY;
    }

    #endregion
    
    //
    // Summary:
    //     Allows retrieval of user interaction with an Xbox 360 Controller and setting
    //     of controller vibration motors. Reference page contains links to related
    //     code samples.
    public static class GamePad
    {
        #region Internal SDL2_GamePad Variables

        // The SDL device lists
        private static IntPtr[] INTERNAL_devices = new IntPtr[4];
        private static IntPtr[] INTERNAL_haptics = new IntPtr[4];
        private static bool[] INTERNAL_isGameController = new bool[4];
        private static Dictionary<int, int> INTERNAL_instanceList = new Dictionary<int, int>();
        
        // We use this to apply XInput-like rumble effects.
        private static SDL.SDL_HapticEffect INTERNAL_effect = new SDL.SDL_HapticEffect
        {
            type = SDL.SDL_HAPTIC_LEFTRIGHT,
            leftright = new SDL.SDL_HapticLeftRight
            {
                type = SDL.SDL_HAPTIC_LEFTRIGHT,
                length = SDL.SDL_HAPTIC_INFINITY,
                large_magnitude = ushort.MaxValue,
                small_magnitude = ushort.MaxValue
            }
        };
        
        // Where we will load our config file into.
        private static MonoGameJoystickConfig INTERNAL_joystickConfig;

        private static bool INTERNAL_wasInit = false;

        #endregion

        #region SDL Init/Quit Methods

        // Explicitly initialize the SDL Joystick/GameController subsystems
        private static bool Init()
        {
            return SDL.SDL_InitSubSystem(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_HAPTIC) == 0;
        }
        
        // Call this when you're done, if you don't want to depend on SDL_Quit();
        internal static void Cleanup()
        {
            if (SDL.SDL_WasInit(SDL.SDL_INIT_GAMECONTROLLER) == 1)
            {
                SDL.SDL_QuitSubSystem(SDL.SDL_INIT_GAMECONTROLLER);
            }
            if (SDL.SDL_WasInit(SDL.SDL_INIT_JOYSTICK) == 1)
            {
                SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);
            }
            if (SDL.SDL_WasInit(SDL.SDL_INIT_HAPTIC) == 1)
            {
                SDL.SDL_QuitSubSystem(SDL.SDL_INIT_HAPTIC);
            }
        }

        #endregion
        
        #region Device List, Open/Close Devices
        
        internal static void INTERNAL_AddInstance(int which)
        {
            if (which > 3)
            {
                return; // Ignoring more than 4 controllers.
            }
            
            // Clear the error buffer. We're about to do a LOT of dangerous stuff.
            SDL.SDL_ClearError();
            
            // We use this when dealing with Haptic initialization.
            IntPtr thisJoystick;

            // Initialize either a GameController or a Joystick.
            if (SDL.SDL_IsGameController(which) == SDL.SDL_bool.SDL_TRUE)
            {
                INTERNAL_isGameController[which] = true;
                INTERNAL_devices[which] = SDL.SDL_GameControllerOpen(which);
                thisJoystick = SDL.SDL_GameControllerGetJoystick(INTERNAL_devices[which]);
            }
            else
            {
                INTERNAL_isGameController[which] = false;
                INTERNAL_devices[which] = SDL.SDL_JoystickOpen(which);
                thisJoystick = INTERNAL_devices[which];
            }

            if (INTERNAL_devices[which] == IntPtr.Zero && thisJoystick == IntPtr.Zero)
            {
                // Crap, something went wrong.
                System.Console.WriteLine("JOYSTICK OPEN ERROR: " + SDL.SDL_GetError());
                return;
            }

            // Add the index, better known as the instance ID, to the dictionary.
            int instance = SDL.SDL_JoystickInstanceID(thisJoystick);
            if (INTERNAL_instanceList.ContainsKey(instance))
            {
                /* Some platforms (read: Windows) will try to open a joystick
                 * multiple times. Fortunately, SDL2 covers this, but we need
                 * our own set of checks to prevent duplicate entries.
                 * -flibit
                 */
                return;
            }
            INTERNAL_instanceList.Add(instance, which);

            // Initialize the haptics for each joystick.
            if (SDL.SDL_JoystickIsHaptic(thisJoystick) == 1)
            {
                INTERNAL_haptics[which] = SDL.SDL_HapticOpenFromJoystick(thisJoystick);
                if (INTERNAL_haptics[which] == IntPtr.Zero)
                {
                    System.Console.WriteLine("HAPTIC OPEN ERROR: " + SDL.SDL_GetError());
                }
            }
            if (INTERNAL_haptics[which] != IntPtr.Zero)
            {
                if (SDL.SDL_HapticEffectSupported(INTERNAL_haptics[which], ref INTERNAL_effect) == 1)
                {
                    SDL.SDL_HapticNewEffect(INTERNAL_haptics[which], ref INTERNAL_effect);
                }
                else if (SDL.SDL_HapticRumbleSupported(INTERNAL_haptics[which]) == 1)
                {
                    SDL.SDL_HapticRumbleInit(INTERNAL_haptics[which]);
                }
            }

            // Check for an SDL_GameController configuration first!
            if (INTERNAL_isGameController[which])
            {
                System.Console.WriteLine(
                    "Controller " + which + ", " +
                    SDL.SDL_GameControllerName(INTERNAL_devices[which]) +
                    ", will use SDL_GameController support."
                );
            }
            else
            {
                System.Console.WriteLine(
                    "Controller " + which + ", " +
                    SDL.SDL_JoystickName(INTERNAL_devices[which]) +
                    ", will use generic MonoGameJoystick support."
                );
            }
        }
        
        internal static void INTERNAL_RemoveInstance(int which)
        {
            int output;
            if (!INTERNAL_instanceList.TryGetValue(which, out output))
            {
                System.Console.WriteLine("Ignoring device removal, ID: " + which);
                return;
            }
            INTERNAL_instanceList.Remove(which);
            if (INTERNAL_haptics[output] != IntPtr.Zero)
            {
                SDL.SDL_HapticClose(INTERNAL_haptics[output]);
            }
            if (INTERNAL_isGameController[output])
            {
                // Not no mores, it ain't.
                INTERNAL_isGameController[output] = false;
                SDL.SDL_GameControllerClose(INTERNAL_devices[output]);
            }
            else
            {
                SDL.SDL_JoystickClose(INTERNAL_devices[output]);
            }
            INTERNAL_devices[output] = IntPtr.Zero;
            INTERNAL_haptics[output] = IntPtr.Zero;
            
            // A lot of errors can happen here, but honestly, they can be ignored...
            SDL.SDL_ClearError();
            
            System.Console.WriteLine("Removed device, player: " + output);
        }
        
        #endregion

        #region Haptic Support Check Helper
        
        // Convenience method to check for Rumble support
        private static bool INTERNAL_HapticSupported(PlayerIndex playerIndex)
        {
            IntPtr haptic = INTERNAL_haptics[(int) playerIndex];
            return (    haptic != IntPtr.Zero &&
                        (   SDL.SDL_HapticEffectSupported(haptic, ref INTERNAL_effect) == 1 ||
                            SDL.SDL_HapticRumbleSupported(haptic) == 1  )   );
        }

        #endregion

        #region Automatic Configuration Method
  
        // Prepare the MonoGameJoystick configuration system
        private static void INTERNAL_AutoConfig()
        {
            if (!Init())
            {
                return;
            }
            
            // Get the intended config file path.
            string osConfigFile = "";
            if (SDL2_GamePlatform.OSVersion.Equals("Windows"))
            {
                osConfigFile = "MonoGameJoystick.cfg"; // Oh well.
            }
            else if (SDL2_GamePlatform.OSVersion.Equals("Mac OS X"))
            {
                osConfigFile += Environment.GetEnvironmentVariable("HOME");
                if (osConfigFile.Length == 0)
                {
                    osConfigFile += "MonoGameJoystick.cfg"; // Oh well.
                }
                else
                {
                    osConfigFile += "/Library/Application Support/MonoGame/MonoGameJoystick.cfg";
                }
            }
            else if (SDL2_GamePlatform.OSVersion.Equals("Linux"))
            {
                // Assuming a non-OSX Unix platform will follow the XDG. Which it should.
                osConfigFile += Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (osConfigFile.Length == 0)
                {
                    osConfigFile += Environment.GetEnvironmentVariable("HOME");
                    if (osConfigFile.Length == 0)
                    {
                        osConfigFile += "MonoGameJoystick.cfg"; // Oh well.
                    }
                    else
                    {
                        osConfigFile += "/.config/MonoGame/MonoGameJoystick.cfg";
                    }
                }
                else
                {
                    osConfigFile += "/MonoGame/MonoGameJoystick.cfg";
                }
            }
            else
            {
                throw new Exception("SDL2_GamePad: SDL2 platform not handled!");
            }
            
            // Check to see if we've already got a config...
            if (File.Exists(osConfigFile))
            {
                // Load the file.
                FileStream fileIn = File.OpenRead(osConfigFile);
                
                // Load the data into our config struct.
                XmlSerializer serializer = new XmlSerializer(typeof(MonoGameJoystickConfig));
                INTERNAL_joystickConfig = (MonoGameJoystickConfig) serializer.Deserialize(fileIn);
                
                // We out.
                fileIn.Close();
            }
            else
            {
                // First of all, just set our config to default values.
                
                // NOTE: These are based on a 360 controller on Linux.
                
                // Start
                INTERNAL_joystickConfig.BUTTON_START.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_START.INPUT_ID = 7;
                INTERNAL_joystickConfig.BUTTON_START.INPUT_INVERT = false;
                
                // Back
                INTERNAL_joystickConfig.BUTTON_BACK.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_BACK.INPUT_ID = 6;
                INTERNAL_joystickConfig.BUTTON_BACK.INPUT_INVERT = false;
                
                // A
                INTERNAL_joystickConfig.BUTTON_A.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_A.INPUT_ID = 0;
                INTERNAL_joystickConfig.BUTTON_A.INPUT_INVERT = false;
                
                // B
                INTERNAL_joystickConfig.BUTTON_B.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_B.INPUT_ID = 1;
                INTERNAL_joystickConfig.BUTTON_B.INPUT_INVERT = false;
                
                // X
                INTERNAL_joystickConfig.BUTTON_X.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_X.INPUT_ID = 2;
                INTERNAL_joystickConfig.BUTTON_X.INPUT_INVERT = false;
                
                // Y
                INTERNAL_joystickConfig.BUTTON_Y.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_Y.INPUT_ID = 3;
                INTERNAL_joystickConfig.BUTTON_Y.INPUT_INVERT = false;
                
                // LB
                INTERNAL_joystickConfig.SHOULDER_LB.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.SHOULDER_LB.INPUT_ID = 4;
                INTERNAL_joystickConfig.SHOULDER_LB.INPUT_INVERT = false;
                
                // RB
                INTERNAL_joystickConfig.SHOULDER_RB.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.SHOULDER_RB.INPUT_ID = 5;
                INTERNAL_joystickConfig.SHOULDER_RB.INPUT_INVERT = false;
                
                // LT
                INTERNAL_joystickConfig.TRIGGER_LT.INPUT_TYPE = InputType.Axis;
                INTERNAL_joystickConfig.TRIGGER_LT.INPUT_ID = 2;
                INTERNAL_joystickConfig.TRIGGER_LT.INPUT_INVERT = false;
                
                // RT
                INTERNAL_joystickConfig.TRIGGER_RT.INPUT_TYPE = InputType.Axis;
                INTERNAL_joystickConfig.TRIGGER_RT.INPUT_ID = 5;
                INTERNAL_joystickConfig.TRIGGER_RT.INPUT_INVERT = false;
                
                // LStick
                INTERNAL_joystickConfig.BUTTON_LSTICK.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_LSTICK.INPUT_ID = 9;
                INTERNAL_joystickConfig.BUTTON_LSTICK.INPUT_INVERT = false;
                
                // RStick
                INTERNAL_joystickConfig.BUTTON_RSTICK.INPUT_TYPE = InputType.Button;
                INTERNAL_joystickConfig.BUTTON_RSTICK.INPUT_ID = 10;
                INTERNAL_joystickConfig.BUTTON_RSTICK.INPUT_INVERT = false;
                
                // DPad Up
                INTERNAL_joystickConfig.DPAD_UP.INPUT_TYPE = InputType.PovUp;
                INTERNAL_joystickConfig.DPAD_UP.INPUT_ID = 0;
                INTERNAL_joystickConfig.DPAD_UP.INPUT_INVERT = false;
                
                // DPad Down
                INTERNAL_joystickConfig.DPAD_DOWN.INPUT_TYPE = InputType.PovDown;
                INTERNAL_joystickConfig.DPAD_DOWN.INPUT_ID = 0;
                INTERNAL_joystickConfig.DPAD_DOWN.INPUT_INVERT = false;
                
                // DPad Left
                INTERNAL_joystickConfig.DPAD_LEFT.INPUT_TYPE = InputType.PovLeft;
                INTERNAL_joystickConfig.DPAD_LEFT.INPUT_ID = 0;
                INTERNAL_joystickConfig.DPAD_LEFT.INPUT_INVERT = false;
                
                // DPad Right
                INTERNAL_joystickConfig.DPAD_RIGHT.INPUT_TYPE = InputType.PovRight;
                INTERNAL_joystickConfig.DPAD_RIGHT.INPUT_ID = 0;
                INTERNAL_joystickConfig.DPAD_RIGHT.INPUT_INVERT = false;
                
                // LX
                INTERNAL_joystickConfig.AXIS_LX.INPUT_TYPE = InputType.Axis;
                INTERNAL_joystickConfig.AXIS_LX.INPUT_ID = 0;
                INTERNAL_joystickConfig.AXIS_LX.INPUT_INVERT = false;
                
                // LY
                INTERNAL_joystickConfig.AXIS_LY.INPUT_TYPE = InputType.Axis;
                INTERNAL_joystickConfig.AXIS_LY.INPUT_ID = 1;
                INTERNAL_joystickConfig.AXIS_LY.INPUT_INVERT = false;
                
                // RX
                INTERNAL_joystickConfig.AXIS_RX.INPUT_TYPE = InputType.Axis;
                INTERNAL_joystickConfig.AXIS_RX.INPUT_ID = 3;
                INTERNAL_joystickConfig.AXIS_RX.INPUT_INVERT = false;
                
                // RY
                INTERNAL_joystickConfig.AXIS_RY.INPUT_TYPE = InputType.Axis;
                INTERNAL_joystickConfig.AXIS_RY.INPUT_ID = 4;
                INTERNAL_joystickConfig.AXIS_RY.INPUT_INVERT = false;
                
                
                // Since it doesn't exist, we need to generate the default config.
                
                // ... but is our directory even there?
                string osConfigDir = osConfigFile.Substring(0, osConfigFile.IndexOf("MonoGameJoystick.cfg"));
                if (!String.IsNullOrEmpty(osConfigDir) && !Directory.Exists(osConfigDir))
                {
                    // Okay, jeez, we're really starting fresh.
                    Directory.CreateDirectory(osConfigDir);
                }
                
                // So, create the file.
                FileStream fileOut = File.Open(osConfigFile, FileMode.OpenOrCreate);
                XmlSerializer serializer = new XmlSerializer(typeof(MonoGameJoystickConfig));
                serializer.Serialize(fileOut, INTERNAL_joystickConfig);
                
                // We out.
                fileOut.Close();
            }

            // Limit to the first 4 sticks to avoid crashes.
            int numSticks = Math.Min(4, SDL.SDL_NumJoysticks());
            for (int x = 0; x < numSticks; x++)
            {
                INTERNAL_AddInstance(x);
            }
        }

        #endregion

        #region SDL-to-Value Helper Methods

        private static bool READTYPE_ReadBool(MonoGameJoystickValue input, IntPtr device, short deadZone)
        {
            if (input.INPUT_TYPE == InputType.Axis)
            {
                short axis = SDL.SDL_JoystickGetAxis(device, input.INPUT_ID);
                if (input.INPUT_INVERT)
                {
                    return (axis < -deadZone);
                }
                return (axis > deadZone);
            }
            else if (input.INPUT_TYPE == InputType.Button)
            {
                return ((SDL.SDL_JoystickGetButton(device, input.INPUT_ID) > 0) ^ input.INPUT_INVERT);
            }
            else if (   input.INPUT_TYPE == InputType.PovUp ||
                        input.INPUT_TYPE == InputType.PovDown ||
                        input.INPUT_TYPE == InputType.PovLeft ||
                        input.INPUT_TYPE == InputType.PovRight  )
            {
                return (((SDL.SDL_JoystickGetHat(device, input.INPUT_ID) & (byte) input.INPUT_TYPE) > 0) ^ input.INPUT_INVERT);
            }
            return false;
        }

        private static float READTYPE_ReadFloat(MonoGameJoystickValue input, IntPtr device)
        {
            float inputMask = input.INPUT_INVERT ? -1.0f : 1.0f;
            if (input.INPUT_TYPE == InputType.Axis)
            {
                // SDL2 clamps to 32767 on both sides.
                float maxRange = input.INPUT_INVERT ? -32767.0f : 32767.0f;
                return ((float) SDL.SDL_JoystickGetAxis(device, input.INPUT_ID)) / maxRange;
            }
            else if (input.INPUT_TYPE == InputType.Button)
            {
                return SDL.SDL_JoystickGetButton(device, input.INPUT_ID) * inputMask;
            }
            else if (   input.INPUT_TYPE == InputType.PovUp ||
                        input.INPUT_TYPE == InputType.PovDown ||
                        input.INPUT_TYPE == InputType.PovLeft ||
                        input.INPUT_TYPE == InputType.PovRight  )
            {
                return (SDL.SDL_JoystickGetHat(device, input.INPUT_ID) & (byte) input.INPUT_TYPE) * inputMask;
            }
            return 0.0f;
        }

        #endregion

        #region Value-To-Input Helper Methods
  
        // Button reader for ReadState
        private static Buttons READ_ReadButtons(IntPtr device, float deadZoneSize)
        {
            short DeadZone = (short) (deadZoneSize * short.MaxValue);
            Buttons b = (Buttons) 0;
   
            // A B X Y
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_A, device, DeadZone))
            {
                b |= Buttons.A;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_B, device, DeadZone))
            {
                b |= Buttons.B;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_X, device, DeadZone))
            {
                b |= Buttons.X;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_Y, device, DeadZone))
            {
                b |= Buttons.Y;
            }
   
            // Shoulder buttons
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.SHOULDER_LB, device, DeadZone))
            {
                b |= Buttons.LeftShoulder;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.SHOULDER_RB, device, DeadZone))
            {
                b |= Buttons.RightShoulder;
            }
   
            // Back/Start
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_BACK, device, DeadZone))
            {
                b |= Buttons.Back;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_START, device, DeadZone))
            {
                b |= Buttons.Start;
            }
   
            // Stick buttons
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_LSTICK, device, DeadZone))
            {
                b |= Buttons.LeftStick;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.BUTTON_RSTICK, device, DeadZone))
            {
                b |= Buttons.RightStick;
            }
   
            // DPad
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.DPAD_UP, device, DeadZone))
            {
                b |= Buttons.DPadUp;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.DPAD_DOWN, device, DeadZone))
            {
                b |= Buttons.DPadDown;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.DPAD_LEFT, device, DeadZone))
            {
                b |= Buttons.DPadLeft;
            }
            if (READTYPE_ReadBool(INTERNAL_joystickConfig.DPAD_RIGHT, device, DeadZone))
            {
                b |= Buttons.DPadRight;
            }

            return b;
        }

        // ReadState can convert stick values to button values
        private static Buttons READ_StickToButtons(Vector2 stick, Buttons left, Buttons right, Buttons up , Buttons down, float DeadZoneSize)
        {
            Buttons b = (Buttons) 0;

            if (stick.X > DeadZoneSize)
            {
                b |= right;
            }
            if (stick.X < -DeadZoneSize)
            {
                b |= left;
            }
            if (stick.Y > DeadZoneSize)
            {
                b |= up;
            }
            if (stick.Y < -DeadZoneSize)
            {
                b |= down;
            }
            
            return b;
        }

        // ReadState can convert trigger values to button values
        private static Buttons READ_TriggerToButton(float trigger, Buttons button, float DeadZoneSize)
        {
            Buttons b = (Buttons) 0;
            
            if (trigger > DeadZoneSize)
            {
                b |= button;
            }
            
            return b;
        }

        #endregion

        #region Internal Controller Read Method

        // This is where we actually read in the controller input!
        private static GamePadState ReadState(PlayerIndex index, GamePadDeadZone deadZone)
        {
            IntPtr device = INTERNAL_devices[(int) index];
            if (device == IntPtr.Zero)
            {
                return GamePadState.InitializedState;
            }
            
            // Do not attempt to understand this number at all costs!
            const float DeadZoneSize = 0.27f;
            
            // SDL_GameController
            if (INTERNAL_isGameController[(int) index])
            {
                // The "master" button state is built from this.
                Buttons gc_buttonState = (Buttons) 0;
                
                // Sticks
                GamePadThumbSticks gc_sticks = new GamePadThumbSticks(
                    new Vector2(
                        (float) SDL.SDL_GameControllerGetAxis(
                            device,
                            SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
                        ) / 32768.0f,
                        (float) SDL.SDL_GameControllerGetAxis(
                            device,
                            SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY
                        ) / -32768.0f
                    ),
                    new Vector2(
                        (float) SDL.SDL_GameControllerGetAxis(
                            device,
                            SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX
                        ) / 32768.0f,
                        (float) SDL.SDL_GameControllerGetAxis(
                            device,
                            SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY
                        ) / -32768.0f
                    )
                );
                gc_sticks.ApplyDeadZone(deadZone, DeadZoneSize);
                gc_buttonState |= READ_StickToButtons(
                    gc_sticks.Left,
                    Buttons.LeftThumbstickLeft,
                    Buttons.LeftThumbstickRight,
                    Buttons.LeftThumbstickUp,
                    Buttons.LeftThumbstickDown,
                    DeadZoneSize
                );
                gc_buttonState |= READ_StickToButtons(
                    gc_sticks.Right,
                    Buttons.RightThumbstickLeft,
                    Buttons.RightThumbstickRight,
                    Buttons.RightThumbstickUp,
                    Buttons.RightThumbstickDown,
                    DeadZoneSize
                );
                
                // Triggers
                GamePadTriggers gc_triggers = new GamePadTriggers(
                    (float) SDL.SDL_GameControllerGetAxis(device, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT) / 32768.0f,
                    (float) SDL.SDL_GameControllerGetAxis(device, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT) / 32768.0f
                );
                gc_buttonState |= READ_TriggerToButton(
                    gc_triggers.Left,
                    Buttons.LeftTrigger,
                    DeadZoneSize
                );
                gc_buttonState |= READ_TriggerToButton(
                    gc_triggers.Right,
                    Buttons.RightTrigger,
                    DeadZoneSize
                );
                
                // Buttons
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) != 0)
                {
                    gc_buttonState |= Buttons.A;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) != 0)
                {
                    gc_buttonState |= Buttons.B;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) != 0)
                {
                    gc_buttonState |= Buttons.X;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) != 0)
                {
                    gc_buttonState |= Buttons.Y;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) != 0)
                {
                    gc_buttonState |= Buttons.Back;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE) != 0)
                {
                    gc_buttonState |= Buttons.BigButton;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) != 0)
                {
                    gc_buttonState |= Buttons.Start;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) != 0)
                {
                    gc_buttonState |= Buttons.LeftStick;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) != 0)
                {
                    gc_buttonState |= Buttons.RightStick;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) != 0)
                {
                    gc_buttonState |= Buttons.LeftShoulder;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) != 0)
                {
                    gc_buttonState |= Buttons.RightShoulder;
                }
                
                // DPad
                GamePadDPad gc_dpad;
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) != 0)
                {
                    gc_buttonState |= Buttons.DPadUp;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) != 0)
                {
                    gc_buttonState |= Buttons.DPadDown;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) != 0)
                {
                    gc_buttonState |= Buttons.DPadLeft;
                }
                if (SDL.SDL_GameControllerGetButton(device, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) != 0)
                {
                    gc_buttonState |= Buttons.DPadRight;
                }
                gc_dpad = new GamePadDPad(gc_buttonState);
                
                // Compile the master buttonstate
                GamePadButtons gc_buttons = new GamePadButtons(gc_buttonState);
                
                return new GamePadState(
                    gc_sticks,
                    gc_triggers,
                    gc_buttons,
                    gc_dpad
                );
            }
            
            // SDL_Joystick
            
            // We will interpret the joystick values into this.
            Buttons buttonState = (Buttons) 0;
            
            // Sticks
            GamePadThumbSticks sticks = new GamePadThumbSticks(
                new Vector2(
                    READTYPE_ReadFloat(INTERNAL_joystickConfig.AXIS_LX, device),
                    -READTYPE_ReadFloat(INTERNAL_joystickConfig.AXIS_LY, device)
                ),
                new Vector2(
                    READTYPE_ReadFloat(INTERNAL_joystickConfig.AXIS_RX, device),
                    -READTYPE_ReadFloat(INTERNAL_joystickConfig.AXIS_RY, device)
                )
            );
            sticks.ApplyDeadZone(deadZone, DeadZoneSize);
            buttonState |= READ_StickToButtons(
                sticks.Left,
                Buttons.LeftThumbstickLeft,
                Buttons.LeftThumbstickRight,
                Buttons.LeftThumbstickUp,
                Buttons.LeftThumbstickDown,
                DeadZoneSize
            );
            buttonState |= READ_StickToButtons(
                sticks.Right,
                Buttons.RightThumbstickLeft,
                Buttons.RightThumbstickRight,
                Buttons.RightThumbstickUp,
                Buttons.RightThumbstickDown,
                DeadZoneSize
            );
            
            // Buttons
            buttonState = READ_ReadButtons(device, DeadZoneSize);
            
            // Triggers
            GamePadTriggers triggers = new GamePadTriggers(
                READTYPE_ReadFloat(INTERNAL_joystickConfig.TRIGGER_LT, device),
                READTYPE_ReadFloat(INTERNAL_joystickConfig.TRIGGER_RT, device)
            );
            buttonState |= READ_TriggerToButton(
                triggers.Left,
                Buttons.LeftTrigger,
                DeadZoneSize
            );
            buttonState |= READ_TriggerToButton(
                triggers.Right,
                Buttons.RightTrigger,
                DeadZoneSize
            );
            
            // Compile the GamePadButtons with our Buttons state
            GamePadButtons buttons = new GamePadButtons(buttonState);
            
            // DPad
            GamePadDPad dpad = new GamePadDPad(buttons.buttons);
   
            // Return the compiled GamePadState.
            return new GamePadState(sticks, triggers, buttons, dpad);
        }

        #endregion

        #region Public GamePad API

        //
        // Summary:
        //     Retrieves the capabilities of an Xbox 360 Controller.
        //
        // Parameters:
        //   playerIndex:
        //     Index of the controller to query.
        public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            // SDL_GameController Capabilities
            
            if (INTERNAL_isGameController[(int) playerIndex])
            {
                // An SDL_GameController will _always_ be feature-complete.
                return new GamePadCapabilities()
                {
                    IsConnected = INTERNAL_devices[(int) playerIndex] != IntPtr.Zero,
                    HasAButton = true,
                    HasBButton = true,
                    HasXButton = true,
                    HasYButton = true,
                    HasBackButton = true,
                    HasStartButton = true,
                    HasDPadDownButton = true,
                    HasDPadLeftButton = true,
                    HasDPadRightButton = true,
                    HasDPadUpButton = true,
                    HasLeftShoulderButton = true,
                    HasRightShoulderButton = true,
                    HasLeftStickButton = true,
                    HasRightStickButton = true,
                    HasLeftTrigger = true,
                    HasRightTrigger = true,
                    HasLeftXThumbStick = true,
                    HasLeftYThumbStick = true,
                    HasRightXThumbStick = true,
                    HasRightYThumbStick = true,
                    HasBigButton = true,
                    HasLeftVibrationMotor = INTERNAL_HapticSupported(playerIndex),
                    HasRightVibrationMotor = INTERNAL_HapticSupported(playerIndex),
                    HasVoiceSupport = false
                };
            }
            
            // SDL_Joystick Capabilities
            
            IntPtr d = INTERNAL_devices[(int) playerIndex];

            if (d == IntPtr.Zero)
            {
                return new GamePadCapabilities();
            }

            return new GamePadCapabilities()
            {
                IsConnected = true,
                
                HasAButton =                INTERNAL_joystickConfig.BUTTON_A.INPUT_TYPE         != InputType.None,
                HasBButton =                INTERNAL_joystickConfig.BUTTON_B.INPUT_TYPE         != InputType.None,
                HasXButton =                INTERNAL_joystickConfig.BUTTON_X.INPUT_TYPE         != InputType.None,
                HasYButton =                INTERNAL_joystickConfig.BUTTON_Y.INPUT_TYPE         != InputType.None,
                HasBackButton =             INTERNAL_joystickConfig.BUTTON_BACK.INPUT_TYPE      != InputType.None,
                HasStartButton =            INTERNAL_joystickConfig.BUTTON_START.INPUT_TYPE     != InputType.None,
                HasDPadDownButton =         INTERNAL_joystickConfig.DPAD_DOWN.INPUT_TYPE        != InputType.None,
                HasDPadLeftButton =         INTERNAL_joystickConfig.DPAD_LEFT.INPUT_TYPE        != InputType.None,
                HasDPadRightButton =        INTERNAL_joystickConfig.DPAD_RIGHT.INPUT_TYPE       != InputType.None,
                HasDPadUpButton =           INTERNAL_joystickConfig.DPAD_UP.INPUT_TYPE          != InputType.None,
                HasLeftShoulderButton =     INTERNAL_joystickConfig.SHOULDER_LB.INPUT_TYPE      != InputType.None,
                HasRightShoulderButton =    INTERNAL_joystickConfig.SHOULDER_RB.INPUT_TYPE      != InputType.None,
                HasLeftStickButton =        INTERNAL_joystickConfig.BUTTON_LSTICK.INPUT_TYPE    != InputType.None,
                HasRightStickButton =       INTERNAL_joystickConfig.BUTTON_RSTICK.INPUT_TYPE    != InputType.None,
                HasLeftTrigger =            INTERNAL_joystickConfig.TRIGGER_LT.INPUT_TYPE       != InputType.None,
                HasRightTrigger =           INTERNAL_joystickConfig.TRIGGER_RT.INPUT_TYPE       != InputType.None,
                HasLeftXThumbStick =        INTERNAL_joystickConfig.AXIS_LX.INPUT_TYPE          != InputType.None,
                HasLeftYThumbStick =        INTERNAL_joystickConfig.AXIS_LY.INPUT_TYPE          != InputType.None,
                HasRightXThumbStick =       INTERNAL_joystickConfig.AXIS_RX.INPUT_TYPE          != InputType.None,
                HasRightYThumbStick =       INTERNAL_joystickConfig.AXIS_RY.INPUT_TYPE          != InputType.None,

                HasLeftVibrationMotor = INTERNAL_HapticSupported(playerIndex),
                HasRightVibrationMotor = INTERNAL_HapticSupported(playerIndex),
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
            if (!INTERNAL_wasInit)
            {
                INTERNAL_AutoConfig();
                INTERNAL_wasInit = true;
            }
            if (SDL.SDL_WasInit(SDL.SDL_INIT_JOYSTICK) == 1)
            {
                SDL.SDL_JoystickUpdate();
            }
            if (SDL.SDL_WasInit(SDL.SDL_INIT_GAMECONTROLLER) == 1)
            {
                SDL.SDL_GameControllerUpdate();
            }
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
            if (!INTERNAL_HapticSupported(playerIndex))
            {
                return false;
            }
            
            if (leftMotor <= 0.0f && rightMotor <= 0.0f)
            {
                SDL.SDL_HapticStopAll(INTERNAL_haptics[(int)playerIndex]);
            }
            else if (SDL.SDL_HapticEffectSupported(INTERNAL_haptics[(int) playerIndex], ref INTERNAL_effect) == 1)
            {
                INTERNAL_effect.leftright.large_magnitude = (ushort) (65535.0f * leftMotor);
                INTERNAL_effect.leftright.small_magnitude = (ushort) (65535.0f * rightMotor);
                SDL.SDL_HapticUpdateEffect(
                    INTERNAL_haptics[(int) playerIndex],
                    0,
                    ref INTERNAL_effect
                );
                SDL.SDL_HapticRunEffect(
                    INTERNAL_haptics[(int) playerIndex],
                    0,
                    1
                );
            }
            else
            {
                float strength;
                if (leftMotor >= rightMotor)
                {
                    strength = leftMotor;
                }
                else
                {
                    strength = rightMotor;
                }
                SDL.SDL_HapticRumblePlay(
                    INTERNAL_haptics[(int) playerIndex],
                    strength,
                    uint.MaxValue // Oh dear...
                );
            }
            return true;
        }

        #endregion
    }
}
