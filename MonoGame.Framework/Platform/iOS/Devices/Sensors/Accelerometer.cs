using System;

using Microsoft.Xna.Framework;

#if !__TVOS__
using CoreMotion;
#endif

using Foundation;

namespace Microsoft.Devices.Sensors
{
	public sealed class Accelerometer : SensorBase<AccelerometerReading>
	{
        static readonly int MaxSensorCount = 10;
        static int instanceCount;
		private static bool started = false;
		private static SensorState state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;

		public static bool IsSupported
		{
#if __TVOS__
            get { return false; }
#else
            get { return motionManager.AccelerometerAvailable; }
#endif
        }

		public SensorState State
		{
			get { return state; }
		}

#if !__TVOS__
		private static event CMAccelerometerHandler readingChanged;

		public Accelerometer()
		{
			if (!IsSupported)
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
            else if (instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Accelerometer class per application has been exceeded.");

            ++instanceCount;

			this.TimeBetweenUpdatesChanged += this.UpdateInterval;
			readingChanged += ReadingChangedHandler;

		}

        protected override void Dispose (bool disposing)
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
            base.Dispose(disposing);
        }

		public override void Start()
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

		public override void Stop()
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
#else // !__TVOS__
		public override void Start()
        {
        }

        public override void Stop()
        {
        }

#endif
    }
}

