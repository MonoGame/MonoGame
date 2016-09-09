// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif
#elif DESKTOPGL
using OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState
    {
        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            // When rendering offscreen the faces change order.
            var offscreen = device.IsRenderTargetBound;

            if (force)
            {
                // Turn off dithering to make sure data returned by Texture.GetData is accurate
                GL.Disable(EnableCap.Dither);
            }

            if (CullMode == CullMode.None)
            {
                if (force || device._lastCullMode != CullMode)
                {
                    GL.Disable(EnableCap.CullFace);
                    GraphicsExtensions.CheckGLError();

                    device._lastCullMode = CullMode;
                }
            }
            else
            {
                if (force || device._lastCullMode != CullMode)
                {
                    GL.Enable(EnableCap.CullFace);
                    GraphicsExtensions.CheckGLError();

                    device._lastCullMode = CullMode;
                }

                if (force || device._lastCullFaceMode != CullFaceMode.Back)
                {
                    GL.CullFace(CullFaceMode.Back);
                    GraphicsExtensions.CheckGLError();

                    device._lastCullFaceMode = CullFaceMode.Back;
                }

                FrontFaceDirection dir;
                if (CullMode == CullMode.CullClockwiseFace)
                {
                    if (offscreen)
                        dir = FrontFaceDirection.Cw;
                    else
                        dir = FrontFaceDirection.Ccw;
                }
                else
                {
                    if (offscreen)
                        dir = FrontFaceDirection.Ccw;
                    else
                        dir = FrontFaceDirection.Cw;
                }

                if (force || device._lastCullDirection != dir)
                {
                    GL.FrontFace(dir);
                    GraphicsExtensions.CheckGLError();

                    device._lastCullDirection = dir;
                }
            }

#if MONOMAC || WINDOWS || DESKTOPGL
			if (FillMode == FillMode.Solid) 
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            else
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
#else
            if (FillMode != FillMode.Solid)
                throw new NotImplementedException();
#endif

            if (force || this.ScissorTestEnable != device._lastRasterizerState.ScissorTestEnable)
			{
			    if (ScissorTestEnable)
				    GL.Enable(EnableCap.ScissorTest);
			    else
				    GL.Disable(EnableCap.ScissorTest);
                GraphicsExtensions.CheckGLError();
                device._lastRasterizerState.ScissorTestEnable = this.ScissorTestEnable;
            }

            if (force || 
                this.DepthBias != device._lastRasterizerState.DepthBias ||
                this.SlopeScaleDepthBias != device._lastRasterizerState.SlopeScaleDepthBias)
            {
                if (this.DepthBias != 0 || this.SlopeScaleDepthBias != 0)
                {   
                    GL.Enable(EnableCap.PolygonOffsetFill);
                    GL.PolygonOffset(this.SlopeScaleDepthBias, this.DepthBias);
                }
                else
                    GL.Disable(EnableCap.PolygonOffsetFill);
                GraphicsExtensions.CheckGLError();
                device._lastRasterizerState.DepthBias = this.DepthBias;
                device._lastRasterizerState.SlopeScaleDepthBias = this.SlopeScaleDepthBias;
            }

            if (device.GraphicsCapabilities.SupportsDepthClamp &&
                (force || this.DepthClipEnable != device._lastRasterizerState.DepthClipEnable))
            {
                if (!DepthClipEnable)
                    GL.Enable((EnableCap) 0x864F); // should be EnableCap.DepthClamp, but not available in OpenTK.Graphics.ES20.EnableCap
                else
                    GL.Disable((EnableCap) 0x864F);
                GraphicsExtensions.CheckGLError();
                device._lastRasterizerState.DepthClipEnable = this.DepthClipEnable;
            }

            // TODO: Implement MultiSampleAntiAlias
        }
    }
}
