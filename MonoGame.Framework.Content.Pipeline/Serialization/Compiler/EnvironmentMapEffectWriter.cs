// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

[ContentTypeWriter]
class EnvironmentMapEffectWriter : BuiltInContentWriter<EnvironmentMapMaterialContent>
{
    protected override void Write(ContentWriter output, EnvironmentMapMaterialContent value)
    {
        output.WriteExternalReference(value.Textures.ContainsKey(EnvironmentMapMaterialContent.TextureKey) ? value.Texture : null);
        output.WriteExternalReference(value.Textures.ContainsKey(EnvironmentMapMaterialContent.EnvironmentMapKey) ? value.EnvironmentMap : null);
        output.Write(value.EnvironmentMapAmount ?? 1.0f);
        output.Write(value.EnvironmentMapSpecular ?? Vector3.Zero);
        output.Write(value.FresnelFactor ?? 1.0f);
        output.Write(value.DiffuseColor ?? Vector3.One);
        output.Write(value.EmissiveColor ?? Vector3.Zero);
        output.Write(value.Alpha ?? 1.0f);
    }
}
