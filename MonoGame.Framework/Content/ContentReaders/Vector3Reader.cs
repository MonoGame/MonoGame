// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Content
{
    internal class Vector3Reader : ContentTypeReader<Vector3>
    {
        public Vector3Reader()
        {
        }

        protected internal override Vector3 Read(ContentReader input, Vector3 existingInstance)
        {
            return input.ReadVector3();
        }
    }
}
