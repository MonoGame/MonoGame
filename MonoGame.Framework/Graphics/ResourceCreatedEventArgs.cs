using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides data for the <see cref="GraphicsDevice.ResourceCreated"/> event. This class cannot be inherited.
    /// </summary>
    public sealed class ResourceCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The newly created resource object.
        /// </summary>
        public Object Resource { get; internal set; }
    }
}
