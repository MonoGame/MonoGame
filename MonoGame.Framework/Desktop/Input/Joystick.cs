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
using Tao.Sdl;

namespace Microsoft.Xna.Framework.Input
{
    public class Joystick
    {
        private int id;
        private IntPtr device;
        public bool Open { get; private set; }
        public string Name { get; private set; }
        public PadConfig Config { get; private set; }
        public Capabilities Details { get; private set; }

        public Joystick(int id)
        {
            // TODO: Complete member initialization
            this.id = id;
            this.device = Tao.Sdl.Sdl.SDL_JoystickOpen(id);
            this.Open = true;
            this.Name = Tao.Sdl.Sdl.SDL_JoystickName(id);
            this.Details = new Capabilities(this.device);
            this.Config = new PadConfig(this.Name, id);
            this.SetDefaults(this.Details);
        }

        private void SetDefaults(Capabilities capabilities)
        {
            if (capabilities != null)
            {
                if (capabilities.NumberOfAxis > 1)
                {
                    this.Config.LeftStick.X.AssignAxis(0, false);
                    this.Config.LeftStick.Y.AssignAxis(1, false);
                }
                if (capabilities.NumberOfPovHats > 0)
                {
                    this.Config.Dpad.AssignPovHat(0);
                }
                if (capabilities.NumberOfButtons > 0)
                {
                    for (int i = 0; i < capabilities.NumberOfButtons; i++)
                    {
                        Input input = this.Config[i];
                        if (input != null)
                        {
                            input.ID = i;
                            input.Negative = false;
                            input.Type = InputType.Button;
                        }
                    }
                }
            }

        }

        public int ID { get { return id; } }

        public static bool Init()
        {
            // only initialise the Joystick part of SDL
            return Sdl.SDL_Init(Sdl.SDL_INIT_JOYSTICK) == 0;
        }

        public static List<Joystick> GrabJoysticks()
        {
            int num = Tao.Sdl.Sdl.SDL_NumJoysticks();
            List<Joystick> list = new List<Joystick>();
            Tao.Sdl.Sdl.SDL_JoystickEventState(0);
            for (int i = 0; i < num; i++)
            {
                list.Add(new Joystick(i));
            }
            return list;

        }



        internal static void Cleanup()
        {
            throw new NotImplementedException();
        }
    }

}
