// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// The exception that may be thrown during a call to Start() or Stop(). The Message field describes the reason for the exception and the ErrorId field contains the error code from the underlying native code implementation of the accelerometer framework.
    /// </summary>
    public class AccelerometerFailedException : SensorFailedException
    {
        /// <summary>
        /// Initializes a new instance of AccelerometerFailedException
        /// </summary>
        /// <param name="message">The descriptive reason for the exception</param>
        /// <param name="errorId">The native error code that caused the exception</param>
        internal AccelerometerFailedException(string message, int errorId)
            : base(message)
        {
            ErrorId = errorId;
        }
    }
}