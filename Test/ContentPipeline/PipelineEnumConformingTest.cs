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
        [TestCase(0, FontDescriptionStyle.Regular)]
        [TestCase(1, FontDescriptionStyle.Bold)]
        [TestCase(2, FontDescriptionStyle.Italic)]
        [TestCase(3, FontDescriptionStyle.Italic |FontDescriptionStyle.Bold)]
        public void FontDescriptionStyleTest(int expected, FontDescriptionStyle style)
        {
            Assert.AreEqual(expected, (int) style);
        }
    }
}
