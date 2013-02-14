using System;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    class SkinnedEffectReader : ContentTypeReader<SkinnedEffect>
    {
        protected internal override SkinnedEffect Read(ContentReader input, SkinnedEffect existingInstance)
        {
            var effect = new SkinnedEffect(input.GraphicsDevice);
			effect.Texture = input.ReadExternalReference<Texture> () as Texture2D;
			effect.WeightsPerVertex = input.ReadInt32 ();
			effect.DiffuseColor = input.ReadVector3 ();
			effect.EmissiveColor = input.ReadVector3 ();
			effect.SpecularColor = input.ReadVector3 ();
			effect.SpecularPower = input.ReadSingle ();
			effect.Alpha = input.ReadSingle ();
            return effect;
        }
    }
}
