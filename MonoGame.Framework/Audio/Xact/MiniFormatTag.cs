// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Audio
{
    enum MiniFormatTag 
    {
        Pcm = 0x0,
        Xma = 0x1,
        Adpcm = 0x2,
        Wma = 0x3,

        // We allow XMA to be reused for a platform specific format.
        PlatformSpecific = Xma,
    }
}