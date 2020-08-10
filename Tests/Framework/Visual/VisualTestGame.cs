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

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	class VisualTestGame : TestGameBase, IFrameCaptureSource {
		public VisualTestGame ()
		{
			new GraphicsDeviceManager (this) {
				PreferredBackBufferWidth = 800,
				PreferredBackBufferHeight = 480,
				GraphicsProfile = GraphicsProfile.HiDef,
			};

			Services.AddService<IFrameCaptureSource> (this);
		}

		protected override void Draw (GameTime gameTime)
		{
			if (_shouldCaptureFrame)
				StartRenderingToTexture ();

			try {
				base.Draw (gameTime);
			} finally {
				if (_shouldCaptureFrame)
					StopRenderingToTexture ();
			}

			_shouldCaptureFrame = false;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (_renderToTextureTarget != null) {
					_renderToTextureTarget.Dispose ();
					_renderToTextureTarget = null;
				}
			}
			base.Dispose (disposing);
		}

		#region IFrameSource Implementation

		private RenderTarget2D _renderToTextureTarget;
		private bool _shouldCaptureFrame;
		public void ScheduleFrameCapture ()
		{
			_shouldCaptureFrame = true;
		}

		public Texture2D GetCapturedFrame ()
		{
			return _renderToTextureTarget;
		}

		public void ReleaseCapturedFrame (Texture2D frame)
		{
			_renderToTextureTarget.Dispose ();
			_renderToTextureTarget = null;
		}

		private void StartRenderingToTexture ()
		{
			if (_renderToTextureTarget != null)
				throw new InvalidOperationException ("Already rendering to a different texture.");

			_renderToTextureTarget = new RenderTarget2D(
				GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
				false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

			GraphicsDevice.SetRenderTarget (_renderToTextureTarget);
		}

		private void StopRenderingToTexture ()
		{
			if (_renderToTextureTarget == null)
				throw new InvalidOperationException ("Not currently rendering to a texture.");

			GraphicsDevice.SetRenderTarget (null);
		}

		#endregion IFrameSource Implementation
	}
}
