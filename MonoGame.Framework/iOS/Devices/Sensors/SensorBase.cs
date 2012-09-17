using System;

namespace Microsoft.Devices.Sensors
{
	public abstract class SensorBase<TSensorReading> : IDisposable
		where TSensorReading : ISensorReading
	{
		private TimeSpan timeBetweenUpdates;
		public TSensorReading CurrentValue { get; protected set; }
		public bool IsDataValid { get; protected set; }
		public TimeSpan TimeBetweenUpdates
		{
			get { return this.timeBetweenUpdates; }
			set
			{
				if (this.timeBetweenUpdates != value)
				{
					this.timeBetweenUpdates = value;
					if (this.TimeBetweenUpdatesChanged != null)
						this.TimeBetweenUpdatesChanged(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler<SensorReadingEventArgs<TSensorReading>> CurrentValueChanged;
		protected event EventHandler<EventArgs> TimeBetweenUpdatesChanged;

		public SensorBase()
		{
			this.TimeBetweenUpdates = TimeSpan.FromMilliseconds(2);
		}

		public virtual void Dispose() { }

		public abstract void Start();

		public abstract void Stop();

		protected void FireOnCurrentValueChanged(object sender, SensorReadingEventArgs<TSensorReading> sample)
		{
			if (this.CurrentValueChanged != null)
				this.CurrentValueChanged(this, sample);
		}
	}
}

