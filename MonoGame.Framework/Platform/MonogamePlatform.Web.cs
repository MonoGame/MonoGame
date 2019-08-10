namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Utility class that returns information about the underlying game platform((this API is unique to MonoGame)
    /// </summary>
    partial class MonogamePlatform
    {
        private static MonogamePlatformType PlatformGetCurrent()
        {
            return MonogamePlatformType.Web;
        }
    }
}
