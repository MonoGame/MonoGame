// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Content
{

	internal class RectangleFReader : ContentTypeReader<RectangleF>
    {
        public RectangleFReader()
        {
        }

        protected internal override RectangleF Read(ContentReader input, RectangleF existingInstance)
        {
            var left = input.ReadSingle();
            var top = input.ReadSingle();
            var width = input.ReadSingle();
            var height = input.ReadSingle();
            return new RectangleF(left, top, width, height);
        }
    }
}
