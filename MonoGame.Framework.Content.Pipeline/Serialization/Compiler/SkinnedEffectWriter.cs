// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class SkinnedEffectWriter : BuiltInContentWriter<SkinnedMaterialContent>
    {
        protected internal override void Write(ContentWriter output, SkinnedMaterialContent value)
        {
            output.WriteExternalReference(value.Textures.ContainsKey(SkinnedMaterialContent.TextureKey) ? value.Texture : null);
            output.Write(value.WeightsPerVertex.GetValueOrDefault(4));
            output.Write(value.DiffuseColor.HasValue ? value.DiffuseColor.Value : Vector3.One);
            output.Write(value.EmissiveColor.HasValue ? value.EmissiveColor.Value : Vector3.Zero);
            output.Write(value.SpecularColor.HasValue ? value.SpecularColor.Value : Vector3.Zero);
            output.Write(value.SpecularPower.HasValue ? value.SpecularPower.Value : 0);
            output.Write(value.Alpha.HasValue ? value.Alpha.Value : 1.0f);
        }
    }
}
