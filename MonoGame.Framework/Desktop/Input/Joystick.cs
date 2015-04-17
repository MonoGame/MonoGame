// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    public class Joystick
    {
        private int id;
        public bool Open { get; private set; }
        public string Name { get; private set; }
        public PadConfig Config { get; private set; }
        public Capabilities Details { get; private set; }

        public Joystick(int id)
        {
            this.id = id;

            var cap = OpenTK.Input.Joystick.GetCapabilities(id);
            this.Open = true;

            this.Name = "";
            this.Details = new Capabilities(cap);
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

        public static List<Joystick> GrabJoysticks()
        {
            int num = 0;
            List<Joystick> list = new List<Joystick>();

            while (OpenTK.Input.Joystick.GetCapabilities(num).IsConnected)
            {
                list.Add(new Joystick(num));
                num++;
            }
            return list;
        }
    }
}
