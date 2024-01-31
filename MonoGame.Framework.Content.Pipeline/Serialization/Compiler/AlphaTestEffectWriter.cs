// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class AlphaTestEffectWriter : BuiltInContentWriter<AlphaTestMaterialContent>
    {
        protected internal override void Write(ContentWriter output, AlphaTestMaterialContent value)
        {
            output.WriteExternalReference(value.Textures.ContainsKey(AlphaTestMaterialContent.TextureKey) ? value.Texture : null);
            output.Write((int)(value.AlphaFunction.HasValue ? value.AlphaFunction.Value : CompareFunction.Greater));
            output.Write((int)(value.ReferenceAlpha.HasValue ? value.ReferenceAlpha.Value : 0));
            output.Write(value.DiffuseColor.GetValueOrDefault());
            output.Write(value.Alpha.GetValueOrDefault());
            output.Write(value.VertexColorEnabled.GetValueOrDefault());
        }
    }
}
