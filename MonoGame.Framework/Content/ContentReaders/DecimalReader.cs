// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    internal class DecimalReader : ContentTypeReader<decimal>
    {
        public DecimalReader()
        {
        }

        protected internal override decimal Read(ContentReader input, decimal existingInstance)
        {
            return input.ReadDecimal();
        }
    }
}
