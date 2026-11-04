// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Devices.Sensors
{
    /// <summary>
    /// Stores values from a gyrometer event
    /// </summary>
	public struct GyrometerReading : ISensorReading
	{
        /// <summary>
        /// Angular Velocity vector
        /// </summary>
		public Vector3 AngularVelocity { get; internal set; }
        /// <summary>
        /// Timestamp of the gyrometer reading
        /// </summary>
		public DateTimeOffset Timestamp { get; internal set; }
	}
}

