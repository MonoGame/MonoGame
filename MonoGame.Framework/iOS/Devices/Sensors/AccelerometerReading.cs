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

