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
    /// Tests <see cref="Texture2D"/> rendering + updates.
    /// </summary>
    [InteractiveTest("Texture Update Test", Categories.General)]
    public class TextureUpdateTest : TestGame
    {
        private Texture2D _customTexture;
        private SpriteBatch _batch;

        private int _xy = 0;
        private int _dxy = 1;
        private int _imageSizeBytes;

        /// <summary>
        /// Load any content here: This is invoked before the <see cref="Game"/>
        /// updates/draws itself.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Also add a custom texture with a solid color.
            _customTexture = new Texture2D(GraphicsDevice, 350, 200, false, SurfaceFormat.Color);

            _UpdateTexture(_xy);
            _batch = new SpriteBatch(GraphicsDevice);
            InitializeGui();
        }

        /// <summary>Updates the texture with some pattern based on the <param name="variation"></param></summary>
        private void _UpdateTexture(int variation)
        {
            _imageSizeBytes = _customTexture.Width * _customTexture.Height * 4;
            var bytes = new byte[_imageSizeBytes];
            for (int i = 0; i < _imageSizeBytes; i++)
            {
                // Red orange (MonoGame color).
                if (i % 4 == 0) { bytes[i] = (byte)(231 + variation); }

                if (i % 4 == 1) { bytes[i] = (byte)(60 + variation); }

                if (i % 4 == 2) { bytes[i] = (byte)(0 + variation); }

                if (i % 4 == 3) { bytes[i] = (byte)(255); }
            }

            _customTexture.SetData(bytes);
        }

        protected override void InitializeGui()
        {
            base.InitializeGui();
            _helpLabel.Text += "Render a single texture onto a moving quad and \nupdate the texture every frame.";
            _helpLabel.SizeToFit();
        }

        /// <summary>
        /// Renders a single frame for a <see cref="Game"/>.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _batch.Begin();

            _UpdateTexture(_xy);
            _batch.Draw(_customTexture, new Vector2(100 + _xy, 50 + _xy), null, Color.White);
            _xy += _dxy;
            if (_xy >= 200 || _xy < 0) { _dxy = -_dxy; }

            _batch.End();
            base.Draw(gameTime);
        }
    }
}
