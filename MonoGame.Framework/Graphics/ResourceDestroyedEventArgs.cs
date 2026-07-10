using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides data for the <see cref="GraphicsDevice.ResourceDestroyed"/> event. This class cannot be inherited.
    /// </summary>
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
