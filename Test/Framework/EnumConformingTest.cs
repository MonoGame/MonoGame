using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class EnumConformingTest
    {
        [Test]
        public void SetDataOptionsEnum()
        {
            Assert.AreEqual(0, (int)(SetDataOptions.None));
            Assert.AreEqual(1, (int)(SetDataOptions.Discard));
            Assert.AreEqual(2, (int)(SetDataOptions.NoOverwrite));
        }

        [Test]
        public void SpriteSortModeEnum()
        {
            Assert.AreEqual(0, (int)(SpriteSortMode.Deferred));
            Assert.AreEqual(1, (int)(SpriteSortMode.Immediate));
            Assert.AreEqual(2, (int)(SpriteSortMode.Texture));
            Assert.AreEqual(3, (int)(SpriteSortMode.BackToFront));
            Assert.AreEqual(4, (int)(SpriteSortMode.FrontToBack));
        }

        [Test]
        public void StencilOperationEnum()
        {
            Assert.AreEqual(0, (int)(StencilOperation.Keep));
            Assert.AreEqual(1, (int)(StencilOperation.Zero));
            Assert.AreEqual(2, (int)(StencilOperation.Replace));
            Assert.AreEqual(3, (int)(StencilOperation.Increment));
            Assert.AreEqual(4, (int)(StencilOperation.Decrement));
            Assert.AreEqual(5, (int)(StencilOperation.IncrementSaturation));
            Assert.AreEqual(6, (int)(StencilOperation.DecrementSaturation));
            Assert.AreEqual(7, (int)(StencilOperation.Invert));
        }
    }
}
