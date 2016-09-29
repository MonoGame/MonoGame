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
            output.Write(value.DiffuseColor ?? new Vector3(1, 1, 1));
            output.Write(value.EmissiveColor ?? new Vector3(0, 0, 0));
            output.Write(value.SpecularColor ?? new Vector3(1, 1, 1));
            output.Write(value.SpecularPower ?? 16);
            output.Write(value.Alpha ?? 1);
            output.Write(value.VertexColorEnabled ?? false);
        }
    }
}
