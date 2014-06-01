using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class TextureReader : ContentTypeReader<Texture>
	{
		protected internal override Texture Read(ContentReader reader, Texture existingInstance)
		{
			return existingInstance;
		}
	}
}