// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A custom song processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a Song type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Song - MonoGame")]
    public class SongProcessor : ContentProcessor<AudioContent, SongContent>
    {
        /// <summary>
        /// Gets or sets the target format quality of the audio content.
        /// </summary>
        /// <value>The ConversionQuality of this audio data.</value>
        public ConversionQuality Quality { get; set; } = ConversionQuality.Best;

        /// <summary>
        /// Builds the content for the source audio.
        /// </summary>
        /// <param name="input">The audio content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built audio.</returns>
        public override SongContent Process(AudioContent input, ContentProcessorContext context)
        {
            // The xnb name is the basis for the final song filename.
            var songFileName = context.OutputFilename;

            // Convert and write out the song media file.
            var profile = AudioProfile.ForPlatform(context.TargetPlatform);
            var finalQuality = profile.ConvertStreamingAudio(context.TargetPlatform, Quality, input, ref songFileName);

            // Let the pipeline know about the song file so it can clean things up.
            context.AddOutputFile(songFileName);
            if (Quality != finalQuality)
            {
                context.Logger.Log(LogLevel.Info, $"Failed to convert using \"{Quality}\" quality, used \"{finalQuality}\" quality");
            }

            // Return the XNB song content.
            return new SongContent
            {
                FileName = PathHelper.GetRelativePath(Path.GetDirectoryName(context.OutputFilename) + Path.DirectorySeparatorChar, songFileName),
                Duration = input.Duration
            };
        }
    }
}
