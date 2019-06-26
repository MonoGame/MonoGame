// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
	internal class Int32Reader : ContentTypeReader<int>
    {
        public Int32Reader()
        {
        }

        protected internal override int Read(ContentReader input, int existingInstance)
        {
            return input.ReadInt32();
        }
    }
}
