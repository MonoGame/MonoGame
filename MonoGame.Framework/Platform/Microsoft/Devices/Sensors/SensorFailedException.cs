// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

