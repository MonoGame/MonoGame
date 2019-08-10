namespace Microsoft.Xna.Framework
{
#if ANDROID
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.Android;
        }
    }
#endif

#if DESKTOPGL
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.DesktopGL;
        }
    }
#endif

#if IOS && !TVOS
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.iOS;
        }
    }
#endif

#if TVOS
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.tvOS;
        }
    }
#endif

#if WEB
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.Web;
        }
    }
#endif

#if WINDOWS && DIRECTX
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.Windows;
        }
    }
#endif

#if WINDOWS_UAP
    partial class PlatformInfo
    {
        private static MonogamePlatform PlatformGetCurrent()
        {
            return MonogamePlatform.WindowsUniversal;
        }
    }
#endif
}
