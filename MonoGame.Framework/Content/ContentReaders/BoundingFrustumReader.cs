// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content
{
    internal class BoundingFrustumReader : ContentTypeReader<BoundingFrustum>
    {
        public BoundingFrustumReader()
        {
        }

        protected internal override BoundingFrustum Read(ContentReader input, BoundingFrustum existingInstance)
        {
            return new BoundingFrustum(input.ReadMatrix());
        }
    }
}
