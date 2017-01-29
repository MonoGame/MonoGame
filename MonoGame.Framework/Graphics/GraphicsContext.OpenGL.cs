// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
using GLPrimitiveType = MonoMac.OpenGL.BeginMode;
#else
using OpenTK.Graphics.OpenGL;
using GLPrimitiveType = OpenTK.Graphics.OpenGL.BeginMode;
#endif
#endif

#if DESKTOPGL
using OpenGL;
#endif

#if ANGLE
using OpenTK.Graphics;
#endif

#if GLES
using OpenTK.Graphics.ES20;
using FramebufferAttachment = OpenTK.Graphics.ES20.All;
using RenderbufferStorage = OpenTK.Graphics.ES20.All;
using GLPrimitiveType = OpenTK.Graphics.ES20.BeginMode;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsContext : IDisposable
    {
        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal BlendState _lastBlendState = new BlendState();
        internal bool _lastBlendEnable = false;

        public GraphicsContext(GraphicsDevice device)
        {
            Initialize(device);
        }
        

        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(_device, force);
            ApplyBlendFactor(force);
        }

        private void ApplyBlendFactor(bool force)
        {
            if (force || BlendFactor != _lastBlendState.BlendFactor)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _lastBlendState.BlendFactor = this.BlendFactor;
            }
        }
        #region Implement IDisposable
        private void PlatformDispose(bool disposing)
        {            
            if (disposing)
            {
                // Release managed objects
                // ...
            }

            // Release native objects
            // ...
        }
        #endregion
    }
}
