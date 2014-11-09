// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using EnableCap = OpenTK.Graphics.ES20.All;
using FrontFaceDirection = OpenTK.Graphics.ES20.All;
using CullFaceMode = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState
    {
        internal void PlatformApplyState(GraphicsDevice device)
        {
            // When rendering offscreen the faces change order.
            var offscreen = device.IsRenderTargetBound;

            // Turn off dithering to make sure data returned by Texture.GetData is accurate
            GL.Disable(EnableCap.Dither);

            if (CullMode == CullMode.None)
            {
                GL.Disable(EnableCap.CullFace);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                GraphicsExtensions.CheckGLError();
                GL.CullFace(CullFaceMode.Back);
                GraphicsExtensions.CheckGLError();

                if (CullMode == CullMode.CullClockwiseFace)
                {
                    if (offscreen)
                        GL.FrontFace(FrontFaceDirection.Cw);
                    else
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    if (offscreen)
                        GL.FrontFace(FrontFaceDirection.Ccw);
                    else
                        GL.FrontFace(FrontFaceDirection.Cw);
                    GraphicsExtensions.CheckGLError();
                }
            }

#if MONOMAC || WINDOWS || LINUX
			if (FillMode == FillMode.Solid) 
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            else
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
#else
            if (FillMode != FillMode.Solid)
                throw new NotImplementedException();
#endif

			if (ScissorTestEnable)
				GL.Enable(EnableCap.ScissorTest);
			else
				GL.Disable(EnableCap.ScissorTest);
            GraphicsExtensions.CheckGLError();

            if (this.DepthBias != 0 || this.SlopeScaleDepthBias != 0)
            {   
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(this.SlopeScaleDepthBias, this.DepthBias);
            }
            else
                GL.Disable(EnableCap.PolygonOffsetFill);
            GraphicsExtensions.CheckGLError();

            // TODO: Implement MultiSampleAntiAlias
        }
    }
}
