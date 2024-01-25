// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
	internal class UInt64Reader : ContentTypeReader<ulong>
    {
        public UInt64Reader()
        {
        }

        protected internal override ulong Read(ContentReader input, ulong existingInstance)
        {
            return input.ReadUInt64();
        }
    }
}
