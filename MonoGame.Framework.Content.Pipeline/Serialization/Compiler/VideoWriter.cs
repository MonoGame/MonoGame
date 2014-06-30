// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class VideoWriter : BuiltInContentWriter<VideoContent>
    {
        protected internal override void Write(ContentWriter output, VideoContent value)
        {
            output.Write(value.Filename);
            output.Write((int)value.Duration.TotalMilliseconds);
            output.Write(value.Width);
            output.Write(value.Height);
            output.Write(value.FramesPerSecond);
            output.Write((int)value.VideoSoundtrackType);
        }
    }
}
