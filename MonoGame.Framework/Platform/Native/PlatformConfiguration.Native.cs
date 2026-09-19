// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace MonoGame.Framework;

public static partial class PlatformConfiguration
{
    /// <summary>
    /// Sets an <see cref="int"/> configuration value.
    /// </summary>
    public static void Set(PlatformConfigKey key, int value)
    {
        MGC.Config_SetInt(key, value);
    }

    /// <summary>
    /// Sets a <see cref="string"/> configuration value.
    /// </summary>
    public static void Set(PlatformConfigKey key, string value)
    {
        MGC.Config_SetString(key, value);
    }
}
