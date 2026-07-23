// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using System;

namespace MonoGame.Framework.Devices.Sensors
{

    /// <summary>
    /// The base class for the use of physical sensors attached to the device the player uses,
    /// which can be utilized for gameplay events.
    /// </summary>
    /// <typeparam name="TSensorReading">The reading value from the sensor.</typeparam>
	public abstract class SensorBase<TSensorReading> : IDisposable
		where TSensorReading : ISensorReading
	{
        bool disposed;
		private TimeSpan timeBetweenUpdates;
	    private TSensorReading currentValue;
        private SensorReadingEventArgs<TSensorReading> eventArgs = new SensorReadingEventArgs<TSensorReading>(default(TSensorReading));

        /// <summary>
        /// The current reading from the sensor.
        /// </summary>
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

        /// <summary>
        /// Whether the data is deemed valid by the device or is outside the device's error range.  
        /// </summary>
		public bool IsDataValid { get; protected set; }

        /// <summary>
        /// The time between the two most recent updates of the sensor.   
        /// </summary>
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

        /// <summary>
        /// Invoked when the <see cref="CurrentValue"/> property changes. 
        /// </summary>
		public event EventHandler<SensorReadingEventArgs<TSensorReading>> CurrentValueChanged;

        /// <summary>
        /// Invoked when the <see cref="TimeBetweenUpdates"/> property changes.  
        /// </summary>
		protected event EventHandler<EventArgs> TimeBetweenUpdatesChanged;

        /// <summary>
        /// Whether the sensor has been disposed. 
        /// </summary>
        protected bool IsDisposed { get { return disposed; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected SensorBase()
		{
			this.TimeBetweenUpdates = TimeSpan.FromMilliseconds(2);
		}

        /// <summary>
        /// Default deconstructor.
        /// </summary>
        ~SensorBase()
        {
            Dispose(false);
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Starts data acquisition from the sensor, allowing it to begin updating its value and firing events.
        /// </summary>
		public abstract void Start();

        /// <summary>
        /// Stops data acquisition from the sensor, preventing it from updating its value and firing events.
        /// </summary>
		public abstract void Stop();
	}
}

