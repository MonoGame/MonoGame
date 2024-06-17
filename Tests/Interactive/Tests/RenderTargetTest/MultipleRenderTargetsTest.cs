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
    /// Renders an object onto a render target and renders that back to that render target:
    /// - This produces an effect of a trail.
    /// </summary>
    [InteractiveTest("Multiple render targets updated", Categories.General)]
    public class MultipleRenderTargetsTest : TestGame
    {
        private Texture2D _texture;
        private RenderTarget2D _renderTarget1;
        private RenderTarget2D _renderTarget2;

        private SpriteBatch _batch;

        private Vector2 _xy = new(20, 50);
        private int _color = 0;
        private Vector2 _dxy = new(2.1f, 1.5f);

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
            int w = 200;
            int h = 300;
            _renderTarget1 = new RenderTarget2D(GraphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.PreserveContents);
            _renderTarget2 = new RenderTarget2D(GraphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.PreserveContents);

            GameDebug.C($"-- Render targets created: {_renderTarget1.Width}x{_renderTarget1.Height}");
            _batch = new SpriteBatch(GraphicsDevice);
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();
            _helpLabel.Text +=
                "You should see two trails on a green background: \n" +
                "Left: Render MonoGame logo and Right contents to render target (i.e. reuse contents)\n" +
                "Right: Render left contents to right (i.e. reuse contents)";
            _helpLabel.SizeToFit();
        }

        /// <summary>
        /// Renders a single frame for a <see cref="Game"/>.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // First render to `_renderTarget1` with the backbuffer from the previous frame (`_renderTarget2`).
            GraphicsDevice.SetRenderTarget(_renderTarget1);
            GraphicsDevice.Clear(Color.GreenYellow);

            _batch.Begin();
            _batch.Draw(_renderTarget2, new Vector2(0, 0), null, Color.White);
            _batch.Draw(_texture, _xy, null, new Color(_color, _color, _color));
            _xy += _dxy;

            if (_xy.X >= 200 || _xy.X < 0) { _dxy.X = -_dxy.X; }

            if (_xy.Y >= 200 || _xy.Y < 0) { _dxy.Y = -_dxy.Y; }

            _color += 1;
            if (_color >= 255) _color = 100;

            _batch.End();

            // Second, render the first render target onto the second render target.
            // This ends up being the backbuffer for the next frame
            GraphicsDevice.SetRenderTarget(_renderTarget2);
            _batch.Begin();
            _batch.Draw(_renderTarget1, new Vector2(0, 0), null, Color.White);
            _batch.End();

            // Now render the final blended render target texture onto the screen.
            GraphicsDevice.SetRenderTarget(null);
            _batch.Begin();
            _batch.Draw(_renderTarget1, new Vector2(0, 50), null, Color.White);
            _batch.Draw(_renderTarget2, new Vector2(_renderTarget1.Width, 50), null, Color.White);
            _batch.End();

            base.Draw(gameTime);
        }
    }
}
