using Microsoft.Xna.Framework.Input.Touch;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    class TouchCollectionTest
    {
        [Test]
        public void WorksWhenConstructedEmpty()
        {
            TouchCollection collection = new TouchCollection();

            Assert.AreEqual(0, collection.Count);
            foreach (var touch in collection)
                Assert.Fail("Shouldn't have any touches in an empty collection");

            Assert.AreEqual(-1, collection.IndexOf(new TouchLocation()));

            TouchLocation touchLocation;
            Assert.False(collection.FindById(1, out touchLocation));
        }
    }
}
