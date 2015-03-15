// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    [ContentImporter(".mp4", DisplayName = "H.264 Video - MonoGame", DefaultProcessor = "VideoProcessor")]
    public class H264Importer : ContentImporter<VideoContent>
    {
        public H264Importer()
        {
        }

        public override VideoContent Import(string filename, ContentImporterContext context)
        {
            var content = new VideoContent(filename);
            return content;
        }
    }
}
