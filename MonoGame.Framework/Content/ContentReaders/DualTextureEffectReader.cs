using System;

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    class DualTextureEffectReader : ContentTypeReader<DualTextureEffect>
    {
        protected internal override DualTextureEffect Read(ContentReader input, DualTextureEffect existingInstance)
        {
			DualTextureEffect effect = new DualTextureEffect(input.GraphicsDevice);
			effect.Texture = input.ReadExternalReference<Texture>() as Texture2D;
			effect.Texture2 = input.ReadExternalReference<Texture>() as Texture2D;
			effect.DiffuseColor = input.ReadVector3 ();
			effect.Alpha = input.ReadSingle ();
			effect.VertexColorEnabled = input.ReadBoolean ();
			return effect;
		}
	}
}

