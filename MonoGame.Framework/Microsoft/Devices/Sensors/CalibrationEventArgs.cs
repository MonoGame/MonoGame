using System;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides data for Calibrate and events.
    /// </summary>
    public class CalibrationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the CalibrationEventArgs class.
        /// </summary>
        /// <remarks>
        /// Obtain a CalibrationEventArgs object by implementing a handler for the Compass.Calibrate event.
        /// </remarks>
        public CalibrationEventArgs()
        {
        }
    }
}