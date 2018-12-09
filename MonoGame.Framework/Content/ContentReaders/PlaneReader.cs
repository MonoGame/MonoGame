// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    internal class PlaneReader : ContentTypeReader<Plane>
    {
        public PlaneReader()
        {
        }

        protected internal override Plane Read(ContentReader input, Plane existingInstance)
        {
            existingInstance.Normal = input.ReadVector3();
            existingInstance.D = input.ReadSingle();
            return existingInstance;
        }
    }
}
