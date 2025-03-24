// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.InteractiveTests.TestUI;
using Color = Microsoft.Xna.Framework.Color;
using GD = MonoGame.InteractiveTests.GameDebug;
using Point = Microsoft.Xna.Framework.Point;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Tests blend states (specifically alpha blend) with many quads.
    /// </summary>
    [InteractiveTest("Alpha Blend Test", Categories.General)]
    public class AlphaBlendTest : TestGame
    {
        private Texture2D _texture;
        private SpriteBatch _batch;

        private const int _NUM_QUADS = 800;
        private Vector2[] _xy;
        private Vector2[] _dxy;
        private bool _isDeferred = true;

        protected override void LoadContent()
        {
            base.LoadContent();

            // Add a texture from a PNG file
            _texture = Texture2D.FromStream(GraphicsDevice,
                TitleContainer.@OpenStream(@"Content\Textures\LogoOnly_64px.png"));
            _batch = new SpriteBatch(GraphicsDevice);
            _xy = new Vector2[_NUM_QUADS];
            _dxy = new Vector2[_NUM_QUADS];
            var w = GraphicsDevice.Viewport.Width;
            var h = GraphicsDevice.Viewport.Height;
            for (int i = 0; i < _NUM_QUADS; ++i)
            {
                _xy[i] = new Vector2(Random.Shared.Next(w), Random.Shared.Next(h));
                _dxy[i] = new Vector2(Random.Shared.Next(6) - 3, Random.Shared.Next(6) - 3);
            }
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();
            _helpLabel.Text += $"{_NUM_QUADS} alpha blended quads moving about.";
            _helpLabel.SizeToFit();

            var modeSwitchButton = new Button
            {
                BackgroundColor = Color.Blue,
                Content = new Label
                {
                    Font = _font,
                    Text = $"Click to switch from {(_isDeferred ? "Deferred" : "Immediate")}",
                    TextColor = Color.White
                },
                Location = new(400, 0)
            };

            modeSwitchButton.Content.SizeToFit();
            modeSwitchButton.SizeToFit();
            modeSwitchButton.Tapped += (sender, e) =>
            {
                _isDeferred = !_isDeferred;
                (modeSwitchButton.Content as Label).Text = $"Click to switch from {(_isDeferred ? "Deferred" : "Immediate")}";
            };
            _universe.Add(modeSwitchButton);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Note: Deferred vs Immediate mode has a huge difference when rendering lots
            // of blended triangles. About 2-3 orders of magnitude in difference.
            // Immediate: 100 calls to `PlatformDrawUserIndexedPrimitives` with 4 vertices / 6 indices each.
            // Deferred : 1 call to `PlatformDrawUserIndexedPrimitives` with 400 vertices / 600 indices total.
            // As the number of primitives (triangle count) increases, the driver overhead overshadows the
            // relative performance difference. Still Deferred is faster just not by an order of magnitude.
            // (Note batch.Begin() by default uses deferred mode)
            _batch.Begin(sortMode: _isDeferred ? SpriteSortMode.Deferred : SpriteSortMode.Immediate,
                blendState: BlendState.AlphaBlend);

            var w = GraphicsDevice.Viewport.Width;
            var h = GraphicsDevice.Viewport.Height;
            var size = new Point(256, 256);
            for (int i = 0; i < _NUM_QUADS; ++i)
            {
                ref Vector2 xy = ref _xy[i];
                ref Vector2 dxy = ref _dxy[i];
                xy += dxy;

                if (xy.X > w || xy.X < 0) { dxy.X = -dxy.X; }

                if (xy.Y > h || xy.Y < 0) { dxy.Y = -dxy.Y; }

                _batch.Draw(_texture, destinationRectangle: new((int)xy.X, (int)xy.Y, size.X, size.Y),
                    null, new Color(Color.White, ((float)i) / (float)_NUM_QUADS));
            }

            _batch.End();
            base.Draw(gameTime);
        }
    }
}
