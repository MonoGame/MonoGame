// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Utilities
{
    /// <summary>
    /// Utility class that returns information about the underlying platform
    /// </summary>
    public static class PlatformInfo
    {
        /// <summary>
        /// Underlying game platform type
        /// </summary>
        public static MonoGamePlatform MonoGamePlatform
        {
            get
            {
#if ANDROID
                return MonoGamePlatform.Android;
#elif DESKTOPGL
                return MonoGamePlatform.DesktopGL;
#elif IOS && !TVOS
                return MonoGamePlatform.iOS;
#elif TVOS
                return MonoGamePlatform.tvOS;
#elif WEB
                return MonoGamePlatform.WebGL;
#elif WINDOWS && DIRECTX
                return MonoGamePlatform.Windows;
#elif WINDOWS_UAP
                return MonoGamePlatform.WindowsUniversal;
#elif SWITCH
                return MonoGamePlatform.NintendoSwitch;
#elif XB1
                return MonoGamePlatform.XboxOne;
#elif PLAYSTATION4
                return MonoGamePlatform.PlayStation4;
#elif STADIA
                return MonoGamePlatform.Stadia;
#endif
            }
        }

        /// <summary>
        /// Graphics backend
        /// </summary>
        public static GraphicsBackend GraphicsBackend
        {
            get
            {
#if DIRECTX
                return GraphicsBackend.DirectX;
#else
                return GraphicsBackend.OpenGL;
#endif
            }
        }
    }
}
