using System;

namespace Microsoft.Devices.Sensors
{
	public enum SensorState
	{
		NotSupported,
		Ready,
		Initializing,
		NoData,
		NoPermissions,
		Disabled
	}
}

