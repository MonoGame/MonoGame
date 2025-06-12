// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class EnvironmentMapEffectWriter : BuiltInContentWriter<EnvironmentMapMaterialContent>
    {
        protected internal override void Write(ContentWriter output, EnvironmentMapMaterialContent value)
        {
            output.WriteExternalReference(value.Textures.ContainsKey(EnvironmentMapMaterialContent.TextureKey) ? value.Texture : null);
            output.WriteExternalReference(value.Textures.ContainsKey(EnvironmentMapMaterialContent.EnvironmentMapKey) ? value.EnvironmentMap : null);
            output.Write(value.EnvironmentMapAmount.HasValue ? value.EnvironmentMapAmount.Value : 1.0f);
            output.Write(value.EnvironmentMapSpecular.HasValue ? value.EnvironmentMapSpecular.Value : Vector3.Zero);
            output.Write(value.FresnelFactor.HasValue ? value.FresnelFactor.Value : 1.0f);
            output.Write(value.DiffuseColor.HasValue ? value.DiffuseColor.Value : Vector3.One);
            output.Write(value.EmissiveColor.HasValue ? value.EmissiveColor.Value : Vector3.Zero);
            output.Write(value.Alpha.HasValue ? value.Alpha.Value : 1.0f);
        }
    }
}
