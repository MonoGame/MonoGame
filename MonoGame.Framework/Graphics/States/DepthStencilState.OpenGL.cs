// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
using GLStencilFunction = MonoMac.OpenGL.StencilFunction;
#else
using OpenTK.Graphics.OpenGL;
using GLStencilFunction = OpenTK.Graphics.OpenGL.StencilFunction;
#endif
#elif DESKTOPGL
using OpenTK.Graphics.OpenGL;
using GLStencilFunction = OpenTK.Graphics.OpenGL.StencilFunction;
#elif GLES
using OpenTK.Graphics.ES20;
using GLStencilFunction = OpenTK.Graphics.ES20.StencilFunction;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        internal void PlatformApplyState(GraphicsDevice device, bool force = false)
        {
            if (force || this.DepthBufferEnable != device._lastDepthStencilState.DepthBufferEnable)
            {
                if (!DepthBufferEnable)
                {
                    GL.Disable(EnableCap.DepthTest);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Depth Buffer
                    GL.Enable(EnableCap.DepthTest);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastDepthStencilState.DepthBufferEnable = this.DepthBufferEnable;
            }

            if (force || this.DepthBufferFunction != device._lastDepthStencilState.DepthBufferFunction)
            {
                GL.DepthFunc(DepthBufferFunction.GetDepthFunction());
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferFunction = this.DepthBufferFunction;
            }

            if (force || this.DepthBufferWriteEnable != device._lastDepthStencilState.DepthBufferWriteEnable)
            {
                GL.DepthMask(DepthBufferWriteEnable);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.DepthBufferWriteEnable = this.DepthBufferWriteEnable;
            }

            if (force || this.StencilEnable != device._lastDepthStencilState.StencilEnable)
            {
                if (!StencilEnable)
                {
                    GL.Disable(EnableCap.StencilTest);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    // enable Stencil
                    GL.Enable(EnableCap.StencilTest);
                    GraphicsExtensions.CheckGLError();
                }
                device._lastDepthStencilState.StencilEnable = this.StencilEnable;
            }

            // set function
            if (this.TwoSidedStencilMode)
            {
#if GLES
                    var cullFaceModeFront = CullFaceMode.Front;
                    var cullFaceModeBack = CullFaceMode.Back;
                    var stencilFaceFront = CullFaceMode.Front;
                    var stencilFaceBack = CullFaceMode.Back;
#elif MONOMAC
                    var cullFaceModeFront = (Version20)CullFaceMode.Front;
                    var cullFaceModeBack = (Version20)CullFaceMode.Back;
                    var stencilFaceFront = StencilFace.Front;
                    var stencilFaceBack = StencilFace.Back;
#else
                var cullFaceModeFront = StencilFace.Front;
                var cullFaceModeBack = StencilFace.Back;
                var stencilFaceFront = StencilFace.Front;
                var stencilFaceBack = StencilFace.Back;
#endif

                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != device._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != device._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFuncSeparate(cullFaceModeFront, GetStencilFunc(this.StencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFunction != device._lastDepthStencilState.CounterClockwiseStencilFunction ||
                    this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
                    this.StencilMask != device._lastDepthStencilState.StencilMask)
			    {
                    GL.StencilFuncSeparate(cullFaceModeBack, GetStencilFunc(this.CounterClockwiseStencilFunction),
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.CounterClockwiseStencilFunction = this.CounterClockwiseStencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                
                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFail != device._lastDepthStencilState.StencilFail ||
					this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail ||
					this.StencilPass != device._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOpSeparate(stencilFaceFront, GetStencilOp(this.StencilFail),
                                         GetStencilOp(this.StencilDepthBufferFail),
                                         GetStencilOp(this.StencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFail = this.StencilFail;
                    device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    device._lastDepthStencilState.StencilPass = this.StencilPass;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.CounterClockwiseStencilFail != device._lastDepthStencilState.CounterClockwiseStencilFail ||
                    this.CounterClockwiseStencilDepthBufferFail != device._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail ||
                    this.CounterClockwiseStencilPass != device._lastDepthStencilState.CounterClockwiseStencilPass)
			    {
                    GL.StencilOpSeparate(stencilFaceBack, GetStencilOp(this.CounterClockwiseStencilFail),
                                         GetStencilOp(this.CounterClockwiseStencilDepthBufferFail),
                                         GetStencilOp(this.CounterClockwiseStencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.CounterClockwiseStencilFail = this.CounterClockwiseStencilFail;
                    device._lastDepthStencilState.CounterClockwiseStencilDepthBufferFail = this.CounterClockwiseStencilDepthBufferFail;
                    device._lastDepthStencilState.CounterClockwiseStencilPass = this.CounterClockwiseStencilPass;
                }
            }
            else
            {
                if (force ||
					this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
					this.StencilFunction != device._lastDepthStencilState.StencilFunction ||
					this.ReferenceStencil != device._lastDepthStencilState.ReferenceStencil ||
					this.StencilMask != device._lastDepthStencilState.StencilMask)
				{
                    GL.StencilFunc(GetStencilFunc(this.StencilFunction), ReferenceStencil, StencilMask);
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFunction = this.StencilFunction;
                    device._lastDepthStencilState.ReferenceStencil = this.ReferenceStencil;
                    device._lastDepthStencilState.StencilMask = this.StencilMask;
                }

                if (force ||
                    this.TwoSidedStencilMode != device._lastDepthStencilState.TwoSidedStencilMode ||
                    this.StencilFail != device._lastDepthStencilState.StencilFail ||
                    this.StencilDepthBufferFail != device._lastDepthStencilState.StencilDepthBufferFail ||
                    this.StencilPass != device._lastDepthStencilState.StencilPass)
                {
                    GL.StencilOp(GetStencilOp(StencilFail),
                                 GetStencilOp(StencilDepthBufferFail),
                                 GetStencilOp(StencilPass));
                    GraphicsExtensions.CheckGLError();
                    device._lastDepthStencilState.StencilFail = this.StencilFail;
                    device._lastDepthStencilState.StencilDepthBufferFail = this.StencilDepthBufferFail;
                    device._lastDepthStencilState.StencilPass = this.StencilPass;
                }
            }

            device._lastDepthStencilState.TwoSidedStencilMode = this.TwoSidedStencilMode;

            if (force || this.StencilWriteMask != device._lastDepthStencilState.StencilWriteMask)
            {
                GL.StencilMask(this.StencilWriteMask);
                GraphicsExtensions.CheckGLError();
                device._lastDepthStencilState.StencilWriteMask = this.StencilWriteMask;
            }
        }

        private static GLStencilFunction GetStencilFunc(CompareFunction function)
        {
            switch (function)
            {
                case CompareFunction.Always: 
                    return GLStencilFunction.Always;
                case CompareFunction.Equal:
                    return GLStencilFunction.Equal;
                case CompareFunction.Greater:
                    return GLStencilFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return GLStencilFunction.Gequal;
                case CompareFunction.Less:
                    return GLStencilFunction.Less;
                case CompareFunction.LessEqual:
                    return GLStencilFunction.Lequal;
                case CompareFunction.Never:
                    return GLStencilFunction.Never;
                case CompareFunction.NotEqual:
                    return GLStencilFunction.Notequal;
                default:
                    return GLStencilFunction.Always;
            }
        }

        private static StencilOp GetStencilOp(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Keep:
                    return StencilOp.Keep;
                case StencilOperation.Decrement:
                    return StencilOp.DecrWrap;
                case StencilOperation.DecrementSaturation:
                    return StencilOp.Decr;
                case StencilOperation.IncrementSaturation:
                    return StencilOp.Incr;
                case StencilOperation.Increment:
                    return StencilOp.IncrWrap;
                case StencilOperation.Invert:
                    return StencilOp.Invert;
                case StencilOperation.Replace:
                    return StencilOp.Replace;
                case StencilOperation.Zero:
                    return StencilOp.Zero;
                default:
                    return StencilOp.Keep;
            }
        }
    }
}

