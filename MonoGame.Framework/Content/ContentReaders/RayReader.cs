// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content
{
    internal class RayReader : ContentTypeReader<Ray>
    {
        public RayReader()
        {
        }

        protected internal override Ray Read(ContentReader input, Ray existingInstance)
        {
            Vector3 position = input.ReadVector3();
            Vector3 direction = input.ReadVector3();
            return new Ray(position, direction);
        }
    }
}
