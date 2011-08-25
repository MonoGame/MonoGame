using System;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{

	internal class SpriteEffect : Effect
	{
		internal SpriteEffect (GraphicsDevice graphicsDevice) : base (graphicsDevice)
		{
			// We only create the fragment code for now
			// There needs to be a vertex shader created as well as per the Microsoft BaseEffects
			CreateFragmentShaderFromSource (SpriteEffectCode.SpriteEffectFragmentCode());
			DefineTechnique ("SpriteBatch", "", 0, 0);
			CurrentTechnique = Techniques ["SpriteBatch"];
		}	

	}
}

