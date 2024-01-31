// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.OpenGL;
using System;

namespace Microsoft.Xna.Framework {
	partial class iOSGameView {
        private interface IOpenGLApi
        {
            FramebufferErrorCode CheckFramebufferStatus (FramebufferTarget target);
            void BindFramebuffer (FramebufferTarget target, int framebuffer);
            void BindRenderbuffer (RenderbufferTarget target, int renderbuffer);
            void DeleteFramebuffers (int n, ref int framebuffers);
            void DeleteRenderbuffers (int n, ref int renderbuffers);
            void FramebufferRenderbuffer (FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, int renderbuffer);
            void GenFramebuffers (int n, ref int framebuffers);
            void GenRenderbuffers (int n, ref int renderbuffers);
            void GetInteger (GetPName name, ref int value);
            void Scissor (int x, int y, int width, int height);
			void Viewport (int x, int y, int width, int height);
        }

		private class Gles20Api : IOpenGLApi {

            public Gles20Api()
            {
                GL.LoadEntryPoints();
            }

            public FramebufferErrorCode CheckFramebufferStatus (FramebufferTarget target)
			{
				return GL.CheckFramebufferStatus (target);
			}

			public void BindFramebuffer (FramebufferTarget target, int framebuffer)
			{
                GL.BindFramebuffer(target, framebuffer);
			}

			public void BindRenderbuffer (RenderbufferTarget target, int renderbuffer)
			{
				GL.BindRenderbuffer (target, renderbuffer);
			}

			public void DeleteFramebuffers (int n, ref int framebuffers)
			{
				GL.DeleteFramebuffers (n, ref framebuffers);
			}

			public void DeleteRenderbuffers (int n, ref int renderbuffers)
			{
				GL.DeleteRenderbuffers (n, ref renderbuffers);
			}

			public void FramebufferRenderbuffer (
				FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbuffertarget, int renderbuffer)
			{
                GL.FramebufferRenderbuffer (target, attachment, renderbuffertarget, renderbuffer);
			}

			public void GenFramebuffers (int n, ref int framebuffers)
			{
				GL.GenFramebuffers (n, out framebuffers);
			}

			public void GenRenderbuffers (int n, ref int renderbuffers)
			{
				GL.GenRenderbuffers (n, out renderbuffers);
			}

			public void GetInteger (GetPName name, ref int value)
			{
				GL.GetInteger (name, out value);
			}

			public void Scissor (int x, int y, int width, int height)
			{
				GL.Scissor (x, y, width, height);
			}

			public void Viewport (int x, int y, int width, int height)
			{
				GL.Viewport (x, y, width, height);
			}
		}
	}
}

