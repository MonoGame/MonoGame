// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Utilities;

/// <summary>
/// Provides configuration for platform-specific engine parameters.
/// </summary>
/// <remarks>
/// <para>
/// The available <see cref="PlatformConfigKey"/> values vary by target platform.
/// Setting a key that is not relevant to the current graphics backend is a no-op, but should be avoided.
/// </para>
/// <para>
/// Settings should be applied in <c>Program.Main()</c> before creating the <see cref="Microsoft.Xna.Framework.Game"/> instance.
/// Native platforms read these values before or during device initialization.
/// </para>
/// </remarks>
public static partial class PlatformConfiguration
{
    /*******************************************/
    /* This is the contract for each platform: */
    /*******************************************/

    /*

    public enum PlatformConfigKey
    {
    }

    /// <summary>
    /// Sets an <see cref="int"/> configuration value.
    /// </summary>
    public static void Set(PlatformConfigKey key, int value)
    {
        MGG.Config_SetInt((int)key, value);
    }

    /// <summary>
    /// Sets a <see cref="string"/> configuration value.
    /// </summary>
    public static void Set(PlatformConfigKey key, string value)
    {
        MGG.Config_SetString((int)key, value);
    }

    */
}
