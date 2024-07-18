// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Utilities
{
    /// <summary>
    /// Utility class that returns information about the underlying platform
    /// </summary>
    public static partial class PlatformInfo
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
#elif SWITCH
                return MonoGamePlatform.NintendoSwitch;
#elif XB1
                return MonoGamePlatform.XboxOne;
#elif PLAYSTATION4
                return MonoGamePlatform.PlayStation4;
#elif PLAYSTATION5
                return MonoGamePlatform.PlayStation5;
#elif STADIA
                return MonoGamePlatform.Stadia;
#else
                return PlatformGetMonoGamePlatform();
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
#elif OPENGL
                return GraphicsBackend.OpenGL;
#else
                return PlatformGetGraphicsBackend();
#endif
            }
        }
    }
}
