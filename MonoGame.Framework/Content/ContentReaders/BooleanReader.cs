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
	internal class BooleanReader : ContentTypeReader<bool>
    {
        public BooleanReader()
        {
        }

        protected internal override bool Read(ContentReader input, bool existingInstance)
        {
            return input.ReadBoolean();
        }
    }
}
