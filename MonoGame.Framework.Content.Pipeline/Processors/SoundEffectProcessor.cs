﻿// MonoGame - Copyright (C) The MonoGame Team
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
        ConversionQuality quality = ConversionQuality.Best;

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
                        targetFormat = ConversionFormat.Adpcm;
                    break;
            }

            input.ConvertFormat(targetFormat, quality, null);

            return new SoundEffectContent(input.Format.NativeWaveFormat, input.Data, input.LoopStart, input.LoopLength, (int)input.Duration.TotalMilliseconds);
        }
    }
}
