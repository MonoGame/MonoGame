// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

