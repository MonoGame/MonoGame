using System;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    internal class OcclusionQueryNonVisualTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ConstructorsAndProperties()
        {
            Assert.Throws<ArgumentNullException>(() => new OcclusionQuery(null));

            var occlusionQuery = new OcclusionQuery(gd);

            Assert.IsFalse(occlusionQuery.IsComplete);

            Assert.Throws<InvalidOperationException>(
                () => { var n = occlusionQuery.PixelCount; },
                "PixelCount throws when query not yet started.");
        }

        [Test]
        public void MismatchedBeginEnd()
        {
            var occlusionQuery = new OcclusionQuery(gd);

            Assert.Throws<InvalidOperationException>(() => occlusionQuery.End());

            occlusionQuery.Begin();
            Assert.Throws<InvalidOperationException>(() => occlusionQuery.Begin());
        }
    }
}