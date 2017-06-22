// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDebug
    {
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
    }
}
