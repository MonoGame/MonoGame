// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class DualTextureEffectWriter : BuiltInContentWriter<DualTextureMaterialContent>
    {
        protected internal override void Write(ContentWriter output, DualTextureMaterialContent value)
        {
            output.WriteExternalReference(value.Textures.ContainsKey(DualTextureMaterialContent.TextureKey) ? value.Texture : null);
            output.WriteExternalReference(value.Textures.ContainsKey(DualTextureMaterialContent.Texture2Key) ? value.Texture2 : null);
            output.Write(value.DiffuseColor.HasValue ? value.DiffuseColor.Value : Vector3.One);
            output.Write(value.Alpha.HasValue ? value.Alpha.Value : 1.0f);
            output.Write(value.VertexColorEnabled.HasValue ? value.VertexColorEnabled.Value : false);
        }
    }
}
