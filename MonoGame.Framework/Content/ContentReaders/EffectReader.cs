using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class EffectReader : ContentTypeReader<Effect>
    {
        private static EffectPool effectpool;

        public EffectReader()
        {
        }

        protected internal override Effect Read(ContentReader input, Effect existingInstance)
        {
            int count = input.ReadInt32();
            
            return new Effect(input.GraphicsDevice,input.ReadBytes(count));
        }
    }
}