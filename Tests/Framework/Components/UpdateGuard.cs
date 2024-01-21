// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGame.Tests.Components {
	class UpdateGuard {
		private int _lastDrawNumberOfUpdate = -1;

		public bool ShouldUpdate (FrameInfo frameInfo)
		{
			if (_lastDrawNumberOfUpdate == frameInfo.DrawNumber)
				return false;
			_lastDrawNumberOfUpdate = frameInfo.DrawNumber;
			return true;
		}
	}
}
