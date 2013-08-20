// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if OPENGL

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using RenderbufferStorage = OpenTK.Graphics.ES20.RenderbufferInternalFormat;
using FramebufferAttachment = OpenTK.Graphics.ES20.FramebufferSlot;
using GenerateMipmapTarget = OpenTK.Graphics.ES20.TextureTarget;
#endif

namespace Microsoft.Xna.Framework.Graphics.GLWrappers
{
    /// <summary>
    /// Wrapper interface for all OpenGL framebuffer object (FBO) methods supported by both ARB and EXT versions
    /// </summary>
    internal interface IFboWrapper
    {
        bool IsRenderbuffer(int renderbuffer);
        bool IsRenderbuffer(uint renderbuffer);

        void BindRenderbuffer(RenderbufferTarget target, int renderbuffer);
        void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer);

        unsafe void DeleteRenderbuffers(int n, int* renderbuffers);
        void DeleteRenderbuffers(int n, int[] renderbuffers);
        void DeleteRenderbuffers(int n, ref int renderbuffers);
        void DeleteRenderbuffers(int n, ref uint renderbuffers);
        unsafe void DeleteRenderbuffers(int n, uint* renderbuffers);
        void DeleteRenderbuffers(int n, uint[] renderbuffers);

        unsafe void GenRenderbuffers(int n, int* renderbuffers);
        void GenRenderbuffers(int n, int[] renderbuffers);
        void GenRenderbuffers(int n, out int renderbuffers);
        void GenRenderbuffers(int n, out uint renderbuffers);
        unsafe void GenRenderbuffers(int n, uint* renderbuffers);
        void GenRenderbuffers(int n, uint[] renderbuffers);

        void RenderbufferStorage(RenderbufferTarget target, RenderbufferStorage internalformat, int width, int height);

        unsafe void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, int* @params);
        void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, int[] @params);
        void GetRenderbufferParameter(RenderbufferTarget target, RenderbufferParameterName pname, out int @params);

        bool IsFramebuffer(int framebuffer);
        bool IsFramebuffer(uint framebuffer);

        void BindFramebuffer(FramebufferTarget target, int framebuffer);
        void BindFramebuffer(FramebufferTarget target, uint framebuffer);

        unsafe void DeleteFramebuffers(int n, int* framebuffers);
        void DeleteFramebuffers(int n, int[] framebuffers);
        void DeleteFramebuffers(int n, ref int framebuffers);
        void DeleteFramebuffers(int n, ref uint framebuffers);
        unsafe void DeleteFramebuffers(int n, uint* framebuffers);
        void DeleteFramebuffers(int n, uint[] framebuffers);

        unsafe void GenFramebuffers(int n, int* framebuffers);
        void GenFramebuffers(int n, int[] framebuffers);
        void GenFramebuffers(int n, out int framebuffers);
        void GenFramebuffers(int n, out uint framebuffers);
        unsafe void GenFramebuffers(int n, uint* framebuffers);
        void GenFramebuffers(int n, uint[] framebuffers);

        FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget target);

#if !GLES
        void FramebufferTexture1D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, int texture, int level);
        void FramebufferTexture1D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level);
#endif

        void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, int texture, int level);
        void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level);

#if !GLES
        void FramebufferTexture3D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, int texture, int level, int zoffset);
        void FramebufferTexture3D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget textarget, uint texture, int level, int zoffset);
#endif

        void FramebufferRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, int renderbuffer);
        void FramebufferRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, uint renderbuffer);

        unsafe void GetFramebufferAttachmentParameter(FramebufferTarget target, FramebufferAttachment attachment, FramebufferParameterName pname, int* @params);
        void GetFramebufferAttachmentParameter(FramebufferTarget target, FramebufferAttachment attachment, FramebufferParameterName pname, int[] @params);
        void GetFramebufferAttachmentParameter(FramebufferTarget target, FramebufferAttachment attachment, FramebufferParameterName pname, out int @params);

        void GenerateMipmap(GenerateMipmapTarget target);
    }
}

#endif
