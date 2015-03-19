// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public class Stick
    {
        public Stick()
        {
            this.X = new Axis();
            this.Y = new Axis();
            this.Press = new Input();
        }

        public Input Press { get; set; }
        public Axis X { get; set; }
        public Axis Y { get; set; }

        internal Vector2 ReadAxisPair(int device)
        {
            return new Vector2(this.X.ReadAxis(device), -this.Y.ReadAxis(device));
        }

    }
}
