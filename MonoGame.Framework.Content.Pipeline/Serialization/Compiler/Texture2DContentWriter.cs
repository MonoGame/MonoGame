// MonoGame - Copyright (C) The MonoGame Team
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
            SurfaceFormat format;
            var bmpContent = value.Faces[0][0];
            if (!bmpContent.TryGetFormat(out format))
                throw new Exception("Couldn't get Format for TextureContent.");

            output.Write((int)format);
            output.Write(bmpContent.Width);
            output.Write(bmpContent.Height);

            // TODO: is this correct?
            var mipCount = value.Faces.Count * value.Faces[0].Count;
            output.Write(mipCount); 

            foreach(var chain in value.Faces)
            {
                foreach (var face in chain)
                {
                    var faceData = face.GetPixelData();
                    output.Write(faceData.Length);
                    output.Write(faceData);
                }
            }
        }
    }
}
