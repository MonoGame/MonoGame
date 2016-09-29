// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Compression quality of the audio content.
    /// </summary>
    public enum ConversionQuality
    {
        /// <summary>
        /// High compression yielding lower file size, but could compromise audio quality
        /// </summary>
        Low,

        /// <summary>
        /// Moderate compression resulting in a compromise between audio quality and file size
        /// </summary>
        Medium,

        /// <summary>
        /// Lowest compression, but the best audio quality
        /// </summary>
        Best,
    }
}
