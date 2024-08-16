// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class Texture2DWriter : BuiltInContentWriter<Texture2DContent>
    {
        protected internal override void Write(ContentWriter output, Texture2DContent value)
        {
            var mipmaps = value.Faces[0];   // Mipmap chain.
            var level0 = mipmaps[0];        // Most detailed mipmap level.

            SurfaceFormat format;
            if (!level0.TryGetFormat(out format))
                throw new Exception("Couldn't get Format for TextureContent.");

            output.Write((int)format);
            output.Write(level0.Width);
            output.Write(level0.Height);
            output.Write(mipmaps.Count);    // Number of mipmap levels.

            foreach (var level in mipmaps)
            {
                var pixelData = level.GetPixelData();
                output.Write(pixelData.Length);
                output.Write(pixelData);
            }
        }
    }
}
