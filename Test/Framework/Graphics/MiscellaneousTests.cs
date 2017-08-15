using MonoGame.Tests.Components;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    internal class MiscellaneousTests : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void Colored3DCube()
        {
            PrepareFrameCapture();

            var cube = new Colored3DCubeComponent(gd);
            cube.LoadContent();
            cube.Draw();
            cube.UnloadContent();

            CheckFrames();
        }
    }
}