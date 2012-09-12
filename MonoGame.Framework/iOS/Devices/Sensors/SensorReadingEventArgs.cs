using System;

namespace Microsoft.Devices.Sensors
{
	public class SensorReadingEventArgs<T> : EventArgs
		where T : ISensorReading
	{
		public T SensorReading { get; set; }

		public SensorReadingEventArgs(T sensorReading)
		{
			this.SensorReading = sensorReading;
		}
	}
}

