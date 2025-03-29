using System;

using Microsoft.Xna.Framework;

using CoreMotion;
using Foundation;

namespace MonoGame.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides methods to access accelerometer data from the device
    /// </summary>
	public sealed partial class Accelerometer : SensorBase<AccelerometerReading>
	{
		static readonly int MaxSensorCount = 10;
		static int instanceCount;
		private static bool started = false;
		private static SensorState state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;
		static readonly CMMotionManager motionManager = new CMMotionManager();

        /// <summary>
        /// Check if an accelerometer is supported on the current device
        /// </summary>
        /// <returns>true if an accelerometer is supported</returns>
		internal static bool PlatformIsSupported()
		{
			return motionManager.AccelerometerAvailable;
		}

        /// <summary>
        /// Check the current state of the accelerometer
        /// </summary>
        /// <returns>Returns current <see cref="SensorState">SensorState</see></returns>
		public SensorState PlatformSensorState()
		{
			return state;
		}

        private static event CMAccelerometerHandler readingChanged;

        /// <summary>
        /// Create a new instance of Accelerometer
        /// </summary>
        /// <exception cref="AccelerometerFailedException">No default sensor is found</exception>
        /// <exception cref="SensorFailedException">The maximum limit of Accelerometer instances has been reached (10)</exception>
		internal void PlatformAccelerometer()
		{
			if (!IsSupported)
				throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
			else if (instanceCount >= MaxSensorCount)
				throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");

			++instanceCount;

            this.TimeBetweenUpdatesChanged += this.UpdateInterval;
            readingChanged += ReadingChangedHandler;

        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
		internal void PlatformDispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					if (started)
						Stop();
					--instanceCount;
				}
			}
		}

        /// <summary>
        /// Begin collecting accelerometer data
        /// </summary>
        /// <exception cref="AccelerometerFailedException">Accelerometer is already started</exception>
		internal void PlatformStart()
		{
			if (started == false)
			{
				motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, AccelerometerHandler);
				started = true;
				state = SensorState.Ready;
			}
			else
				throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);
		}

        /// <summary>
        /// Stop collection accelerometer data
        /// </summary>
		internal void PlatformStop()
		{
			motionManager.StopAccelerometerUpdates();
			started = false;
			state = SensorState.Disabled;
		}

        private void AccelerometerHandler(CMAccelerometerData data, NSError error)
        {
            readingChanged(data, error);
        }

		private void ReadingChangedHandler(CMAccelerometerData data, NSError error)
		{
			AccelerometerReading reading = new AccelerometerReading();
			this.IsDataValid = error == null;
			if (this.IsDataValid)
			{
				this.IsDataValid = true;
				reading.Acceleration = new Vector3((float)data.Acceleration.X, (float)data.Acceleration.Y, (float)data.Acceleration.Z);
				reading.Timestamp = DateTime.UtcNow;
				this.CurrentValue = reading;
				this.IsDataValid = error == null;
			}
		}

		private void UpdateInterval(object sender, EventArgs args)
		{
			motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
		}

        internal static void PlatformInitialize()
		{
		}
    }
}