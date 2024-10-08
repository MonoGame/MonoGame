#region File Description
//-----------------------------------------------------------------------------
// Accelerometer.cs
//
// MonoGame Foundation Game Platform
// Copyright (C) MonoGame Foundation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using ___SafeGameName___.Core.Localization;
using Microsoft.Xna.Framework;
using System;
#endregion

namespace ___SafeGameName___.Core.Inputs;

/// <summary>
/// A static encapsulation of accelerometer input to provide games with a polling-based
/// accelerometer system.
/// </summary>
public static class Accelerometer
{
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
            throw new InvalidOperationException(Resources.ErrorAccelerometerInitializeOnce);
        }

        // remember that we are initialized
        isInitialized = true;
    }

    /// <summary>
    /// Gets the current state of the accelerometer.
    /// </summary>
    /// <returns>A new AccelerometerState with the current state of the accelerometer.</returns>
    public static AccelerometerState GetState()
    {
        // make sure we've initialized the Accelerometer before we try to get the state
        if (!isInitialized)
        {
            throw new InvalidOperationException(Resources.ErrorAccelerometerMustInitialize);
        }

        // create a new value for our state
        Vector3 stateValue = new Vector3();

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
        return string.Format(Resources.ErrorAccelerometerToString, Acceleration, IsActive);
    }
}
