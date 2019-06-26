// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
	internal class SingleReader : ContentTypeReader<float>
    {
        public SingleReader()
        {
        }

        protected internal override float Read(ContentReader input, float existingInstance)
        {
            return input.ReadSingle();
        }
    }
}