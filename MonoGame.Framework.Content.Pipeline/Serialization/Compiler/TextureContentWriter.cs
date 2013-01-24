// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    public class TextureContentWriter : ContentTypeWriter<TextureContent>
    {
        protected internal override void Write(ContentWriter output, TextureContent value)
        {
            SurfaceFormat format;
            var bmpContent = value.Faces[0][0];
            if (bmpContent.TryGetFormat(out format))
                throw new Exception("Couldn't get Format for TextureContent.");

            output.WriteObject((int)format);
            output.WriteObject(bmpContent.Width);
            output.WriteObject(bmpContent.Height);

            // TODO: is this correct?
            var mipCount = value.Faces.Count * value.Faces[0].Count;
            output.WriteObject(mipCount); 

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

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TextureReader).AssemblyQualifiedName;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(TextureReader).AssemblyQualifiedName;
        }
    }
}
