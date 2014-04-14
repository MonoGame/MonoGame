// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Represents a processed sound effect.
    /// </summary>
    public sealed class SoundEffectContent
    {
        internal List<byte> format;
        internal List<byte> data;
        internal int loopStart;
        internal int loopLength;
        internal int duration;

        /// <summary>
        /// Initializes a new instance of the SoundEffectContent class.
        /// </summary>
        /// <param name="format">The WAV header.</param>
        /// <param name="data">The audio waveform data.</param>
        /// <param name="loopStart">The start of the loop segment (must be block aligned).</param>
        /// <param name="loopLength">The length of the loop segment (must be block aligned).</param>
        /// <param name="duration">The duration of the wave file in milliseconds.</param>
        internal SoundEffectContent(ReadOnlyCollection<byte> format, ReadOnlyCollection<byte> data, int loopStart, int loopLength, int duration)
        {
            this.format = new List<byte>(format);
            this.data = new List<byte>(data);
            this.loopStart = loopStart;
            this.loopLength = loopLength;
            this.duration = duration;
        }
    }
}
