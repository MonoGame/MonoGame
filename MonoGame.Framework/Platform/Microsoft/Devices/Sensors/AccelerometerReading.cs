// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors
{
	public struct AccelerometerReading : ISensorReading
	{
		public Vector3 Acceleration { get; internal set; }
		public DateTimeOffset Timestamp { get; internal set; }
	}
}

