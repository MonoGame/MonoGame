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
    /// A custom song processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a Song type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Song - MonoGame")]
    public class SongProcessor : ContentProcessor<AudioContent, SongContent>
    {
        ConversionQuality quality = ConversionQuality.Best;

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
        }

        /// <summary>
        /// Builds the content for the source audio.
        /// </summary>
        /// <param name="input">The audio content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built audio.</returns>
        public override SongContent Process(AudioContent input, ContentProcessorContext context)
        {
            // Most platforms will use AAC ("mp4") by default
            var targetFormat = ConversionFormat.Aac;

            switch (context.TargetPlatform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsPhone8:
                case TargetPlatform.WindowsStoreApp:
                    targetFormat = ConversionFormat.WindowsMedia;
                    break;

                case TargetPlatform.DesktopGL:
                    targetFormat = ConversionFormat.Vorbis;
                    break;
            }

            // Get the song output path with the target format extension.
            var songFileName = Path.ChangeExtension(context.OutputFilename, AudioHelper.GetExtension(targetFormat));

            // Make sure the output folder for the song exists.
            Directory.CreateDirectory(Path.GetDirectoryName(songFileName));

            // Convert and write out the song media file.
            input.ConvertFormat(targetFormat, quality, songFileName);

            // Let the pipeline know about the song file so it can clean things up.
            context.AddOutputFile(songFileName);

            // Return the XNB song content.
            return new SongContent(PathHelper.GetRelativePath(Path.GetDirectoryName(context.OutputFilename) + Path.DirectorySeparatorChar, songFileName), input.Duration);
        }
    }
}
