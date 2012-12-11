// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A custom song processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a Song type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Song Processor - MonoGame")]
    public class SongProcessor : ContentProcessor<AudioContent, SongContent>
    {
        ConversionQuality quality;

        /// <summary>
        /// Gets or sets the target format quality of the audio content.
        /// </summary>
        /// <value>The ConversionQuality of this audio data.</value>
        public ConversionQuality Quality { get { return quality; } set { quality = value; } }

        /// <summary>
        /// Initializes a new instance of SongProcessor.
        /// </summary>
        public SongProcessor()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the content for the source audio.
        /// </summary>
        /// <param name="input">The audio content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built audio.</returns>
        public override SongContent Process(AudioContent input, ContentProcessorContext context)
        {
            throw new NotImplementedException();
        }
    }
}
