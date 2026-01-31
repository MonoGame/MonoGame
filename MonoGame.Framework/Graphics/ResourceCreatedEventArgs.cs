// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#nullable enable

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
        public Object? Resource { get; internal set; }
    }
}
