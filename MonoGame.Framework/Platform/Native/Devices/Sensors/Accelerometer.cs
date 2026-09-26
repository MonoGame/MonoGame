// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Interop;

namespace MonoGame.Framework.Devices.Sensors;

public sealed partial class Accelerometer : SensorBase<AccelerometerReading>
{
    private static readonly List<Accelerometer> s_instances = new List<Accelerometer>();
    private static int s_lastReadingSequence;
    private static int s_startedInstanceCount;

    private bool _started;
    private SensorState _state;

    internal static bool PlatformIsSupported()
    {
        return MGP.Accelerometer_IsSupported() != 0;
    }

    internal SensorState PlatformSensorState()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().Name);

        return _state;
    }

    internal void PlatformAccelerometer()
    {
        _state = PlatformIsSupported() ? SensorState.Disabled : SensorState.NotSupported;
        s_instances.Add(this);
    }

    internal static void PlatformInitialize()
    {
    }

    internal void PlatformStart()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().Name);
        if (!PlatformIsSupported())
            throw new PlatformNotSupportedException();
        if (_started)
            throw new AccelerometerFailedException("Failed to start accelerometer data acquisition. Data acquisition already started.", -1);

        _started = true;
        _state = SensorState.Initializing;
        if (s_startedInstanceCount++ == 0)
            MGP.Accelerometer_Start();
    }

    internal void PlatformStop()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().Name);

        if (_started && --s_startedInstanceCount == 0)
            MGP.Accelerometer_Stop();

        _started = false;
        _state = SensorState.Disabled;
    }

    internal void PlatformDispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            if (_started)
                PlatformStop();

            s_instances.Remove(this);
        }
    }

    internal static void PlatformUpdate()
    {
        if (s_instances.Count == 0)
            return;

        SensorState state = (SensorState)MGP.Accelerometer_GetState();
        for (int index = 0; index < s_instances.Count; index++)
        {
            Accelerometer accelerometer = s_instances[index];
            if (accelerometer._started)
                accelerometer._state = state;
        }

        if (state != SensorState.Ready
            || MGP.Accelerometer_GetReading(out float x, out float y, out float z, out int sequence) == 0
            || sequence == s_lastReadingSequence)
        {
            return;
        }

        s_lastReadingSequence = sequence;
        AccelerometerReading reading = new AccelerometerReading
        {
            Acceleration = new Vector3(x, y, z),
            Timestamp = DateTimeOffset.UtcNow
        };

        for (int index = 0; index < s_instances.Count; index++)
        {
            Accelerometer accelerometer = s_instances[index];
            if (!accelerometer._started)
                continue;

            accelerometer.IsDataValid = true;
            accelerometer.CurrentValue = reading;
        }
    }
}
