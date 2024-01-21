// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
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
