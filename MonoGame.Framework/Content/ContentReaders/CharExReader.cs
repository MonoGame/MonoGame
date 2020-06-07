// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content
{
    internal class CharExReader : ContentTypeReader<CharEx>
{
    internal CharExReader()
    {
    }

    protected internal override CharEx Read(ContentReader input, CharEx existingInstance)
    {
        return input.ReadCharEx();
    }
}
}
