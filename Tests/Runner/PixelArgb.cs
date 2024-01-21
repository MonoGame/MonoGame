// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoGame.Tests {
	[StructLayout (LayoutKind.Sequential)]
	struct PixelArgb {
		public const int MaxDelta = 4 * Byte.MaxValue;

		public byte B;
		public byte G;
		public byte R;
		public byte A;

		public unsafe int Delta (PixelArgb* other)
		{
			return
				Math.Abs (B - other->B) +
				Math.Abs (G - other->G) +
				Math.Abs (R - other->R) +
				Math.Abs (A - other->A);
		}
	}
}
