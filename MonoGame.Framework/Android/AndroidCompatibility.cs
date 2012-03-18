namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Properties that change from how XNA works by default
    /// </summary>
    public static class AndroidCompatibility
    {
        public enum ESVersions
        {
            v1_1,
            v2_0
        }

        static AndroidCompatibility()
        {
            ScaleImageToPowerOf2 = true;
            ESVersion = ESVersions.v2_0;
        }

        public static bool ScaleImageToPowerOf2 { get; set; }
        public static ESVersions ESVersion { get; set; }

    }
}