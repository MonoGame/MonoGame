// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio;

internal sealed class WebGL2AudioProfile : AudioProfile
{
    public override bool Supports(TargetPlatform platform) => platform == TargetPlatform.WebGL2;

    public override ConversionQuality ConvertAudio(TargetPlatform platform, ConversionQuality quality, AudioContent content)
    {
        return DefaultAudioProfile.ConvertToFormat(content, ConversionFormat.Pcm, quality, null);
    }

    public override ConversionQuality ConvertStreamingAudio(TargetPlatform platform, ConversionQuality quality, AudioContent content, ref string outputFileName)
    {
        // For broadest compatibility on web, convert audio to mp3
        // macOS/iOS + safari supports ogg/vorbis with Safari 18.4+
        // but older versions would not support it
        outputFileName = Path.ChangeExtension(outputFileName, AudioHelper.GetExtension(ConversionFormat.Mp3));
        Directory.CreateDirectory(Path.GetDirectoryName(outputFileName)!);
        return DefaultAudioProfile.ConvertToFormat(content, ConversionFormat.Mp3, quality, outputFileName);
    }
}
