// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
	internal class DoubleReader : ContentTypeReader<double>
    {
        public DoubleReader()
        {
        }

        protected internal override double Read(ContentReader input, double existingInstance)
        {
            return input.ReadDouble();
        }
    }
}

