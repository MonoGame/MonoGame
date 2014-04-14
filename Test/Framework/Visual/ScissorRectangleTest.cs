using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    [TestFixture]
    internal class ScissorRectangleTest : VisualTestFixtureBase
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _texture;
        private RenderTarget2D _extraRenderTarget;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Game.LoadContentWith += (sender, e) =>
            {
                _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
                _texture = Game.Content.Load<Texture2D>(Paths.Texture("Surge"));
                _extraRenderTarget = new RenderTarget2D(Game.GraphicsDevice, 256, 256);
            };

            Game.UnloadContentWith += (sender, e) =>
            {
                _texture.Dispose();
                _spriteBatch.Dispose();
            };

            Game.PreDrawWith += (sender, e) => Game.GraphicsDevice.Clear(Color.CornflowerBlue);
        }

        [Test]
        public void Draw_with_render_target_change()
        {
            Game.DrawWith += (sender, e) =>
            {
                var renderTargets = Game.GraphicsDevice.GetRenderTargets();
                Game.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, 20, 20);
                Game.GraphicsDevice.SetRenderTarget(_extraRenderTarget);
                Game.GraphicsDevice.SetRenderTargets(renderTargets);
                DrawTexture();
            };

            RunSingleFrameTest();
        }

        [Test]
        public void Draw_without_render_target_change()
        {
            Game.DrawWith += (sender, e) =>
            {
                var renderTargets = Game.GraphicsDevice.GetRenderTargets();
                Game.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, 20, 20);
                Game.GraphicsDevice.SetRenderTargets(renderTargets);
                DrawTexture();
            };

            RunSingleFrameTest();
        }

        private void DrawTexture()
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.GraphicsDevice.RasterizerState = new RasterizerState { ScissorTestEnable = true };
            _spriteBatch.Draw(_texture, new Vector2(0, 0), Color.White);
            _spriteBatch.End();
        }
    }
}
