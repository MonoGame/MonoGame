// MonoGame - Copyright (C) The MonoGame Team
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
            output.WriteExternalReference(value.Texture);
            output.WriteExternalReference(value.Texture2);
            output.Write(value.DiffuseColor.HasValue ? value.DiffuseColor.Value : Vector3.One);
            output.Write(value.Alpha.HasValue ? value.Alpha.Value : 1.0f);
            output.Write(value.VertexColorEnabled.HasValue ? value.VertexColorEnabled.Value : false);
        }
    }
}
