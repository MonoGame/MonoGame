// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Devices.Sensors
{
	public abstract class SensorBase<TSensorReading> : IDisposable
		where TSensorReading : ISensorReading
	{
#if IOS
        protected static readonly MonoTouch.CoreMotion.CMMotionManager motionManager = new MonoTouch.CoreMotion.CMMotionManager();
#endif
        bool disposed;
		private TimeSpan timeBetweenUpdates;
	    private TSensorReading currentValue;

		public TSensorReading CurrentValue 
        {
            get { return currentValue; }
		    protected set
		    {
		        currentValue = value;
		        if (this.CurrentValueChanged != null)
		            this.CurrentValueChanged(this, new SensorReadingEventArgs<TSensorReading>(currentValue));
		    }
		}
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
        protected bool IsDisposed { get { return disposed; } }

		public SensorBase()
		{
			this.TimeBetweenUpdates = TimeSpan.FromMilliseconds(2);
		}

        ~SensorBase()
        {
            Dispose(false);
        }

		public void Dispose()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Derived classes override this method to dispose of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if unmanaged resources are to be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }

		public abstract void Start();

		public abstract void Stop();
	}
}

