﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Target formats supported for audio source conversions.
    /// </summary>
    public enum ConversionFormat
    {
        /// <summary>
        /// A PCM encoding technique using 4 bits
        /// </summary>
        Adpcm,

        /// <summary>
        /// 8/16-bit mono/stereo PCM audio 8KHz-48KHz
        /// </summary>
        Pcm,

        /// <summary>
        /// Windows Media CBR formats (64 kbps, 128 kbps, 192 kbps)
        /// </summary>
        WindowsMedia,

        /// <summary>
        /// The Xbox compression format
        /// </summary>
        Xma,
    }
}
