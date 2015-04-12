// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public class PadConfig
    {
        private string name;
        private int index;

        public PadConfig(string name, int index)
        {
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
