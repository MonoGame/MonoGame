// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#nullable enable

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
        public string? Name { get; internal set; }

        /// <summary>
        /// The resource manager tag of the destroyed resource.
        /// </summary>
        public Object? Tag { get; internal set; }
    }
}
