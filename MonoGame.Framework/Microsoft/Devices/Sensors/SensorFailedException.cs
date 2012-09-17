using System;

namespace Microsoft.Devices.Sensors
{
	public class SensorFailedException : Exception
	{
		public int ErrorId { get; protected set; }

        internal SensorFailedException(string message)
            : base(message)
        {
        }
    }
}

