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
#if PLATFORM_MACOS_LEGACY
using MonoMac;
using MonoMac.OpenGL;
#else
using ObjCRuntime;
using OpenTK.Graphics.OpenGL;
#endif
#endif

#if (WINDOWS || DESKTOPGL) && !GLES
using OpenTK.Graphics.OpenGL;

#endif

#if GLES
using OpenTK.Graphics.ES20;
using System.Security;
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
			internal const string OpenGLLibrary = ObjCRuntime.Constants.OpenGLESLibrary;
#elif ANDROID
            internal const string OpenGLLibrary = "libGLESv2.dll";
            [DllImport("libEGL.dll", EntryPoint = "eglGetProcAddress")]
            public static extern IntPtr EGLGetProcAddress(string funcname);
#endif
            #region GL_EXT_discard_framebuffer

            internal const All AllColorExt = (All)0x1800;
            internal const All AllDepthExt = (All)0x1801;
            internal const All AllStencilExt = (All)0x1802;

            [SuppressUnmanagedCodeSecurity]
            [DllImport(OpenGLLibrary, EntryPoint = "glDiscardFramebufferEXT", ExactSpelling = true)]
            internal extern static void GLDiscardFramebufferExt(All target, int numAttachments, [MarshalAs(UnmanagedType.LPArray)] All[] attachments);

            #endregion

            #region GL_APPLE_framebuffer_multisample

            internal const All AllFramebufferIncompleteMultisampleApple = (All)0x8D56;
            internal const All AllMaxSamplesApple = (All)0x8D57;
            internal const All AllReadFramebufferApple = (All)0x8CA8;
            internal const All AllDrawFramebufferApple = (All)0x8CA9;
            internal const All AllRenderBufferSamplesApple = (All)0x8CAB;

            [SuppressUnmanagedCodeSecurity]
            [DllImport(OpenGLLibrary, EntryPoint = "glRenderbufferStorageMultisampleAPPLE", ExactSpelling = true)]
            internal extern static void GLRenderbufferStorageMultisampleApple(All target, int samples, All internalformat, int width, int height);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(OpenGLLibrary, EntryPoint = "glResolveMultisampleFramebufferAPPLE", ExactSpelling = true)]
            internal extern static void GLResolveMultisampleFramebufferApple();

            internal void GLBlitFramebufferApple(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, TextureMagFilter filter)
            {
                GLResolveMultisampleFramebufferApple();
            }

            #endregion

            #region GL_NV_framebuffer_multisample

            internal const All AllFramebufferIncompleteMultisampleNV = (All)0x8D56;
            internal const All AllMaxSamplesNV = (All)0x8D57;
            internal const All AllReadFramebufferNV = (All)0x8CA8;
            internal const All AllDrawFramebufferNV = (All)0x8CA9;
            internal const All AllRenderBufferSamplesNV = (All)0x8CAB;

            #endregion

            #region GL_IMG_multisampled_render_to_texture

            internal const All AllFramebufferIncompleteMultisampleImg = (All)0x9134;
            internal const All AllMaxSamplesImg = (All)0x9135;

            #endregion

            #region GL_EXT_multisampled_render_to_texture

            internal const All AllFramebufferIncompleteMultisampleExt = (All)0x8D56;
            internal const All AllMaxSamplesExt = (All)0x8D57;

            #endregion

            internal delegate void GLInvalidateFramebufferDelegate(All target, int numAttachments, All[] attachments);
            internal delegate void GLRenderbufferStorageMultisampleDelegate(All target, int samples, All internalFormat, int width, int height);
            internal delegate void GLBlitFramebufferDelegate(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearBufferMask mask, TextureMagFilter filter);
            internal delegate void GLFramebufferTexture2DMultisampleDelegate(All target, All attachment, All textarget, int texture, int level, int samples);

            internal GLInvalidateFramebufferDelegate GLInvalidateFramebuffer;
            internal GLRenderbufferStorageMultisampleDelegate GLRenderbufferStorageMultisample;
            internal GLFramebufferTexture2DMultisampleDelegate GLFramebufferTexture2DMultisample;
            internal GLBlitFramebufferDelegate GLBlitFramebuffer;

            internal All AllReadFramebuffer = All.Framebuffer;
            internal All AllDrawFramebuffer = All.Framebuffer;

            internal FramebufferHelper(GraphicsDevice graphicsDevice)
            {
#if IOS
                if (graphicsDevice._extensions.Contains("GL_EXT_discard_framebuffer"))
                {
                    this.GLInvalidateFramebuffer = new GLInvalidateFramebufferDelegate(GLDiscardFramebufferExt);
                    this.SupportsInvalidateFramebuffer = true;
                }

                if (graphicsDevice._extensions.Contains("GL_APPLE_framebuffer_multisample"))
                {
                    this.GLRenderbufferStorageMultisample = new GLRenderbufferStorageMultisampleDelegate(GLRenderbufferStorageMultisampleApple);
                    this.GLBlitFramebuffer = new GLBlitFramebufferDelegate(GLBlitFramebufferApple);
                    this.AllReadFramebuffer = AllReadFramebufferApple;
                    this.AllDrawFramebuffer = AllDrawFramebufferApple;
                }
#elif ANDROID
                // eglGetProcAddress doesn't guarantied returning NULL if the entry point doesn't exist. The returned address *should* be the same for all invalid entry point
                var invalidFuncPtr = EGLGetProcAddress("InvalidFunctionName");

                if (graphicsDevice._extensions.Contains("GL_EXT_discard_framebuffer"))
                {
                    var glDiscardFramebufferEXTPtr = EGLGetProcAddress("glDiscardFramebufferEXT");
                    if (glDiscardFramebufferEXTPtr != invalidFuncPtr)
                    {
                        this.GLInvalidateFramebuffer = Marshal.GetDelegateForFunctionPointer<GLInvalidateFramebufferDelegate>(glDiscardFramebufferEXTPtr);
                        this.SupportsInvalidateFramebuffer = true;
                    }
                }
                if (graphicsDevice._extensions.Contains("GL_EXT_multisampled_render_to_texture"))
                {
                    var glRenderbufferStorageMultisampleEXTPtr = EGLGetProcAddress("glRenderbufferStorageMultisampleEXT");
                    var glFramebufferTexture2DMultisampleEXTPtr = EGLGetProcAddress("glFramebufferTexture2DMultisampleEXT");
                    if (glRenderbufferStorageMultisampleEXTPtr != invalidFuncPtr && glFramebufferTexture2DMultisampleEXTPtr != invalidFuncPtr)
                    {
                        this.GLRenderbufferStorageMultisample = Marshal.GetDelegateForFunctionPointer<GLRenderbufferStorageMultisampleDelegate>(glRenderbufferStorageMultisampleEXTPtr);
                        this.GLFramebufferTexture2DMultisample = Marshal.GetDelegateForFunctionPointer<GLFramebufferTexture2DMultisampleDelegate>(glFramebufferTexture2DMultisampleEXTPtr);
                    }
                }
                else if (graphicsDevice._extensions.Contains("GL_IMG_multisampled_render_to_texture"))
                {
                    var glRenderbufferStorageMultisampleIMGPtr = EGLGetProcAddress("glRenderbufferStorageMultisampleIMG");
                    var glFramebufferTexture2DMultisampleIMGPtr = EGLGetProcAddress("glFramebufferTexture2DMultisampleIMG");
                    if (glRenderbufferStorageMultisampleIMGPtr != invalidFuncPtr && glFramebufferTexture2DMultisampleIMGPtr != invalidFuncPtr)
                    {
                        this.GLRenderbufferStorageMultisample = Marshal.GetDelegateForFunctionPointer<GLRenderbufferStorageMultisampleDelegate>(glRenderbufferStorageMultisampleIMGPtr);
                        this.GLFramebufferTexture2DMultisample = Marshal.GetDelegateForFunctionPointer<GLFramebufferTexture2DMultisampleDelegate>(glFramebufferTexture2DMultisampleIMGPtr);
                    }
                }
                else if (graphicsDevice._extensions.Contains("GL_NV_framebuffer_multisample"))
                {
                    var glRenderbufferStorageMultisampleNVPtr = EGLGetProcAddress("glRenderbufferStorageMultisampleNV");
                    var glBlitFramebufferNVPtr = EGLGetProcAddress("glBlitFramebufferNV");
                    if (glRenderbufferStorageMultisampleNVPtr != invalidFuncPtr && glBlitFramebufferNVPtr != invalidFuncPtr)
                    {
                        this.GLRenderbufferStorageMultisample = Marshal.GetDelegateForFunctionPointer<GLRenderbufferStorageMultisampleDelegate>(glRenderbufferStorageMultisampleNVPtr);
                        this.GLBlitFramebuffer = Marshal.GetDelegateForFunctionPointer<GLBlitFramebufferDelegate>(glBlitFramebufferNVPtr);
                        this.AllReadFramebuffer = AllReadFramebufferNV;
                        this.AllDrawFramebuffer = AllDrawFramebufferNV;
                    }
                }
#endif

                this.SupportsBlitFramebuffer = this.GLBlitFramebuffer != null;
            }

            internal virtual void GenRenderbuffer(out int renderbuffer)
            {
                renderbuffer = 0;
#if (ANDROID || IOS)
                GL.GenRenderbuffers(1, out renderbuffer);
#else
                GL.GenRenderbuffers(1, ref renderbuffer);
#endif
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
                if (samples > 0 && this.GLRenderbufferStorageMultisample != null)
                    GLRenderbufferStorageMultisample(All.Renderbuffer, samples, (All)internalFormat, width, height);
                else
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferInternalFormat)internalFormat, width, height);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenFramebuffer(out int framebuffer)
            {
                framebuffer = 0;
#if (ANDROID || IOS)
                GL.GenFramebuffers(1, out framebuffer);
#else
                GL.GenFramebuffers(1, ref framebuffer);
#endif
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindFramebuffer(int framebuffer)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BindReadFramebuffer(int readFramebuffer)
            {
                GL.BindFramebuffer((FramebufferTarget)AllReadFramebuffer, readFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal readonly All[] GLDiscardAttachementsDefault = { AllColorExt, AllDepthExt, AllStencilExt, };
            internal readonly All[] GLDiscardAttachements = { All.ColorAttachment0, All.DepthAttachment, All.StencilAttachment, };

            internal virtual void InvalidateDrawFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
                this.GLInvalidateFramebuffer(AllDrawFramebuffer, 3, GLDiscardAttachements);
            }

            internal virtual void InvalidateReadFramebuffer()
            {
                Debug.Assert(this.SupportsInvalidateFramebuffer);
                this.GLInvalidateFramebuffer(AllReadFramebuffer, 3, GLDiscardAttachements);
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
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, (FramebufferSlot)attachement, (TextureTarget)target, texture, level);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void FramebufferRenderbuffer(int attachement, int renderbuffer, int level = 0)
            {
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, (FramebufferSlot)attachement, RenderbufferTarget.Renderbuffer, renderbuffer);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void GenerateMipmap(int target)
            {
                GL.GenerateMipmap((TextureTarget)target);
                GraphicsExtensions.CheckGLError();
            }

            internal virtual void BlitFramebuffer(int iColorAttachment, int width, int height)
            {
                this.GLBlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, TextureMagFilter.Nearest);
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
                        case FramebufferErrorCode.FramebufferIncompleteDimensions: message = "Not all attached images have the same width and height."; break;
                        case FramebufferErrorCode.FramebufferIncompleteMissingAttachment: message = "No images are attached to the framebuffer."; break;
                        case FramebufferErrorCode.FramebufferUnsupported: message = "The combination of internal formats of the attached images violates an implementation-dependent set of restrictions."; break; 
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
