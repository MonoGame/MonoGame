// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using System.IO;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A sound effect processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a SoundEffect type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Sound Effect - MonoGame")]
    public class SoundEffectProcessor : ContentProcessor<AudioContent, SoundEffectContent>
    {
        int loopLength;
        int loopStart;
        ConversionQuality quality = ConversionQuality.Best;

        /// <summary>
        /// Gets or sets the loop length, in samples.
        /// </summary>
        /// <value>The number of samples in the loop.</value>
        public int LoopLength { get { return loopLength; } set { loopLength = value; } }

        /// <summary>
        /// Gets or sets the loop start, in samples.
        /// </summary>
        /// <value>The number of samples to the start of the loop.</value>
        public int LoopStart { get { return loopStart; } set { loopStart = value; } }

        /// <summary>
        /// Gets or sets the target format quality of the audio content.
        /// </summary>
        /// <value>The ConversionQuality of this audio data.</value>
        public ConversionQuality Quality { get { return quality; } set { quality = value; } }

        /// <summary>
        /// Initializes a new instance of SoundEffectProcessor.
        /// </summary>
        public SoundEffectProcessor()
        {
        }

        /// <summary>
        /// Builds the content for the source audio.
        /// </summary>
        /// <param name="input">The audio content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built audio.</returns>
        public override SoundEffectContent Process(AudioContent input, ContentProcessorContext context)
        {
            var targetFormat = ConversionFormat.Pcm;

            switch (quality)
            {
                case ConversionQuality.Medium:
                case ConversionQuality.Low:
                    if ((context.TargetPlatform == TargetPlatform.iOS) || (context.TargetPlatform == TargetPlatform.MacOSX))
                        targetFormat = ConversionFormat.ImaAdpcm;
                    else
                    {
                        // TODO: For some reason this doesn't work on Windows
                        // so we fallback to plain PCM and depend on the 
                        // bitrate reduction only.
                        //targetFormat = ConversionFormat.Adpcm;
                    }
                    break;
            }

            input.ConvertFormat(targetFormat, quality, null);

            return new SoundEffectContent(input.Format.NativeWaveFormat, input.Data, loopStart > 0 ? loopStart : input.LoopStart, loopLength > 0 ? loopLength : input.LoopLength, (int)input.Duration.TotalMilliseconds);
        }
    }
}
