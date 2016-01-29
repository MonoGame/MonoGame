// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public enum InputType { PovUp = 1, Button = 16, Axis = 32, PovDown = 4, PovLeft = 8, PovRight = 2, None = -1 };

    public class Input
    {
        public int ID;
        public InputType Type;
        public bool Negative { get; set; }

        private static OpenTK.Input.JoystickAxis GetAxis(int id)
        {
            switch (id)
            {
                case 0:
                    return OpenTK.Input.JoystickAxis.Axis0;
                case 1:
                    return OpenTK.Input.JoystickAxis.Axis1;
                case 2:
                    return OpenTK.Input.JoystickAxis.Axis2;
                case 3:
                    return OpenTK.Input.JoystickAxis.Axis3;
                case 4:
                    return OpenTK.Input.JoystickAxis.Axis4;
                case 5:
                    return OpenTK.Input.JoystickAxis.Axis5;
                case 6:
                    return OpenTK.Input.JoystickAxis.Axis6;
                case 7:
                    return OpenTK.Input.JoystickAxis.Axis7;
                case 8:
                    return OpenTK.Input.JoystickAxis.Axis8;
                case 9:
                    return OpenTK.Input.JoystickAxis.Axis9;
                case 10:
                    return OpenTK.Input.JoystickAxis.Axis10;
                default:
                    return OpenTK.Input.JoystickAxis.Last;
            }
        }

        private static OpenTK.Input.JoystickButton GetButton(int id)
        {
            switch (id)
            {
                case 0:
                    return OpenTK.Input.JoystickButton.Button0;
                case 1:
                    return OpenTK.Input.JoystickButton.Button1;
                case 2:
                    return OpenTK.Input.JoystickButton.Button2;
                case 3:
                    return OpenTK.Input.JoystickButton.Button3;
                case 4:
                    return OpenTK.Input.JoystickButton.Button4;
                case 5:
                    return OpenTK.Input.JoystickButton.Button5;
                case 6:
                    return OpenTK.Input.JoystickButton.Button6;
                case 7:
                    return OpenTK.Input.JoystickButton.Button7;
                case 8:
                    return OpenTK.Input.JoystickButton.Button8;
                case 9:
                    return OpenTK.Input.JoystickButton.Button9;
                case 10:
                    return OpenTK.Input.JoystickButton.Button10;
                case 11:
                    return OpenTK.Input.JoystickButton.Button11;
                case 12:
                    return OpenTK.Input.JoystickButton.Button12;
                case 13:
                    return OpenTK.Input.JoystickButton.Button13;
                case 14:
                    return OpenTK.Input.JoystickButton.Button14;
                case 15:
                    return OpenTK.Input.JoystickButton.Button15;
                case 16:
                    return OpenTK.Input.JoystickButton.Button16;
                case 17:
                    return OpenTK.Input.JoystickButton.Button17;
                case 18:
                    return OpenTK.Input.JoystickButton.Button18;
                case 19:
                    return OpenTK.Input.JoystickButton.Button19;
                case 20:
                    return OpenTK.Input.JoystickButton.Button20;
                case 21:
                    return OpenTK.Input.JoystickButton.Button21;
                case 22:
                    return OpenTK.Input.JoystickButton.Button22;
                case 23:
                    return OpenTK.Input.JoystickButton.Button23;
                case 24:
                    return OpenTK.Input.JoystickButton.Button24;
                case 25:
                    return OpenTK.Input.JoystickButton.Button25;
                case 26:
                    return OpenTK.Input.JoystickButton.Button26;
                case 27:
                    return OpenTK.Input.JoystickButton.Button27;
                case 28:
                    return OpenTK.Input.JoystickButton.Button28;
                case 29:
                    return OpenTK.Input.JoystickButton.Button29;
                case 30:
                    return OpenTK.Input.JoystickButton.Button30;
                case 31:
                    return OpenTK.Input.JoystickButton.Button31;
                default:
                    return OpenTK.Input.JoystickButton.Last;
            }
        }

        private static OpenTK.Input.JoystickHat GetHat(int id)
        {
            switch (id)
            {
                case 0:
                    return OpenTK.Input.JoystickHat.Hat0;
                case 1:
                    return OpenTK.Input.JoystickHat.Hat1;
                case 2:
                    return OpenTK.Input.JoystickHat.Hat2;
                case 3:
                    return OpenTK.Input.JoystickHat.Hat3;
                default:
                    return OpenTK.Input.JoystickHat.Last;
            }
        }

        internal bool ReadBool(int device, short DeadZone)
        {
            switch (Type)
            {
                case InputType.Axis:
                    var axis = OpenTK.Input.Joystick.GetState(device).GetAxis(GetAxis(this.ID));
                    if (this.Negative)
                    {
                        return (axis < -DeadZone);
                    }
                    return (axis > DeadZone);
                case InputType.Button:
                    return ((OpenTK.Input.Joystick.GetState(device).GetButton(GetButton(this.ID)) == OpenTK.Input.ButtonState.Pressed) ^ this.Negative);
                case InputType.PovUp:
                    return OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsUp ^ this.Negative;
                case InputType.PovDown:
                    return OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsDown ^ this.Negative;
                case InputType.PovLeft:
                    return OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsLeft ^ this.Negative;
                case InputType.PovRight:
                    return OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsRight ^ this.Negative;
            }

            return false;
        }

        internal float ReadFloat(int device)
        {
            float mask = this.Negative ? -1f : 1f;
            switch (this.Type)
            {
                case InputType.Axis:
                    float range = this.Negative ? ((float)(-32768)) : ((float)0x7fff);
                    return OpenTK.Input.Joystick.GetState(device).GetAxis(GetAxis(this.ID)) / range;
                case InputType.Button:
                    return (float)Convert.ToInt32(OpenTK.Input.Joystick.GetState(device).GetButton(GetButton(this.ID)) == OpenTK.Input.ButtonState.Pressed) * mask;
                case InputType.PovUp:
                    return (float)Convert.ToInt32(OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsUp) * mask;
                case InputType.PovDown:
                    return (float)Convert.ToInt32(OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsDown) * mask;
                case InputType.PovLeft:
                    return (float)Convert.ToInt32(OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsLeft) * mask;
                case InputType.PovRight:
                    return (float)Convert.ToInt32(OpenTK.Input.Joystick.GetState(device).GetHat(GetHat(this.ID)).IsRight) * mask;
            }

            return 0f;
        }
    }
}
