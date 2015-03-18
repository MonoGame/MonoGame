// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public class Axis
    {
        public Axis()
        {
            this.Negative = new Input();
            this.Positive = new Input();            
        }

        public Input Negative { get; set; }
        public Input Positive { get; set; }
        public InputType Type { get; set; }

        public float ReadAxis(int device)
        {
            return (this.Positive.ReadFloat(device) - this.Negative.ReadFloat(device));
        }

        internal void AssignAxis(int id, bool negative)
        {
            this.Negative.ID = id;
            this.Negative.Negative = !negative;
            this.Negative.Type = InputType.Axis;
            this.Positive.ID = id;
            this.Positive.Negative = negative;
            this.Positive.Type = InputType.Axis;
        }
    }
}
