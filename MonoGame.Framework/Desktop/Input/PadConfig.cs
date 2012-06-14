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
    public class PadConfig
    {
        private string name;
        private int index;

        public PadConfig(string name, int index)
        {
            // TODO: Complete member initialization
            this.name = name;
            this.index = index;
            this.LeftStick = new Stick();
            this.RightStick = new Stick();
            this.Dpad = new DPad();
            this.Button_A = new Input();
            this.Button_B = new Input();
            this.Button_X = new Input();
            this.Button_Y = new Input();
            this.Button_LB = new Input();
            this.Button_RB = new Input();
            this.Button_Start = new Input();
            this.Button_Back = new Input();
            this.LeftTrigger = new Input();
            this.RightTrigger = new Input();

        }

        public int Index { get { return index; } }
        public string JoystickName { get { return name; } }

        public Input Button_A { get; set; }
        public Input Button_B { get; set; }
        public Input Button_X { get; set; }
        public Input Button_Y { get; set; }
        public Input Button_Back { get; set; }
        public Input Button_Start { get; set; }
        public Input Button_LB { get; set; }
        public Input Button_RB { get; set; }
        public Stick LeftStick { get; set; }
        public Stick RightStick { get; set; }
        public DPad Dpad { get; set; }
        public Input LeftTrigger { get; set; }
        public Input RightTrigger { get; set; }

        public Input this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.Button_A;

                    case 1:
                        return this.Button_B;

                    case 2:
                        return this.Button_X;

                    case 3:
                        return this.Button_Y;

                    case 4:
                        return this.Button_LB;

                    case 5:
                        return this.Button_RB;

                    case 6:
                        return this.Button_Back;

                    case 7:
                        return this.Button_Start;
                }
                return null;

            }
        }
    }

}
