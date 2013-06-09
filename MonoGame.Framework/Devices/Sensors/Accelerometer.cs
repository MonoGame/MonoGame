using Microsoft.Devices.Sensors;
using System;
using Microsoft.Xna.Framework;

#if ANDROID
using Android.Content;
using Android.Hardware;
#elif iOS
using MonoTouch.CoreMotion;
using MonoTouch.Foundation;
#endif

namespace Microsoft.Devices.Sensors
{
    public sealed class Accelerometer : SensorBase<AccelerometerReading>
    {
        static readonly int MaxSensorCount = 10;
        static int instanceCount;
#if ANDROID
        static SensorManager sensorManager;
        static Sensor sensor;
        SensorListener listener;
#elif iOS
        private static event CMAccelerometerHandler readingChanged;
#endif
        SensorState state;
        static bool started = false;

        /// <summary>
        /// Gets or sets whether the device on which the application is running supports the accelerometer sensor.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
#if ANDROID
                if (sensorManager == null)
                    Initialize();
                return sensor != null;
#elif iOS
                return motionManager.AccelerometerAvailable;
#else
                return false;
#endif
            }
        }


        /// <summary>
        /// Gets the current state of the accelerometer. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState State
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().Name);
#if ANDROID
                if (sensorManager == null)
                    Initialize();
#endif
                return state;
            }
            set
            {
                state = value;
            }
        }


        public Accelerometer()
        {
            if (!IsSupported)
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);

            else if (instanceCount >= MaxSensorCount)
                throw new SensorFailedException(string.Format("The limit of {0} simultaneous instances of the Accelerometer class per application has been exceeded.", MaxSensorCount));

            ++instanceCount;
#if ANDROID
            state = sensor != null ? SensorState.Initializing : SensorState.NotSupported;
            listener = new SensorListener();
#elif iOS
            this.TimeBetweenUpdatesChanged += this.UpdateInterval;
            readingChanged += ReadingChangedHandler;
#endif



        }

        static void Initialize()
        {
#if ANDROID
            sensorManager = (SensorManager)Game.Activity.GetSystemService(Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
#endif
        }

#if ANDROID
        void ActivityPaused(object sender, EventArgs eventArgs)
        {
            sensorManager.UnregisterListener(listener, sensor);
        }

        void ActivityResumed(object sender, EventArgs eventArgs)
        {
            sensorManager.RegisterListener(listener, sensor, SensorDelay.Game);
        }
#endif

        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
#if ANDROID
            if (sensorManager == null)
                Initialize();
#endif
            if (started == false)
            {
#if ANDROID
                if (sensorManager != null && sensor != null)
                {
                    listener.accelerometer = this;
                    AndroidGameActivity.Paused += ActivityPaused;
                    AndroidGameActivity.Resumed += ActivityResumed;
                    sensorManager.RegisterListener(listener, sensor, SensorDelay.Game);
                }
                else
                {
                    throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. No default sensor found.", -1);
                }
#elif iOS
                motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, AccelerometerHandler);
                started = true;
                state = SensorState.Ready;
#endif

                started = true;
                State = SensorState.Ready;
                return;
            }
            else
            {
                throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);
            }
        }

        /// <summary>
        /// Stops data acquisition from the accelerometer.
        /// </summary>
        public override void Stop()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (started)
            {
#if ANDROID
                if (sensorManager != null && sensor != null)
                {
                    AndroidGameActivity.Paused -= ActivityPaused;
                    AndroidGameActivity.Resumed -= ActivityResumed;
                     sensorManager.UnregisterListener(listener, sensor);
                    listener.accelerometer = null;
                }
#elif iOS
			motionManager.StopAccelerometerUpdates();
#endif
            }
            started = false;
            state = SensorState.Disabled;
        }

#if iOS
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
		}
#endif

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (started)
                        Stop();
                    --instanceCount;
                    if (instanceCount == 0)
                    {
#if ANDROID
                        sensor = null;
                        sensorManager = null;
#endif
                    }
                }
            }
            base.Dispose(disposing);
        }


        private void UpdateInterval(object sender, EventArgs args)
        {
#if iOS
            motionManager.AccelerometerUpdateInterval = this.TimeBetweenUpdates.TotalSeconds;
#endif
        }

    }

#if ANDROID
    class SensorListener : Java.Lang.Object, ISensorEventListener
    {



        internal Accelerometer accelerometer;

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {

            //do nothing
        }

        public void OnSensorChanged(SensorEvent e)
        {
            try
            {
                if (e != null && e.Sensor.Type == SensorType.Accelerometer && accelerometer != null)
                {
                    var values = e.Values;
                    try
                    {
                        AccelerometerReading reading = new AccelerometerReading();
                        accelerometer.IsDataValid = (values != null && values.Count == 3);
                        if (accelerometer.IsDataValid)
                        {
                            reading.Acceleration = new Vector3(values[0], values[1], values[2]);
                            reading.Timestamp = DateTime.Now;
                        }
                        accelerometer.CurrentValue = reading;
                    }
                    finally
                    {
                        IDisposable d = values as IDisposable;
                        if (d != null)
                            d.Dispose();
                    }
                }
            }
            catch (NullReferenceException)
            {
                //Occassionally an NullReferenceException is thrown when accessing e.Values??
                // mono    : Unhandled Exception: System.NullReferenceException: Object reference not set to an instance of an object
                // mono    :   at Android.Runtime.JNIEnv.GetObjectField (IntPtr jobject, IntPtr jfieldID) [0x00000] in <filename unknown>:0 
                // mono    :   at Android.Hardware.SensorEvent.get_Values () [0x00000] in <filename unknown>:0
            }
        }
    }
#endif

}
