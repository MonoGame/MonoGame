namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Type of the underlying game platform(this API is unique to MonoGame)
    /// </summary>
    public enum MonogamePlatformType
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
        PlayStation4
    }

    /// <summary>
    /// Utility class that returns information about the underlying game platform(this API is unique to MonoGame)
    /// </summary>
    public static partial class MonogamePlatform
    {
        public static MonogamePlatformType Current
        {
            get
            {
                return PlatformGetCurrent();
            }
        }
    }
}
