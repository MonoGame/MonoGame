// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework;

partial class TitleContainer
{
    static partial void PlatformInit()
    {
        Location = AppContext.BaseDirectory;
    }

    private static Stream PlatformOpenStream(string safeName)
    {
        throw new NotImplementedException();
    }

    private static Stream PlatformOpenWriteStream(string safeName)
    {
        throw new NotImplementedException();
    }
}
