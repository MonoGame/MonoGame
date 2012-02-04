namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Properties that change from how XNA works by default
    /// </summary>
    public static class AndroidCompatibility
    {
        static AndroidCompatibility()
        {
            DoCatchupUpdates = false;
            ScaleImageToPowerOf2 = true;
        }

        public static bool DoCatchupUpdates { get; set; }
        public static bool ScaleImageToPowerOf2 { get; set; }
    }
}