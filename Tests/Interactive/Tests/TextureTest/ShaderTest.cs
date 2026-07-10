// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using GD = MonoGame.InteractiveTests.GameDebug;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Tests <see cref="Texture2D"/> rendering with a shader.
    /// </summary>
    [InteractiveTest("Shader Test", Categories.General)]
    public class ShaderTest : TestGame
    {
        private Texture2D _texture;
        private SpriteBatch _batch;

        private float _tween = 0;
        private Effect _effect;

        protected override void LoadContent()
        {
            base.LoadContent();

            // Add a texture from a PNG file
            _texture = Texture2D.FromStream(GraphicsDevice,
                TitleContainer.OpenStream(@"Content\Textures\LogoOnly_64px.png"));
            _effect = new Effect(GraphicsDevice, TestUtils.ReadBytesFromStream(@"Content\Effect\test.fx.mgfxo"));
            _batch = new SpriteBatch(GraphicsDevice);
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();

            _helpLabel.Text += "Render a quad with texture + apply shader.";
            _helpLabel.SizeToFit();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);

            // Setup matrices. In real production code, you want to cache/update only when things change.
            var vp = GraphicsDevice.Viewport;
            Matrix.CreateOrthographicOffCenter(0, vp.Width * (1 - _tween / 2), vp.Height * (1 - _tween / 2), 0, 0, -1,
                out var projection);

            if (GraphicsDevice.UseHalfPixelOffset)
            {
                projection.M41 += -0.5f * projection.M11;
                projection.M42 += -0.5f * projection.M22;
            }

            _effect.Parameters["MatrixTransform"].SetValue(projection);
            _effect.Parameters["tween4"].SetValue(_tween);
            //_effect.Parameters["Texture"].SetValue(_texture); // This is automatically set by the "Draw" call.
            _batch.Begin(effect: _effect);

            _batch.Draw(_texture, new Vector2(100, 150), null, Color.White);
            _tween += 0.01f;
            if (_tween >= 1.0) { _tween = 0.0f; }

            _batch.End();
            base.Draw(gameTime);
        }
    }
}
