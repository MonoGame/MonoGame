// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using System;

namespace Microsoft.Devices.Sensors
{
	public abstract class SensorBase<TSensorReading> : IDisposable
		where TSensorReading : ISensorReading
	{
#if IOS
        [CLSCompliant(false)]
        protected static readonly CoreMotion.CMMotionManager motionManager = new CoreMotion.CMMotionManager();
#endif
        bool disposed;
		private TimeSpan timeBetweenUpdates;
	    private TSensorReading currentValue;
        private SensorReadingEventArgs<TSensorReading> eventArgs = new SensorReadingEventArgs<TSensorReading>(default(TSensorReading));

		public TSensorReading CurrentValue 
        {
            get { return currentValue; }
		    protected set
		    {
		        currentValue = value;

                var handler = CurrentValueChanged;

                if (handler != null)
                {
                    eventArgs.SensorReading = value;
                    handler(this, eventArgs);
                }
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
					EventHelpers.Raise(this, TimeBetweenUpdatesChanged, EventArgs.Empty);
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

