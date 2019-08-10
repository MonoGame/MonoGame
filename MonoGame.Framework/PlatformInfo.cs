namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Type of the underlying game platform(this API is unique to MonoGame)
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
    /// Utility class that returns information about the underlying platform(this API is unique to MonoGame)
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
    }
}
