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
        throw new PipelineException("Streaming audio content is not supported for the WebGL2 platform.");
    }
}
