using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class ResourceDestroyedEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the destroyed resource.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The resource manager tag of the destroyed resource.
        /// </summary>
        public Object Tag { get; internal set; }
    }
}
