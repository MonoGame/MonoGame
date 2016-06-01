// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class EffectMaterialWriter : BuiltInContentWriter<EffectMaterialContent>
    {
        protected internal override void Write(ContentWriter output, EffectMaterialContent value)
        {
            output.WriteExternalReference(value.CompiledEffect);
            var dict = new Dictionary<string, object>();
            foreach (var item in value.Textures)
            {
                dict.Add(item.Key, item.Value);
            }
            foreach (var item in value.OpaqueData)
            {
                if (item.Key != EffectMaterialContent.EffectKey && item.Key != EffectMaterialContent.CompiledEffectKey)
                    dict.Add(item.Key, item.Value);
            }
            output.WriteObject(dict);
        }
    }
}