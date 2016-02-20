using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    public class PipelineEnumConformingTest
    {
        [Test]
        public void FontDescriptionStyleTest()
        {
            Assert.AreEqual(0, (int) FontDescriptionStyle.Regular);
            Assert.AreEqual(1, (int) FontDescriptionStyle.Bold);
            Assert.AreEqual(2, (int) FontDescriptionStyle.Italic);
        }
    }
}
