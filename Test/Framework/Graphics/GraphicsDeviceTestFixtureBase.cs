using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    internal class GraphicsDeviceTestFixtureBase
    {
        protected TestGameBase game;
        protected GraphicsDeviceManager gdm;
        protected GraphicsDevice gd;

        [SetUp]
        public void SetUp()
        {
            game = new TestGameBase();
            gdm = new GraphicsDeviceManager(game);
            ((IGraphicsDeviceManager) game.Services.GetService(typeof(IGraphicsDeviceManager))).CreateDevice();
            gd = game.GraphicsDevice;
        }

        [TearDown]
        public void TearDown()
        {
            game.Dispose();
        }

        
    }
}