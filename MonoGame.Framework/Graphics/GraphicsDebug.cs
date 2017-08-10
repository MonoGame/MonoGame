// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDebug : IDisposable
    {
        private readonly GraphicsDevice _device;
        private bool _isDisposed = false;

        /// <summary>
        /// Constructs a new instance of <see cref="GraphicsDebug"/>, which provides debugging APIs
        /// for the underlying graphics hardware.
        /// </summary>
        /// <param name="device">The associated graphics device.</param>
        public GraphicsDebug(GraphicsDevice device)
        {
            _device = device;

            PlatformConstruct();
        }

        /// <summary>
        /// Whether this instance has already had <see cref="Dispose()"/> called on it.
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        /// Attempt to dequeue a debugging message from the graphics subsystem.
        /// </summary>
        /// <remarks>
        /// When running on a graphics device with debugging enabled, this allows you to retrieve
        /// subsystem-specific (e.g. DirectX, OpenGL, etc.) debugging messages including information
        /// about improper usage of shaders and APIs.
        /// </remarks>
        /// <param name="message">The graphics debugging message if retrieved, null otherwise.</param>
        /// <returns>True if a graphics debugging message was retrieved, false otherwise.</returns>
        public bool TryDequeueMessage(out GraphicsDebugMessage message)
        {
            return PlatformTryDequeueMessage(out message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    PlatformDispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes the resources associated with this <see cref="GraphicsDebug"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
