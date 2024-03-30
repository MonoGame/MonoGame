// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Devices.Sensors
{
	public interface ISensorReading
	{
		DateTimeOffset Timestamp { get; }
	}
}

