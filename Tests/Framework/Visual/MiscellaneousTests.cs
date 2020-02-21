// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using MonoGame.Tests.Components;
using NUnit.Framework;

namespace MonoGame.Tests.Visual {
	[TestFixture]
	class MiscellaneousTests : VisualTestFixtureBase
    {
		[Test]
#if !DESKTOPGL
        // TODO: this sometimes crashes when trying to clean up a shader.
        // it's notable that the cleanup is called from another thread and
        // run in Threading.Run at the start of a run loop when the crash happens.
        // I can consistently reproduce this by running the ShaderTests and this in
        // succession using ReSharpers test runner
        [Ignore("Shader cleanup causes a crash, we should investigate this")]
#else
        [Ignore ("Microsoft.Xna.Framework.Graphics.MonoGameGLException : GL.GetError() returned 1286. Invesigate")]
#endif
		public void DrawOrder_falls_back_to_order_of_addition_to_Game ()
		{
			Game.PreDrawWith += (sender, e) => {
				Game.GraphicsDevice.Clear (Color.CornflowerBlue);
			};

			Game.Components.Add (new ImplicitDrawOrderComponent (Game));
			RunMultiFrameTest (captureCount: 5);
		}
		
		[TestCase(true)]
		[TestCase(false)]
        [Ignore("Fix me!")]
		public void TexturedQuad_lighting (bool enableLighting)
		{
			Game.Components.Add (new TexturedQuadComponent (Game, enableLighting));
			RunSingleFrameTest ();
		}

		[Test, Ignore("Fix me!")]
		public void SpaceshipModel ()
		{
			Game.Components.Add (new SpaceshipModelDrawComponent(Game));
			RunMultiFrameTest (captureCount: 10, captureStride: 2);
		}
	}
}
