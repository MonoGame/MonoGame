// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    partial class PlatformInfo
    {
#if ANDROID
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.Android;
        }
#endif

#if DESKTOPGL
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.DesktopGL;
        }
#endif

#if IOS && !TVOS
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.iOS;
        }
#endif

#if TVOS
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.tvOS;
        }
#endif

#if WEB
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.Web;
        }
#endif

#if WINDOWS && DIRECTX
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.Windows;
        }
#endif

#if WINDOWS_UAP
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.WindowsUniversal;
        }
#endif

#if DIRECTX
        private static GraphicsBackend PlatformGetGraphicsBackend()
        {
            return GraphicsBackend.DirectX;
        }
#else
        private static GraphicsBackend PlatformGetGraphicsBackend()
        {
            return GraphicsBackend.OpenGL;
        }
#endif
    }
}
