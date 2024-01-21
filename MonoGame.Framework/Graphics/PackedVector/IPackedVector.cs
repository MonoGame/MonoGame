// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// Author: Kenneth James Pouncey

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.packedvector.ipackedvector.aspx
	public interface IPackedVector
	{
		void PackFromVector4 (Vector4 vector);

		Vector4 ToVector4 ();
	}
	
	// PackedVector Generic interface
	// http://msdn.microsoft.com/en-us/library/bb197661.aspx
	public interface IPackedVector<TPacked> : IPackedVector
	{
		TPacked PackedValue { get; set; }
	}

}



