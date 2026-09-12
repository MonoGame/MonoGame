// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Utilities;

public static partial class PlatformConfiguration
{
    /// <summary>
    /// Configuration keys for native platform-specific engine parameters.
    /// </summary>
    /// <remarks>
    /// No configuration keys are currently available for OpenGL.
    /// </remarks>
    public enum PlatformConfigKey
    {
    }

    /// <summary>
    /// Sets an <see cref="int"/> configuration value.
    /// <remarks>No-op on OpenGL.</remarks>
    /// </summary>
    public static void Set(PlatformConfigKey key, int value)
    {
    }

    /// <summary>
    /// Sets a <see cref="string"/> configuration value.
    /// <remarks>No-op on OpenGL.</remarks>
    /// </summary>
    public static void Set(PlatformConfigKey key, string value)
    {
    }
}
