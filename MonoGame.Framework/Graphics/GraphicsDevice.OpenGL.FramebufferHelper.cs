// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac;
using MonoMac.OpenGL;
#endif

#if (WINDOWS || LINUX) && !GLES
using OpenTK.Graphics.OpenGL;

#endif

#if GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
#if GLES
        internal class FramebufferHelper
        {
            public bool SupportsInvalidateFramebuffer { get; private set; }

            public bool SupportsBlitFramebuffer { get; private set; }
#if IOS
            internal const string OpenGLLibrary = MonoTouch.Constants.OpenGLESLibrary;
#elif ANDROID
            internal const string OpenGLLibrary = "libGLESv2.dll";
#endif

            #region GL_EXT_discard_framebuffer

            internal const All AllColorExt = (All)0x1800;
            internal const All AllDepthExt = (All)0x1801;
            internal const All AllStencilExt = (All)0x1802;

            [DllImport(OpenGLLibrary, EntryPoint = "glDiscardFramebufferEXT")]
            internal extern static void GLDiscardFramebufferExt(All target, int numAttachments, [MarshalAs(UnmanagedType.LPArray)] All[] attachments);

            #endregion

            #region GL_APPLE_framebuffer_multisample

            internal const All AllFramebufferIncompleteMultisampleApple = (All)0x8D56;
            internal const All AllMaxSamplesApple = (All)0x8D57;
            internal const All AllReadFramebufferApple = (All)0x8CA8;
            internal const All AllDrawFramebufferApple = (All)0x8CA9;
            internal const All AllRenderBufferSamplesApple = (All)0x8CAB;

            [DllImport(OpenGLLibrary, EntryPoint = "glRenderbufferStorageMultisampleAPPLE")]
            internal extern static void GLRenderbufferStorageMultisampleApple(All target, int samples, All internalformat, int width, int height);

            [DllImport(OpenGLLibrary, EntryPoint = "glResolveMultisampleFramebufferAPPLE")]
            internal extern static void GLResolveMultisampleFramebufferApple();

            internal void GLBlitFramebufferApple(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, TextureMagFilter filter)
            {
                GLResolveMultisampleFramebufferApple();
            }

            #endregion

            #region GL_IMG_multisampled_render_to_texture

            internal const All AllFramebufferIncompleteMultisampleImg = (All)0x9134;
            internal const All AllMaxSamplesImg = (All)0x9135;

            [DllImport(OpenGLLibrary, EntryPoint = "glRenderbufferStorageMultisampleIMG")]
            internal extern static void GLRenderbufferStorageMultisampleImg(All target, int samples, All internalformat, int width, int height);

            [DllImport(OpenGLLibrary, EntryPoint = "glFramebufferTexture2DMultisampleIMG")]
            internal extern static void GLFramebufferTexture2DMultisampleImg(All target, All attachment, All textarget, int texture, int level, int samples);

            #endregion

            #region GL_EXT_multisampled_render_to_texture

            internal const All AllFramebufferIncompleteMultisampleExt = (All)0x8D56;
            internal const All AllMaxSamplesExt = (All)0x8D57;

            [DllImport(OpenGLLibrary, EntryPoint = "glRenderbufferStorageMultisampleEXT")]
            internal extern static void GLRenderbufferStorageMultisampleExt(All target, int samples, All internalformat, int width, int height);

            [DllImport(OpenGLLibrary, EntryPoint = "glFramebufferTexture2DMultisampleEXT")]
            internal extern static void GLFramebufferTexture2DMultisampleExt(All target, All attachment, All textarget, int texture, int level, int samples);

            #endregion

            internal delegate void GLRenderbufferStorageMultisampleDelegate(All target, int samples, All internalFormat, int width, int height);
            internal delegate void GLBlitFramebufferDelegate(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, TextureMagFilter filter);
            internal delegate void GLFramebufferTexture2DMultisampleDelegate(All target, All attachment, All textarget, int texture, int level, int samples);

            internal GLRenderbufferStorageMultisampleDelegate GLRenderbufferStorageMultisample;
            internal GLFramebufferTexture2DMultisampleDelegate GLFramebufferTexture2DMultisample;
            internal GLBlitFramebufferDelegate GLBlitFramebuffer;

            internal All AllReadFramebuffer = All.Framebuffer;
            internal All AllDrawFramebuffer = All.Framebuffer;

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
                if (graphicsDevice._extensions.Contains("GL_EXT_discard_framebuffer"))
                {
                    this.SupportsInvalidateFramebuffer = true;
                }

                if (graphicsDevice._extensions.Contains("GL_APPLE_framebuffer_multisample"))
                {
                    this.GLRenderbufferStorageMultisample = new GLRenderbufferStorageMultisampleDelegate(GLRenderbufferStorageMultisampleApple);
                    this.GLBlitFramebuffer = new GLBlitFramebufferDelegate(GLBlitFramebufferApple);
                    this.AllReadFramebuffer = AllReadFramebufferApple;
                    this.AllDrawFramebuffer = AllDrawFramebufferApple;
                    
                }
                if (graphicsDevice._extensions.Contains("GL_EXT_multisampled_render_to_texture"))
                {
                    this.GLRenderbufferStorageMultisample = new GLRenderbufferStorageMultisampleDelegate(GLRenderbufferStorageMultisampleExt);
                    this.GLFramebufferTexture2DMultisample = new GLFramebufferTexture2DMultisampleDelegate(GLFramebufferTexture2DMultisampleExt);
                }
                else if (graphicsDevice._extensions.Contains("GL_IMG_multisampled_render_to_texture"))
                {
                    this.GLRenderbufferStorageMultisample = new GLRenderbufferStorageMultisampleDelegate(GLRenderbufferStorageMultisampleImg);
                    this.GLFramebufferTexture2DMultisample = new GLFramebufferTexture2DMultisampleDelegate(GLFramebufferTexture2DMultisampleImg);
                }

                this.SupportsBlitFramebuffer = this.GLBlitFramebuffer != null;
            }

            internal virtual void GenRenderbuffer(out int renderbuffer)
            {
                renderbuffer = 0;
                GL.GenRenderbuffers(1, ref renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindRenderbuffer(int renderbuffer)
            {
                GL.BindRenderbuffer(All.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void DeleteRenderbuffer(int renderbuffer)
            {
                GL.DeleteRenderbuffers(1, ref renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void RenderbufferStorageMultisample(int samples, int internalFormat, int width, int height)
            {
                if (samples > 0 && this.GLRenderbufferStorageMultisample != null)
                    GLRenderbufferStorageMultisample(All.Renderbuffer, samples, (All)internalFormat, width, height);
                else
                    GL.RenderbufferStorage(All.Renderbuffer, (All)internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenFramebuffer(out int framebuffer)
            {
                framebuffer = 0;
                GL.GenFramebuffers(1, ref framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindFramebuffer(int framebuffer)
            {
                GL.BindFramebuffer(All.Framebuffer, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindReadFramebuffer(int readFramebuffer)
            {
                GL.BindFramebuffer(AllReadFramebuffer, readFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal readonly All[] GLDiscardAttachementsDefault = { AllColorExt, AllDepthExt, AllStencilExt, };
            internal readonly All[] GLDiscardAttachements = { All.ColorAttachment0, All.DepthAttachment, All.StencilAttachment, };

            internal virtual void InvalidateDrawFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
                GLDiscardFramebufferExt(AllDrawFramebuffer, 3, GLDiscardAttachements);
            }

            internal virtual void InvalidateReadFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
                GLDiscardFramebufferExt(AllReadFramebuffer, 3, GLDiscardAttachements);
            }

            internal virtual void DeleteFramebuffer(int framebuffer)
            {
                GL.DeleteFramebuffers(1, ref framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferTexture2D(int attachement, int target, int texture, int level = 0, int samples = 0)
            {
                if (samples > 0 && this.GLFramebufferTexture2DMultisample != null)
                    this.GLFramebufferTexture2DMultisample(All.Framebuffer, (All)attachement, (All)target, texture, level, samples);
                else
                    GL.FramebufferTexture2D(All.Framebuffer, (All)attachement, (All)target, texture, level);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferRenderbuffer(int attachement, int renderbuffer, int level = 0)
            {
                GL.FramebufferRenderbuffer(All.Framebuffer, (All)attachement, All.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenerateMipmap(int target)
            {
                GL.GenerateMipmap((All)target);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BlitFramebuffer(int iColorAttachment, int width, int height)
            {
                this.GLBlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void CheckFramebufferStatus()
            {
                var status = GL.CheckFramebufferStatus(All.Framebuffer);
                if (status != All.FramebufferComplete)
                {
                    string message = "Framebuffer Incomplete.";
                    switch (status)
                    {
                        case All.FramebufferIncompleteAttachment: message = "Not all framebuffer attachment points are framebuffer attachment complete."; break;
                        case All.FramebufferIncompleteDimensions: message = "Not all attached images have the same width and height."; break;
                        case All.FramebufferIncompleteMissingAttachment: message = "No images are attached to the framebuffer."; break;
                        case All.FramebufferUnsupported: message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions."; break; 
                    }
                    throw new InvalidOperationException(message);
                }
            }
        }

#else
        internal class FramebufferHelper
        {
            public bool SupportsInvalidateFramebuffer { get; private set; }

            public bool SupportsBlitFramebuffer { get; private set; }

#if MONOMAC
			[DllImport(Constants.OpenGLLibrary, EntryPoint = "glRenderbufferStorageMultisampleEXT")]
		    internal extern static void GLRenderbufferStorageMultisampleExt(All target, int samples, All internalformat, int width, int height);

			[DllImport(Constants.OpenGLLibrary, EntryPoint = "glBlitFramebufferEXT")]
			internal extern static void GLBlitFramebufferExt(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, BlitFramebufferFilter filter);

			[DllImport(Constants.OpenGLLibrary, EntryPoint = "glGenerateMipmapEXT")]
			internal extern static void GLGenerateMipmapExt(GenerateMipmapTarget target);
#endif

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
                this.SupportsBlitFramebuffer = true;
                this.SupportsInvalidateFramebuffer = false;
            }

            internal virtual void GenRenderbuffer(out int renderbuffer)
            {
                GL.GenRenderbuffers(1, out renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindRenderbuffer(int renderbuffer)
            {
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void DeleteRenderbuffer(int renderbuffer)
            {
                GL.DeleteRenderbuffers(1, ref renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void RenderbufferStorageMultisample(int samples, int internalFormat, int width, int height)
            {
#if !MONOMAC
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, (RenderbufferStorage)internalFormat, width, height);
#else
				GLRenderbufferStorageMultisampleExt(All.Renderbuffer, samples, (All)internalFormat, width, height);
#endif
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenFramebuffer(out int framebuffer)
            {
                GL.GenFramebuffers(1, out framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindFramebuffer(int framebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindReadFramebuffer(int readFramebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, readFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void InvalidateDrawFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
            }

            internal virtual void InvalidateReadFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
            }

            internal virtual void DeleteFramebuffer(int framebuffer)
            {
                GL.DeleteFramebuffers(1, ref framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferTexture2D(int attachement, int target, int texture, int level = 0, int samples = 0)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachement, (TextureTarget)target, texture, level);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferRenderbuffer(int attachement, int renderbuffer, int level = 0)
            {
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferAttachment)attachement, RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenerateMipmap(int target)
            {
#if !MONOMAC
                GL.GenerateMipmap((GenerateMipmapTarget)target);
#else
				GLGenerateMipmapExt((GenerateMipmapTarget)target);
#endif
                GraphicsExtensions.CheckGLError();

            }

            internal virtual void BlitFramebuffer(int iColorAttachment, int width, int height)
            {

                GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
#if !MONOMAC
                GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
#else
				GLBlitFramebufferExt(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
#endif
                GraphicsExtensions.CheckGLError();

            }

            internal virtual void CheckFramebufferStatus()
            {
                var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                {
                    string message = "Framebuffer Incomplete.";
                    switch (status)
                    {
                        case FramebufferErrorCode.FramebufferIncompleteAttachment: message = "Not all framebuffer attachment points are framebuffer attachment complete."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMissingAttachment: message = "No images are attached to the framebuffer."; break;
                        case FramebufferErrorCode.FramebufferUnsupported: message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMultisample: message = "Not all attached images have the same number of samples."; break;
                    }
                    throw new InvalidOperationException(message);
                }
            }
        }

#if !MONOMAC
        internal sealed class FramebufferHelperEXT : FramebufferHelper
        {
            internal FramebufferHelperEXT(GraphicsDevice graphicsDevice)
                : base(graphicsDevice)
            {
            }

            internal override void GenRenderbuffer(out int id)
            {
                GL.Ext.GenRenderbuffers(1, out id);
                GraphicsExtensions.CheckGLError();
            }

            internal override void BindRenderbuffer(int id)
            {
                GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, id);
                GraphicsExtensions.CheckGLError();
            }

            internal override void DeleteRenderbuffer(int id)
            {
                GL.Ext.DeleteRenderbuffers(1, ref id);
                GraphicsExtensions.CheckGLError();
            }

            internal override void RenderbufferStorageMultisample(int samples, int internalFormat, int width, int height)
            {
                GL.Ext.RenderbufferStorageMultisample(RenderbufferTarget.RenderbufferExt, samples, (RenderbufferStorage)internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal override void GenFramebuffer(out int id)
            {
                GL.Ext.GenFramebuffers(1, out id);
                GraphicsExtensions.CheckGLError();
            }

            internal override void BindFramebuffer(int id)
            {
                GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, id);
                GraphicsExtensions.CheckGLError();
            }

            internal override void BindReadFramebuffer(int readFramebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, readFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal override void DeleteFramebuffer(int id)
            {
                GL.Ext.DeleteFramebuffers(1, ref id);
                GraphicsExtensions.CheckGLError();
            }

            internal override void FramebufferTexture2D(int attachement, int target, int texture, int level = 0, int samples = 0)
            {
                GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, (FramebufferAttachment)attachement, (TextureTarget)target, texture, level);
                GraphicsExtensions.CheckGLError();
            }

            internal override void FramebufferRenderbuffer(int attachement, int renderbuffer, int level = 0)
            {
                GL.Ext.FramebufferRenderbuffer(FramebufferTarget.FramebufferExt, (FramebufferAttachment)attachement, RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal override void GenerateMipmap(int target)
            {
                GL.Ext.GenerateMipmap((GenerateMipmapTarget)target);
                GraphicsExtensions.CheckGLError();
            }

            internal override void BlitFramebuffer(int iColorAttachment, int width, int height)
            {
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
                GL.Ext.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                GraphicsExtensions.CheckGLError();
            }

            internal override void CheckFramebufferStatus()
            {
                var status = GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt);
                if (status != FramebufferErrorCode.FramebufferComplete)
                {
                    string message = "Framebuffer Incomplete.";
                    switch (status)
                    {
                        case FramebufferErrorCode.FramebufferIncompleteAttachmentExt: message = "Not all framebuffer attachment points are framebuffer attachment complete."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt: message = "No images are attached to the framebuffer."; break;
                        case FramebufferErrorCode.FramebufferUnsupportedExt: message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMultisample: message = "Not all attached images have the same number of samples."; break;
                    }
                    throw new InvalidOperationException(message);
                }
            }
        }
#endif
#endif
    }
}
