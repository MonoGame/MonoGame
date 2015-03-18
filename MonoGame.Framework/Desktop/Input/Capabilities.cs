// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public class Capabilities
    {
        public Capabilities(OpenTK.Input.JoystickCapabilities cap)
        {      
            this.NumberOfAxis = cap.AxisCount;
            this.NumberOfButtons = cap.ButtonCount;
            this.NumberOfPovHats = cap.HatCount;
        }

        public int NumberOfAxis { get; private set; }
        public int NumberOfButtons { get; private set; }
        public int NumberOfPovHats { get; private set; }

    }
}
