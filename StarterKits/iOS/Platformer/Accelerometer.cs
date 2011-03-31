#region File Description
//-----------------------------------------------------------------------------
// Accelerometer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Platformer
{
    /// <summary>
    /// A static encapsulation of accelerometer input to provide games with a polling-based
    /// accelerometer system.
    /// </summary>
    public static class Accelerometer
    {
#if WINDOWS_PHONE
        // the accelerometer sensor on the device
        private static Microsoft.Devices.Sensors.Accelerometer accelerometer = new Microsoft.Devices.Sensors.Accelerometer();
        
        // we need an object for locking because the ReadingChanged event is fired
        // on a different thread than our game
        private static object threadLock = new object();

        // we use this to keep the last known value from the accelerometer callback
        private static Vector3 nextValue = new Vector3();
#endif

        // we want to prevent the Accelerometer from being initialized twice.
        private static bool isInitialized = false;

        // whether or not the accelerometer is active
        private static bool isActive = false;

        /// <summary>
        /// Initializes the Accelerometer for the current game. This method can only be called once per game.
        /// </summary>
        public static void Initialize()
        {
            // make sure we don't initialize the Accelerometer twice
            if (isInitialized)
            {
                throw new InvalidOperationException("Initialize can only be called once");
            }

#if WINDOWS_PHONE
            // try to start the sensor only on devices, catching the exception if it fails            
            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Device)            
            {
                try
                {
                    accelerometer.ReadingChanged += new EventHandler<Microsoft.Devices.Sensors.AccelerometerReadingEventArgs>(sensor_ReadingChanged);
                    accelerometer.Start();
                    isActive = true;
                }
                catch (Microsoft.Devices.Sensors.AccelerometerFailedException)
                {
                    isActive = false;
                }
            }
            else
            {
                // we always return isActive on emulator because we use the arrow
                // keys for simulation which is always available.
                isActive = true;
            }
#endif

            // remember that we are initialized
            isInitialized = true;
        }
        
#if WINDOWS_PHONE
        private static void sensor_ReadingChanged(object sender, Microsoft.Devices.Sensors.AccelerometerReadingEventArgs e)
        {
            // store the accelerometer value in our variable to be used on the next Update
            lock (threadLock)
            {
                nextValue = new Vector3((float)e.X, (float)e.Y, (float)e.Z);
            }
        }
#endif

        /// <summary>
        /// Gets the current state of the accelerometer.
        /// </summary>
        /// <returns>A new AccelerometerState with the current state of the accelerometer.</returns>
        public static AccelerometerState GetState()
        {
            // make sure we've initialized the Accelerometer before we try to get the state
            if (!isInitialized)
            {
                throw new InvalidOperationException("You must Initialize before you can call GetState");
            }

            // create a new value for our state
            Vector3 stateValue = new Vector3();

#if WINDOWS_PHONE
            // if the accelerometer is active
            if (isActive)
            {
                if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Device)
                {
                    // if we're on device, we'll just grab our latest reading from the accelerometer
                    lock (threadLock)
                    {
                        stateValue = nextValue;
                    }
                }
                else
                {
                    // if we're in the emulator, we'll generate a fake acceleration value using the arrow keys
                    // press the pause/break key to toggle keyboard input for the emulator
                    KeyboardState keyboardState = Keyboard.GetState();

                    stateValue.Z = -1;

                    if (keyboardState.IsKeyDown(Keys.Left))
                        stateValue.X--;
                    if (keyboardState.IsKeyDown(Keys.Right))
                        stateValue.X++;
                    if (keyboardState.IsKeyDown(Keys.Up))
                        stateValue.Y++;
                    if (keyboardState.IsKeyDown(Keys.Down))
                        stateValue.Y--;

                    stateValue.Normalize();
                }
            }
#endif

            return new AccelerometerState(stateValue, isActive);
        }
    }

    /// <summary>
    /// An encapsulation of the accelerometer's current state.
    /// </summary>
    public struct AccelerometerState
    {
        /// <summary>
        /// Gets the accelerometer's current value in G-force.
        /// </summary>
        public Vector3 Acceleration { get; private set; }

        /// <summary>
        /// Gets whether or not the accelerometer is active and running.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Initializes a new AccelerometerState.
        /// </summary>
        /// <param name="acceleration">The current acceleration (in G-force) of the accelerometer.</param>
        /// <param name="isActive">Whether or not the accelerometer is active.</param>
        public AccelerometerState(Vector3 acceleration, bool isActive)
            : this()
        {
            Acceleration = acceleration;
            IsActive = isActive;
        }

        /// <summary>
        /// Returns a string containing the values of the Acceleration and IsActive properties.
        /// </summary>
        /// <returns>A new string describing the state.</returns>
        public override string ToString()
        {
            return string.Format("Acceleration: {0}, IsActive: {1}", Acceleration, IsActive);
        }
    }
}
