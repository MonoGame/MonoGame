using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
	public class TextureReader : ContentTypeReader<Texture>
	{
		protected internal override Texture Read(ContentReader reader, Texture existingInstance)
		{
			return existingInstance;
		}
	}
}