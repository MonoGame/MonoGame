// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if (OPENGL && !GLES)

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#endif

namespace Microsoft.Xna.Framework.Graphics.GLWrappers
{
    /// <summary>
    /// Wrapper implementation for EXT versions of OpenGL framebuffer object (FBO) methods
    /// </summary>
    internal class ExtFboWrapper : IFboWrapper
    {
        public bool IsRenderbuffer(int renderbuffer)
        {
            return GL.Ext.IsRenderbuffer(renderbuffer);
        }

        public bool IsRenderbuffer(uint renderbuffer)
        {
            return GL.Ext.IsRenderbuffer(renderbuffer);
        }

        public void BindRenderbuffer(RenderbufferTarget target, int renderbuffer)
        {
            GL.Ext.BindRenderbuffer(target, renderbuffer);
        }

        public void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer)
        {
            GL.Ext.BindRenderbuffer(target, renderbuffer);
        }

        public unsafe void DeleteRenderbuffers(int n, int* renderbuffers)
        {
            GL.Ext.DeleteRenderbuffers(n, renderbuffers);
        }

        public void DeleteRenderbuffers(int n, int[] renderbuffers)
        {
            GL.Ext.DeleteRenderbuffers(n, renderbuffers);
        }

        public void DeleteRenderbuffers(int n, ref int renderbuffers)
        {
            GL.Ext.DeleteRenderbuffers(n, ref renderbuffers);
        }

        public void DeleteRenderbuffers(int n, ref uint renderbuffers)
        {
            GL.Ext.DeleteRenderbuffers(n, ref renderbuffers);
        }

        public unsafe void DeleteRenderbuffers(int n, uint* renderbuffers)
        {
            GL.Ext.DeleteRenderbuffers(n, renderbuffers);
        }

        public void DeleteRenderbuffers(int n, uint[] renderbuffers)
        {
            GL.Ext.DeleteRenderbuffers(n, renderbuffers);
        }

        public unsafe void GenRenderbuffers(int n, int* renderbuffers)
        {
            GL.Ext.GenRenderbuffers(n, renderbuffers);
        }

        public void GenRenderbuffers(int n, int[] renderbuffers)
        {
            GL.Ext.GenRenderbuffers(n, renderbuffers);
        }

        public void GenRenderbuffers(int n, out int renderbuffers)
        {
            GL.Ext.GenRenderbuffers(n, out renderbuffers);
        }

        public void GenRenderbuffers(int n, out uint renderbuffers)
        {
            GL.Ext.GenRenderbuffers(n, out renderbuffers);
        }

        public unsafe void GenRenderbuffers(int n, uint* renderbuffers)
        {
            GL.Ext.GenRenderbuffers(n, renderbuffers);
        }

        public void GenRenderbuffers(int n, uint[] renderbuffers)
        {
            GL.Ext.GenRenderbuffers(n, renderbuffers);
        }

        public void RenderbufferStorage(RenderbufferTarget target, RenderbufferStorage internalformat, int width, int height)
        {
            GL.Ext.RenderbufferStorage(target, internalformat, width, height);
        }

        public unsafe void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, int* @params)
        {
            GL.Ext.GetRenderbufferParameter(target, pname, @params);
        }

        public void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, int[] @params)
        {
            GL.Ext.GetRenderbufferParameter(target, pname, @params);
        }

        public void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, out int @params)
        {
            GL.Ext.GetRenderbufferParameter(target, pname, out @params);
        }

        public bool IsFramebuffer(int framebuffer)
        {
            return GL.Ext.IsFramebuffer(framebuffer);
        }

        public bool IsFramebuffer(uint framebuffer)
        {
            return GL.Ext.IsFramebuffer(framebuffer);
        }

        public void BindFramebuffer(FramebufferTarget target, int framebuffer)
        {
            GL.Ext.BindFramebuffer(target, framebuffer);
        }

        public void BindFramebuffer(FramebufferTarget target, uint framebuffer)
        {
            GL.Ext.BindFramebuffer(target, framebuffer);
        }

        public unsafe void DeleteFramebuffers(int n, int* framebuffers)
        {
            GL.Ext.DeleteFramebuffers(n, framebuffers);
        }

        public void DeleteFramebuffers(int n, int[] framebuffers)
        {
            GL.Ext.DeleteFramebuffers(n, framebuffers);
        }

        public void DeleteFramebuffers(int n, ref int framebuffers)
        {
            GL.Ext.DeleteFramebuffers(n, ref framebuffers);
        }

        public void DeleteFramebuffers(int n, ref uint framebuffers)
        {
            GL.Ext.DeleteFramebuffers(n, ref framebuffers);
        }

        public unsafe void DeleteFramebuffers(int n, uint* framebuffers)
        {
            GL.Ext.DeleteFramebuffers(n, framebuffers);
        }

        public void DeleteFramebuffers(int n, uint[] framebuffers)
        {
            GL.Ext.DeleteFramebuffers(n, framebuffers);
        }

        public unsafe void GenFramebuffers(int n, int* framebuffers)
        {
            GL.Ext.GenFramebuffers(n, framebuffers);
        }

        public void GenFramebuffers(int n, int[] framebuffers)
        {
            GL.Ext.GenFramebuffers(n, framebuffers);
        }

        public void GenFramebuffers(int n, out int framebuffers)
        {
            GL.Ext.GenFramebuffers(n, out framebuffers);
        }

        public void GenFramebuffers(int n, out uint framebuffers)
        {
            GL.Ext.GenFramebuffers(n, out framebuffers);
        }

        public unsafe void GenFramebuffers(int n, uint* framebuffers)
        {
            GL.Ext.GenFramebuffers(n, framebuffers);
        }

        public void GenFramebuffers(int n, uint[] framebuffers)
        {
            GL.Ext.GenFramebuffers(n, framebuffers);
        }

        public FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget target)
        {
            return GL.Ext.CheckFramebufferStatus(target);
        }

        public void FramebufferTexture1D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, int texture, int level)
        {
            GL.Ext.FramebufferTexture1D(target, attachment, textarget, texture, level);
        }

        public void FramebufferTexture1D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level)
        {
            GL.Ext.FramebufferTexture1D(target, attachment, textarget, texture, level);
        }

        public void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, int texture, int level)
        {
            GL.Ext.FramebufferTexture2D(target, attachment, textarget, texture, level);
        }

        public void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level)
        {
            GL.Ext.FramebufferTexture2D(target, attachment, textarget, texture, level);
        }

        public void FramebufferTexture3D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, int texture, int level, int zoffset)
        {
            GL.Ext.FramebufferTexture3D(target, attachment, textarget, texture, level, zoffset);
        }

        public void FramebufferTexture3D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level, int zoffset)
        {
            GL.Ext.FramebufferTexture3D(target, attachment, textarget, texture, level, zoffset);
        }

        public void FramebufferRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, int renderbuffer)
        {
            GL.Ext.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
        }

        public void FramebufferRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, uint renderbuffer)
        {
            GL.Ext.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
        }

        public unsafe void GetFramebufferAttachmentParameter(FramebufferTarget target, FramebufferAttachment attachment, FramebufferParameterName pname, int* @params)
        {
            GL.Ext.GetFramebufferAttachmentParameter(target, attachment, pname, @params);
        }

        public void GetFramebufferAttachmentParameter(FramebufferTarget target, FramebufferAttachment attachment, FramebufferParameterName pname, int[] @params)
        {
            GL.Ext.GetFramebufferAttachmentParameter(target, attachment, pname, @params);
        }

        public void GetFramebufferAttachmentParameter(FramebufferTarget target, FramebufferAttachment attachment, FramebufferParameterName pname, out int @params)
        {
            GL.Ext.GetFramebufferAttachmentParameter(target, attachment, pname, out @params);
        }

        public void GenerateMipmap(GenerateMipmapTarget target)
        {
            GL.Ext.GenerateMipmap(target);
        }
    }
}

#endif
