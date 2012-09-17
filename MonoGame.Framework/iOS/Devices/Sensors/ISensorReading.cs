using System;

namespace Microsoft.Devices.Sensors
{
	public interface ISensorReading
	{
		DateTimeOffset Timestamp { get; }
	}
}

