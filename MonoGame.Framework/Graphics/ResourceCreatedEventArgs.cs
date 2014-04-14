using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class ResourceCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// The newly created resource object.
        /// </summary>
        public Object Resource { get; internal set; }
    }
}
