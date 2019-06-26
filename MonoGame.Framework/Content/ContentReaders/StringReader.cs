// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Content
{
    internal class StringReader : ContentTypeReader<String>
    {
        public StringReader()
        {
        }

        protected internal override string Read(ContentReader input, string existingInstance)
        {
            return input.ReadString();
        }
    }
}
