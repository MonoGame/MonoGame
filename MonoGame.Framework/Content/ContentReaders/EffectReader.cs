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
            var effect = existingInstance;
            
            int dataSize = (int)input.ReadUInt32();
            byte[] data = input.ContentManager.GetScratchBuffer(dataSize);
            input.Read(data, 0, dataSize);

            if (effect == null)
                effect = new Effect(input.GraphicsDevice, data, 0, dataSize);
            effect.Name = input.AssetName;
            return effect;
        }
    }
}
