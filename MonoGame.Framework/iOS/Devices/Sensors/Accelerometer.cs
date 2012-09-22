using System;

using Microsoft.Xna.Framework;

using MonoTouch.CoreMotion;
using MonoTouch.Foundation;

namespace Microsoft.Devices.Sensors
{
	public sealed class Accelerometer : SensorBase<AccelerometerReading>
	{
		private static CMMotionManager motionManager = new CMMotionManager();
		private static bool started = false;
		private static SensorState state = IsSupported ? SensorState.Initializing : SensorState.NotSupported;

		public static bool IsSupported
		{
			get { return motionManager.AccelerometerAvailable; }
		}
		public SensorState State
		{
			get { return state; }
		}

		private static event CMAccelerometerHandler readingChanged;

		public Accelerometer()
		{
			if (!IsSupported)
				throw new SensorFailedException();

			this.TimeBetweenUpdatesChanged += this.UpdateInterval;
			readingChanged += ReadingChangedHandler;

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
				throw new SensorFailedException();
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
                reading.Timestamp = DateTime.Now;
                this.CurrentValue = reading;
                this.IsDataValid = error == null;
            }
            FireOnCurrentValueChanged(this, new SensorReadingEventArgs<AccelerometerReading>(reading));
		}

		private void UpdateInterval(object sender, EventArgs args)
		{
			motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
		}
	}
}

