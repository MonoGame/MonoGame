// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device's gyrometer sensor.
    /// </summary>
    public sealed partial class Gyrometer : SensorBase<GyrometerReading>
    {
        static readonly int MaxSensorCount = 10;
        static SensorManager sensorManager;
        static Sensor sensor;
        SensorListener listener;
        SensorState state;
        bool started = false;
        static int instanceCount;

        /// <summary>
        /// Gets or sets whether the device on which the application is running supports the gyrometer sensor.
        /// </summary>

        public static bool PlatformIsSupported()
        {
            if (sensorManager == null)
                Initialize();
            return sensor != null;
        }

        /// <summary>
        /// Gets the current state of the gyrometer. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState PlatformSensorState()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (sensorManager == null)
            {
                Initialize();
                state = sensor != null ? SensorState.Initializing : SensorState.NotSupported;
            }
            return state;
        }

        /// <summary>
        /// Creates a new instance of the Gyrometer object.
        /// </summary>
        internal void PlatformGyrometer()
        {
            if (instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Gyrometer class per application has been exceeded.");
            ++instanceCount;

            state = sensor != null ? SensorState.Initializing : SensorState.NotSupported;
            listener = new SensorListener();
        }

        /// <summary>
        /// Initializes the platform resources required for the gyrometer sensor.
        /// </summary>
        static void PlatformInitialize()
        {
            sensorManager = (SensorManager)Game.Activity.GetSystemService(Context.SensorService);
            sensor = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
        }

        void ActivityPaused(object sender, EventArgs eventArgs)
        {
            sensorManager.UnregisterListener(listener, sensor);
        }

        void ActivityResumed(object sender, EventArgs eventArgs)
        {
            sensorManager.RegisterListener(listener, sensor, SensorDelay.Game);
        }

        /// <summary>
        /// Starts data acquisition from the gyrometer.
        /// </summary>
        internal void PlatformStart()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (sensorManager == null)
                Initialize();
            if (started == false)
            {
                if (sensorManager != null && sensor != null)
                {
                    listener.gyrometer = this;
                    sensorManager.RegisterListener(listener, sensor, SensorDelay.Game);
                    // So the system can pause and resume the sensor when the activity is paused
                    AndroidGameActivity.Paused += ActivityPaused;
                    AndroidGameActivity.Resumed += ActivityResumed;
                }
                else
                {
                    throw new GyrometerFailedException("Failed to start gyrometer data acquisition. No default sensor found.", -1);
                }
                started = true;
                state = SensorState.Ready;
                return;
            }
            else
            {
                throw new GyrometerFailedException("Failed to start gyrometer data acquisition. Data acquisition already started.", -1);
            }
        }

        /// <summary>
        /// Stops data acquisition from the gyrometer.
        /// </summary>
        internal void PlatformStop()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (started)
            {
                if (sensorManager != null && sensor != null)
                {
                    AndroidGameActivity.Paused -= ActivityPaused;
                    AndroidGameActivity.Resumed -= ActivityResumed;
                    sensorManager.UnregisterListener(listener, sensor);
                    listener.gyrometer = null;
                }
            }
            started = false;
            state = SensorState.Disabled;
        }

        internal void PlatformDispose(bool disposing)
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
                        sensor = null;
                        sensorManager = null;
                    }
                }
            }
        }

        class SensorListener : Java.Lang.Object, ISensorEventListener
        {
            internal Gyrometer gyrometer;

            public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
                //do nothing
            }

            public void OnSensorChanged(SensorEvent e)
            {
                try
                {
                    if (e != null && e.Sensor.Type == SensorType.Gyrometer && gyrometer != null)
                    {
                        var values = e.Values;
                        try
                        {
                            GyrometerReading reading = new GyrometerReading();
                            gyrometer.IsDataValid = (values != null && values.Count == 3);
                            if (gyrometer.IsDataValid)
                            {
                                const float gravity = Android.Hardware.SensorManager.GravityEarth;
                                reading.AngularVelocity = new Vector3(values[0], values[1], values[2]) / gravity;
                                reading.Timestamp = DateTime.UtcNow;
                            }
                            gyrometer.CurrentValue = reading;
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
    }
}
