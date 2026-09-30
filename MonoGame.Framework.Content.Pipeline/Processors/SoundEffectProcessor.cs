// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// A sound effect processor that processes an intermediate AudioContent type. This type encapsulates the source audio content, producing a SoundEffect type that can be used in the game.
    /// </summary>
    [ContentProcessor(DisplayName = "Sound Effect - MonoGame")]
    public class SoundEffectProcessor : ContentProcessor<AudioContent, SoundEffectContent>
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
        public override SoundEffectContent Process(AudioContent input, ContentProcessorContext context)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(context);

            var profile = AudioProfile.ForPlatform(context.TargetPlatform);
            var finalQuality = profile.ConvertAudio(context.TargetPlatform, Quality, input);
            if (Quality != finalQuality)
                context.Logger.Log(LogLevel.Info, $"Failed to convert using \"{Quality}\" quality, used \"{finalQuality}\" quality");

            return new SoundEffectContent
            {
                Format = [.. input.Format.NativeWaveFormat],
                Data = [.. input.Data],
                LoopStart = input.LoopStart,
                LoopLength = input.LoopLength,
                Duration = (int)input.Duration.TotalMilliseconds
            };
        }
    }
}
