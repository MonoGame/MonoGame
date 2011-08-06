using System;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
	// TODO is this class really in xna public interface?
	public class SpriteEffect : Effect
	{
		public SpriteEffect (GraphicsDevice graphicsDevice) : base (graphicsDevice)
		{
			// We only create the fragment code for now
			// There needs to be a vertex shader created as well as per the Microsoft BaseEffects
			CreateFragmentShaderFromSource (SpriteEffectCode.SpriteEffectFragmentCode());
			DefineTechnique ("SpriteBatch", "", 0, 0);
			CurrentTechnique = Techniques ["SpriteBatch"];
		}	

	}
}

