#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

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

