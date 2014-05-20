// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class BasicEffectWriter : BuiltInContentWriter<BasicMaterialContent>
    {
        protected internal override void Write(ContentWriter output, BasicMaterialContent value)
        {
            output.WriteExternalReference(value.Textures.ContainsKey(BasicMaterialContent.TextureKey) ? value.Texture : null);
            output.Write(value.DiffuseColor.GetValueOrDefault());
            output.Write(value.EmissiveColor.GetValueOrDefault());
            output.Write(value.SpecularColor.GetValueOrDefault());
            output.Write(value.SpecularPower.GetValueOrDefault());
            output.Write(value.Alpha.GetValueOrDefault());
            output.Write(value.VertexColorEnabled.GetValueOrDefault());
        }
    }
}
