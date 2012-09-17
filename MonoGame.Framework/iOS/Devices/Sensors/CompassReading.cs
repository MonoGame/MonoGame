using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors
{
    public struct CompassReading : ISensorReading
    {
        public double HeadingAccuracy { get; internal set; }
        public double MagneticHeading { get; internal set; }
        public Vector3 MagnetometerReading { get; internal set; }
        public DateTimeOffset Timestamp { get; internal set; }
        public double TrueHeading { get; internal set; }
    }
}
