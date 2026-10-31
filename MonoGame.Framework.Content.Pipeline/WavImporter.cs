// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline;

/// <summary>
/// Provides methods for reading .wav audio files for use in the Content Pipeline.
/// </summary>
[ContentImporter(".wav", DisplayName = "Wav Importer - MonoGame", DefaultProcessor = "SoundEffectProcessor")]
public class WavImporter : ContentImporter<AudioContent>
{
    /// <summary>
    /// Called by the XNA Framework when importing a .wav audio file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
    /// </summary>
    /// <param name="filename">Name of a game asset file.</param>
    /// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
    /// <returns>Resulting game asset.</returns>
    /// <remarks>This importer supports PCM in unsigned 8-bit, signed 16-bit, signed 24-bit, IEEE Float 32-bit, MS-ADPCM or IMA/ADPCM with sample rates from 8KHz up to 48KHz.</remarks>
    public override AudioContent Import(string filename, ContentImporterContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(filename);
        ArgumentNullException.ThrowIfNull(context);
        if (!File.Exists(filename))
            throw new FileNotFoundException(filename);

        // Validate the format of the input.
        var content = new AudioContent(filename, AudioFileType.Wav);
        if (content.Format.SampleRate is < 8000 or > 48000)
            throw new InvalidContentException($"Audio file {Path.GetFileName(filename)} contains audio data with unsupported sample rate of {content.Format.SampleRate}Hz. Supported sample rates are from 8000Hz up to 48000Hz.");
        var validPcm = content.Format is { Format: 1, BitsPerSample: 8 or 16 or 24 };
        var validAdpcm = content.Format.Format is 2 or 17 && content.Format.BitsPerSample == 4;
        var validIeeeFloat = content.Format is { Format: 3, BitsPerSample: 32 };
        if (!(validPcm || validAdpcm || validIeeeFloat))
            throw new InvalidContentException($"Audio file {Path.GetFileName(filename)} contains audio data with unsupported format of {content.Format.Format} and bit depth of {content.Format.BitsPerSample}. Supported bit depths are unsigned 8-bit, signed 16-bit, signed 24-bit for PCM(1) and 32-bit for IEEE Float(3).");

        return content;
    }
}
