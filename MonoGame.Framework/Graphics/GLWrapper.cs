// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if OPENGL

using System;
using Microsoft.Xna.Framework.Graphics.GLWrappers;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Wrapper for ARB and EXT versions of OpenGL methods
    /// </summary>
    internal static class GLWrapper
    {
        /// <summary>
        /// Initialize the relevant wrappers based on what the OpenGL driver supports
        /// </summary>
        /// <param name="device"></param>
        internal static void Initialize(GraphicsDevice device)
        {
#if !GLES
            if (device._extensions.Contains("GL_ARB_framebuffer_object"))
                Fbo = new ArbFboWrapper();
            else if (device._extensions.Contains("GL_EXT_framebuffer_object"))
                Fbo = new ExtFboWrapper();
#else
            if (device._extensions.Contains("GL_OES_framebuffer_object"))
                Fbo = new ArbFboWrapper();
#endif
            else
                throw new NotSupportedException("Framebuffer objects (FBOs) are not supported by the current OpenGL drivers.");
        }

        /// <summary>
        /// Wrapper for ARB and EXT versions of OpenGL framebuffer object (FBO) methods
        /// </summary>
        internal static IFboWrapper Fbo { get; private set; }
    }
}

#endif
