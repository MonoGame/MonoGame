// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    internal class TextureCubeWriter : BuiltInContentWriter<TextureCubeContent>
    {
        protected internal override void Write(ContentWriter output, TextureCubeContent value)
        {
            var mipmaps0 = value.Faces[0];  // Mipmap chain of face 0 (+X).
            var level0 = mipmaps0[0];       // Most detailed mipmap level of face 0.

            SurfaceFormat format;
            if (!level0.TryGetFormat(out format))
                throw new Exception("Couldn't get format for TextureCubeContent.");

            output.Write((int)format);      // Surface format
            output.Write(level0.Width);     // Cube map size
            output.Write(mipmaps0.Count);   // Number of mipmap levels

            // The number of faces in TextureCubeContent is guaranteed to be 6.
            foreach (var mipmaps in value.Faces)
            {
                foreach (var level in mipmaps)
                {
                    byte[] pixelData = level.GetPixelData();
                    output.Write(pixelData.Length);
                    output.Write(pixelData);
                }
            }
        }
    }
}
