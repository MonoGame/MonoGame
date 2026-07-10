// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using GD = MonoGame.InteractiveTests.GameDebug;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Tests <see cref="RenderTarget2D"/> rendering: Render to a render target and then render
    /// the render target texture.
    /// </summary>
    [InteractiveTest("RenderTarget Test", Categories.General)]
    public class RenderTargetTest : TestGame
    {
        private Texture2D _texture;
        private RenderTarget2D _renderTarget;
        private SpriteBatch _batch;

        private int _xy = 0;
        private int _dxy = 1;

        /// <summary>
        /// Load any content here: This is invoked before the <see cref="Game"/>
        /// updates/draws itself.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Add a texture from a PNG file
            _texture = Texture2D.FromStream(GraphicsDevice,
                TitleContainer.@OpenStream(@"Content\Textures\LogoOnly_64px.png"));
            _renderTarget = new RenderTarget2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color, DepthFormat.None,  0, RenderTargetUsage.PreserveContents);

            _batch = new SpriteBatch(GraphicsDevice);
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();

            _helpLabel.Text += "Showcases render targets: First renders onto a \n" +
                              "RenderTarget, then render that texture  on a moving quad.";
            _helpLabel.SizeToFit();
        }

        /// <summary>
        /// Renders a single frame for a <see cref="Game"/>.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Note: Render targets have to be rendered BEFORE any other
            // frame buffer content. Any framebuffer rendering that
            // happens before Render Target rendering may be discarded on
            // certain platforms such as OpenGL.
            _batch.Begin();
            _batch.Draw(_texture, new Vector2(50, 150), null, Color.Red);
            _batch.End();

            // First render to the RenderTarget.
            GraphicsDevice.SetRenderTarget(_renderTarget);
            _batch.Begin();
            _batch.Draw(_texture, new Vector2(_xy, _xy), null, Color.White);
            _batch.End();

            // Now render the render target texture onto the screen.
            GraphicsDevice.SetRenderTarget(null);
            _batch.Begin();
            _batch.Draw(_renderTarget, new Vector2(100 + _xy, 100 + _xy), null, Color.White);
            _xy += _dxy;
            if (_xy >= 200 || _xy < 0) { _dxy = -_dxy; }

            _batch.End();

            base.Draw(gameTime);
        }
    }
}
