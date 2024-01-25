// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    internal class Vector4Reader : ContentTypeReader<Vector4>
    {
        public Vector4Reader()
        {
        }

        protected internal override Vector4 Read(ContentReader input, Vector4 existingInstance)
        {
            return input.ReadVector4();
        }
    }
}
