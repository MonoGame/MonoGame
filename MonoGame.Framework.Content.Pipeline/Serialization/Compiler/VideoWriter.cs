// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

[ContentTypeWriter]
class VideoWriter : BuiltInContentWriter<VideoContent>
{
    protected override void Write(ContentWriter output, VideoContent value)
    {
        output.WriteObject(value.Filename);
        output.WriteObject((int)value.Duration.TotalMilliseconds);
        output.WriteObject(value.Width);
        output.WriteObject(value.Height);
        output.WriteObject(value.FramesPerSecond);
        output.WriteObject((int)value.VideoSoundtrackType);
    }
}
