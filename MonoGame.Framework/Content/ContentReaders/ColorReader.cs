// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content
{
	internal class ColorReader : ContentTypeReader<Color>
	{
		public ColorReader ()
		{
		}

		protected internal override Color Read (ContentReader input, Color existingInstance)
		{
            // Read RGBA as four separate bytes to make sure we comply with XNB format document
            byte r = input.ReadByte();
            byte g = input.ReadByte();
            byte b = input.ReadByte();
            byte a = input.ReadByte();
            return new Color(r, g, b, a);
		}
	}
}
