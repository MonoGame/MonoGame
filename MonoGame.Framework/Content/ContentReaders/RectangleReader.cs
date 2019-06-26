// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
	
	internal class RectangleReader : ContentTypeReader<Rectangle>
    {
        public RectangleReader()
        {
        }

        protected internal override Rectangle Read(ContentReader input, Rectangle existingInstance)
        {
            int left = input.ReadInt32();
            int top = input.ReadInt32();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            return new Rectangle(left, top, width, height);
        }
    }
}
