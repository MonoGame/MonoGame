// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Content;
using Android.Hardware;
using Microsoft.Xna.Framework;

namespace Microsoft.Devices.Sensors
{
    /// <summary>
    /// Provides Android applications access to the device's compass sensor.
    /// </summary>
    public sealed class Compass : SensorBase<CompassReading>
    {
        static readonly int MaxSensorCount = 10;
        static SensorManager sensorManager;
        static Sensor sensorMagneticField;
        static Sensor sensorAccelerometer;
        SensorListener listener;
        SensorState state;
        bool started = false;
        static int instanceCount;

        /// <summary>
        /// Gets whether the device on which the application is running supports the compass sensor.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                if (sensorManager == null)
                    Initialize();
                return sensorMagneticField != null;
            }
        }

        /// <summary>
        /// Gets the current state of the compass. The value is a member of the SensorState enumeration.
        /// </summary>
        public SensorState State
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().Name);
                if (sensorManager == null)
                    Initialize();
                return state;
            }
        }

        /// <summary>
        /// Creates a new instance of the Compass object.
        /// </summary>
        public Compass()
        {
            if (instanceCount >= MaxSensorCount)
                throw new SensorFailedException("The limit of 10 simultaneous instances of the Compass class per application has been exceeded.");
            ++instanceCount;

            state = sensorMagneticField != null ? SensorState.Initializing : SensorState.NotSupported;
            listener = new SensorListener();
        }

        /// <summary>
        /// Initializes the platform resources required for the compass sensor.
        /// </summary>
        static void Initialize()
        {
            sensorManager = (SensorManager)Game.Activity.GetSystemService(Context.SensorService);
            sensorMagneticField = sensorManager.GetDefaultSensor(SensorType.MagneticField);
            sensorAccelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
        }

        void ActivityPaused(object sender, EventArgs eventArgs)
        {
            sensorManager.UnregisterListener(listener, sensorMagneticField);
            sensorManager.UnregisterListener(listener, sensorAccelerometer);
        }

        void ActivityResumed(object sender, EventArgs eventArgs)
        {
            sensorManager.RegisterListener(listener, sensorAccelerometer, SensorDelay.Game);
            sensorManager.RegisterListener(listener, sensorMagneticField, SensorDelay.Game);
        }

        /// <summary>
        /// Starts data acquisition from the compass.
        /// </summary>
        public override void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
            if (sensorManager == null)
                Initialize();
            if (started == false)
            {
                if (sensorManager != null && sensorMagneticField != null && sensorAccelerometer != null)
                {
                    listener.compass = this;
                    sensorManager.RegisterListener(listener, sensorMagneticField, SensorDelay.Game);
                    sensorManager.RegisterListener(listener, sensorAccelerometer, SensorDelay.Game);
                }
                else
                {
                    throw new SensorFailedException("Failed to start compass data acquisition. No default sensor found.");
                }
                started = true;
                state = SensorState.Ready;
                return;
            }
            else
            {
                throw new SensorFailedException("Failed to start compass data acquisition. Data acquisition already started.");
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
                if (sensorManager != null && sensorMagneticField != null && sensorAccelerometer != null)
                {
                    sensorManager.UnregisterListener(listener, sensorAccelerometer);
                    sensorManager.UnregisterListener(listener, sensorMagneticField);
                    listener.compass = null;
                }
            }
            started = false;
            state = SensorState.Disabled;
        }

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
                        sensorAccelerometer = null;
                        sensorMagneticField = null;
                        sensorManager = null;
                    }
                }
            }
            base.Dispose(disposing);
        }

        class SensorListener : Java.Lang.Object, ISensorEventListener
        {
            internal Compass compass;
            float[] valuesAccelerometer;
            float[] valuesMagenticField;
            float[] matrixR;
            float[] matrixI;
            float[] matrixValues;

            public SensorListener()
            {
                valuesAccelerometer = new float[3];
                valuesMagenticField = new float[3];
                matrixR = new float[9];
                matrixI = new float[9];
                matrixValues = new float[3];
            }

            public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
            {
                //do nothing
            }

            public void OnSensorChanged(SensorEvent e)
            {
                try
                {
                    switch (e.Sensor.Type)
                    {
                        case SensorType.Accelerometer:
                            valuesAccelerometer[0] = e.Values[0];
                            valuesAccelerometer[1] = e.Values[1];
                            valuesAccelerometer[2] = e.Values[2];
                            break;

                        case SensorType.MagneticField:
                            valuesMagenticField[0] = e.Values[0];
                            valuesMagenticField[1] = e.Values[1];
                            valuesMagenticField[2] = e.Values[2];
                            break;
                    }

                    compass.IsDataValid = SensorManager.GetRotationMatrix(matrixR, matrixI, valuesAccelerometer, valuesMagenticField);
                    if (compass.IsDataValid)
                    {
                        SensorManager.GetOrientation(matrixR, matrixValues);
                        CompassReading reading = new CompassReading();
                        reading.MagneticHeading = matrixValues[0];
                        Vector3 magnetometer = new Vector3(valuesMagenticField[0], valuesMagenticField[1], valuesMagenticField[2]);
                        reading.MagnetometerReading = magnetometer;
                        // We need the magnetic declination from true north to calculate the true heading from the magnetic heading.
                        // On Android, this is available through Android.Hardware.GeomagneticField, but this requires your geo position.
                        reading.TrueHeading = reading.MagneticHeading;
                        reading.Timestamp = DateTime.UtcNow;
                        compass.CurrentValue = reading;
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

