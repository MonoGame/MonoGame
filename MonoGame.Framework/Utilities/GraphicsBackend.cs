// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Utilities
{
    /// <summary>
    /// Type of the underlying graphics backend.
    /// </summary>
    public enum GraphicsBackend
    {
        /// <summary>
        /// Represents the Microsoft DirectX graphics backend.
        /// </summary>
        DirectX,

        /// <summary>
        /// Represents the OpenGL graphics backend.
        /// </summary>
        OpenGL,

        /// <summary>
        /// Represents the Vulkan graphics backend.
        /// </summary>
        Vulkan,

        /// <summary>
        /// Represents the Apple Metal graphics backend.
        /// </summary>
        Metal
    }
}