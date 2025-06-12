// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Helper methods for audio importing, conversion and processing.
    /// </summary>
    class AudioHelper
    {
        // This array must remain in sync with the ConversionFormat enum.
        static string[] conversionFormatExtensions = new[] { "wav", "wav", "wma", "xma", "wav", "m4a", "ogg", "mp3" };

        /// <summary>
        /// Gets the file extension for an audio format.
        /// </summary>
        /// <param name="format">The conversion format</param>
        /// <returns>The file extension for the given conversion format.</returns>
        static public string GetExtension(ConversionFormat format)
        {
            return conversionFormatExtensions[(int)format];
        }
    }
}
