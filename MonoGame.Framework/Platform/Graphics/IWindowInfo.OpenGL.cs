// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.OpenGL
{
    /// <summary>
    /// Represents an interface for retrieving window information.
    /// </summary>
    public interface IWindowInfo
    {
        /// <summary>
        /// Gets the handle of the window.
        /// </summary>
        IntPtr Handle { get; }
    }
}
