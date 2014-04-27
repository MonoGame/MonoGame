// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC
using MonoMac.OpenGL;
using GLStencilFunction = MonoMac.OpenGL.StencilFunction;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
using GLStencilFunction = OpenTK.Graphics.OpenGL.StencilFunction;
#elif GLES
using OpenTK.Graphics.ES20;
using EnableCap = OpenTK.Graphics.ES20.All;
using GLStencilFunction = OpenTK.Graphics.ES20.All;
using StencilOp = OpenTK.Graphics.ES20.All;
using DepthFunction = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class DepthStencilState
    {
        internal void PlatformApplyState(GraphicsDevice device)
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

                DepthFunction func;
                switch (DepthBufferFunction)
                {
                    default:
                    case CompareFunction.Always:
                        func = DepthFunction.Always;
                        break;
                    case CompareFunction.Equal:
                        func = DepthFunction.Equal;
                        break;
                    case CompareFunction.Greater:
                        func = DepthFunction.Greater;
                        break;
                    case CompareFunction.GreaterEqual:
                        func = DepthFunction.Gequal;
                        break;
                    case CompareFunction.Less:
                        func = DepthFunction.Less;
                        break;
                    case CompareFunction.LessEqual:
                        func = DepthFunction.Lequal;
                        break;
                    case CompareFunction.Never:
                        func = DepthFunction.Never;
                        break;
                    case CompareFunction.NotEqual:
                        func = DepthFunction.Notequal;
                        break;
                }

                GL.DepthFunc(func);
                GraphicsExtensions.CheckGLError();
            }

            GL.DepthMask(DepthBufferWriteEnable);
            GraphicsExtensions.CheckGLError();

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

                // set function
                if (this.TwoSidedStencilMode)
                {
#if GLES
                    var cullFaceModeFront = (All)CullFaceMode.Front;
                    var cullFaceModeBack = (All)CullFaceMode.Back;
                    var stencilFaceFront = (All)CullFaceMode.Front;
                    var stencilFaceBack = (All)CullFaceMode.Back;
#else
                    var cullFaceModeFront = (Version20)CullFaceMode.Front;
                    var cullFaceModeBack = (Version20)CullFaceMode.Back;
                    var stencilFaceFront = StencilFace.Front;
                    var stencilFaceBack = StencilFace.Back;
#endif

                    GL.StencilFuncSeparate(cullFaceModeFront, GetStencilFunc(this.StencilFunction), 
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    GL.StencilFuncSeparate(cullFaceModeBack, GetStencilFunc(this.CounterClockwiseStencilFunction), 
                                           this.ReferenceStencil, this.StencilMask);
                    GraphicsExtensions.CheckGLError();
                    GL.StencilOpSeparate(stencilFaceFront, GetStencilOp(this.StencilFail), 
                                         GetStencilOp(this.StencilDepthBufferFail), 
                                         GetStencilOp(this.StencilPass));
                    GraphicsExtensions.CheckGLError();
                    GL.StencilOpSeparate(stencilFaceBack, GetStencilOp(this.CounterClockwiseStencilFail), 
                                         GetStencilOp(this.CounterClockwiseStencilDepthBufferFail), 
                                         GetStencilOp(this.CounterClockwiseStencilPass));
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    GL.StencilFunc(GetStencilFunc(this.StencilFunction), ReferenceStencil, StencilMask);
                    GraphicsExtensions.CheckGLError();
                    
                    GL.StencilOp(GetStencilOp(StencilFail),
                                 GetStencilOp(StencilDepthBufferFail),
                                 GetStencilOp(StencilPass));
                    GraphicsExtensions.CheckGLError();
                }

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

