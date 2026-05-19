// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using GD = MonoGame.InteractiveTests.GameDebug;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Tests <see cref="Texture2D"/> rendering.
    /// </summary>
    [InteractiveTest("Texture Basic Test", Categories.General)]
    public class TextureBasicTest : TestGame
    {
        private Texture2D _texture;
        private Texture2D _customTexture;
        private SpriteBatch _batch;

        private TimeSpan _prevTime;
        private bool _flipColor;
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

            // Also add a custom texture with a solid color.
            _customTexture = new Texture2D(GraphicsDevice, 128, 128, false, SurfaceFormat.Color);

            int imageSizeBytes = _customTexture.Width * _customTexture.Height * 4;
            var bytes = new byte[imageSizeBytes];
            for (int i = 0; i < imageSizeBytes; i++)
            {
                // Red orange (MonoGame color).
                if (i % 4 == 0) { bytes[i] = (byte)(231); }

                if (i % 4 == 1) { bytes[i] = (byte)(60); }

                if (i % 4 == 2) { bytes[i] = (byte)(0); }

                if (i % 4 == 3) { bytes[i] = (byte)(255); }
            }

            _customTexture.SetData(bytes);

            _batch = new SpriteBatch(GraphicsDevice);
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();

            _helpLabel.Text += "Showcases rendering of a texture: One moving logo,\n one static with custom bitmap.";
            _helpLabel.SizeToFit();
        }

        /// <summary>
        /// Renders a single frame for a <see cref="Game"/>.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - _prevTime >= TimeSpan.FromMilliseconds(2500))
            {
                // Output some memory stuff.
                _prevTime = gameTime.TotalGameTime;
                _flipColor = !_flipColor;
            }

            GraphicsDevice.Clear(_flipColor ? Color.LightGray : Color.LightBlue);

            _batch.Begin();

            _batch.Draw(_texture, new Vector2(100 + _xy, 50 + _xy), null, Color.White);
            _xy += _dxy;
            if (_xy >= 200 || _xy < 0) { _dxy = -_dxy; }

            _batch.Draw(_customTexture, new Vector2(50, 250), null, Color.White);
            _batch.End();
            base.Draw(gameTime);
        }
    }
}
