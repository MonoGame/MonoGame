// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Type of the underlying game platform
    /// </summary>
    public enum MonogamePlatform
    {
        Android,
        iOS,
        tvOS,
        DesktopGL,
        Windows,
        WindowsUniversal,
        Web,
        PSVita,
        XBoxOne,
        PlayStation4,
        Switch
    }

    /// <summary>
    /// Graphics backend
    /// </summary>
    public enum GraphicsBackend
    {
        DirectX,
        OpenGL
    }

    /// <summary>
    /// Utility class that returns information about the underlying platform
    /// </summary>
    public static partial class PlatformInfo
    {
        /// <summary>
        /// Underlying game platform type
        /// </summary>
        public static MonogamePlatform MonogamePlatform
        {
            get
            {
                return PlatformGetCurrent();
            }
        }

        /// <summary>
        /// Graphics backend
        /// </summary>
        public static GraphicsBackend GraphicsBackend
        {
            get
            {
                return PlatformGetGraphicsBackend();
            }
        }
    }
}
