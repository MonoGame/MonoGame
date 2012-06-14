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

namespace Microsoft.Xna.Framework.Input
{
    public enum InputType { PovUp = 1, Button = 16, Axis = 32, PovDown = 4, PovLeft = 8, PovRight = 2, None = -1 };

    public class Input
    {
        public int ID;
        public InputType Type;
        public bool Negative { get; set; }


        internal bool ReadBool(IntPtr device, short DeadZone)
        {
            switch (Type)
            {
                case InputType.Axis:
                    var axis = Tao.Sdl.Sdl.SDL_JoystickGetAxis(device, this.ID);
                    if (this.Negative)
                    {
                        return (axis < -DeadZone);
                    }
                    return (axis > DeadZone);
                case InputType.Button:
                    return ((Tao.Sdl.Sdl.SDL_JoystickGetButton(device, this.ID) > 0) ^ this.Negative);
                case InputType.PovUp:
                case InputType.PovDown:
                case InputType.PovLeft:
                case InputType.PovRight:
                    // Cast the type as an int to get the correct sdl mask for the hat
                    return (((Tao.Sdl.Sdl.SDL_JoystickGetHat(device, this.ID) & (int)Type) > 0) ^ this.Negative);
                case InputType.None:
                default:
                    return false;
            }
        }

        internal float ReadFloat(IntPtr device)
        {
            float mask = this.Negative ? -1f : 1f;
            switch (this.Type)
            {
                case InputType.Axis:
                    float range = this.Negative ? ((float)(-32768)) : ((float)0x7fff);
                    return (((float)Tao.Sdl.Sdl.SDL_JoystickGetAxis(device, this.ID)) / range);
                case InputType.Button:
                    return (Tao.Sdl.Sdl.SDL_JoystickGetButton(device, this.ID) * mask);
                case InputType.PovUp:
                case InputType.PovDown:
                case InputType.PovLeft:
                case InputType.PovRight:
                    return ((Tao.Sdl.Sdl.SDL_JoystickGetHat(device, this.ID) & (int)Type) * mask);
            }
            return 0f;

        }
    }
}
