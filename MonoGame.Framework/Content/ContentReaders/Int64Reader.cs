// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Content
{
	internal class Int64Reader : ContentTypeReader<long>
	{
		public Int64Reader ()
		{
		}

		protected internal override long Read (ContentReader input, long existingInstance)
		{
			return input.ReadInt64 ();
		}
	}
}
