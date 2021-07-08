using System.IO;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Resolver for content outside content manager
    /// </summary>
    public abstract class ContentResolver
    {
        /// <summary>
        /// Resolve stream from another place
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public abstract Stream Resolve(string contentPath);
    }
}
