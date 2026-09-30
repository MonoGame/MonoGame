// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

[ContentTypeWriter]
class SkinnedEffectWriter : BuiltInContentWriter<SkinnedMaterialContent>
{
    protected override void Write(ContentWriter output, SkinnedMaterialContent value)
    {
        output.WriteExternalReference(value.Textures.ContainsKey(SkinnedMaterialContent.TextureKey) ? value.Texture : null);
        output.Write(value.WeightsPerVertex.GetValueOrDefault(4));
        output.Write(value.DiffuseColor ?? Vector3.One);
        output.Write(value.EmissiveColor ?? Vector3.Zero);
        output.Write(value.SpecularColor ?? Vector3.Zero);
        output.Write(value.SpecularPower ?? 0);
        output.Write(value.Alpha ?? 1.0f);
    }
}
