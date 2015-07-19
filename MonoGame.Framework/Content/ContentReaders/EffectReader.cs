// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class EffectReader : ContentTypeReader<Effect>
    {
        public EffectReader()
        {
        }

        protected internal override Effect Read(ContentReader input, Effect existingInstance)
        {
            int count = input.ReadInt32();
            
            var effect = new Effect(input.GraphicsDevice,input.ReadBytes(count));
            effect.Name = input.AssetName;
            
            return effect;
        }
    }
}