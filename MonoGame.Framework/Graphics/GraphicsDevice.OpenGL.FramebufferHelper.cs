// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MonoGame.OpenGL;

using System.Security;

namespace Microsoft.Xna.Framework.Graphics
{
    // ARB_framebuffer_object implementation
    partial class GraphicsDevice
    {
        internal class FramebufferHelper
        {
            private static FramebufferHelper _instance;

            public static FramebufferHelper Create(GraphicsDevice gd)
            {
                if (gd.GraphicsCapabilities.SupportsFramebufferObjectARB)
                {
                    _instance = new FramebufferHelper(gd);
                }
#if !(GLES)
                else if (gd.GraphicsCapabilities.SupportsFramebufferObjectEXT)
                {
                    _instance = new FramebufferHelperEXT(gd);
                }
#endif
                else
                {
                    throw new PlatformNotSupportedException(
                        "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                        "Try updating your graphics drivers.");
                }

                return _instance;
            }

            public static FramebufferHelper Get()
            {
                if (_instance == null)
                    throw new InvalidOperationException("The FramebufferHelper has not been created yet!");
                return _instance;
            }

            public bool SupportsInvalidateFramebuffer { get; private set; }

            public bool SupportsBlitFramebuffer { get; private set; }

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
                GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, (RenderbufferStorage)internalFormat, width, height);
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
                GL.GenerateMipmap((GenerateMipmapTarget)target);
                GraphicsExtensions.CheckGLError();

            }

            internal virtual void BlitFramebuffer(int iColorAttachment, int width, int height)
            {

                GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + iColorAttachment);
                GraphicsExtensions.CheckGLError();
                GL.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
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
    }
}
