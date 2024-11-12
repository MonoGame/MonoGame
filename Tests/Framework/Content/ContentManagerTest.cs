using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Content
{
    public class ContentManagerTest
    {
        [TestCase("C:\\image.png")]
        [TestCase("\\image.png")]
        [TestCase("//image.png")]
        public void ThrowExceptionIfAssetNameIsRootedPath(string assetName)
        {
            GameServiceContainer services = new GameServiceContainer();
            ContentManager content = new ContentManager(services, "Content");
            var exception = Assert.Throws<ContentLoadException>(() => content.Load<Texture2D>(assetName));
            StringAssert.Contains("rooted (absolute)", exception.Message);
        }
    }
}
