// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.ContentPipeline
{
    internal class StringToColorConverterTests
    {
        [TestCase("255,255,255,255", 255, 255, 255, 255)]
        [TestCase("255,0,255,255", 255, 0, 255, 255)]
        [TestCase("0,0,0,0", 0, 0, 0, 0)]
        [TestCase("100,149,237,255", 100, 149, 237, 255)]
        [TestCase("231,60,0,255", 231, 60, 0, 255)]
        public void ConvertFromMGString(string input, int r, int g, int b, int a)
        {
            StringToColorConverter _converter = new StringToColorConverter();

            var result = _converter.ConvertFrom(null, null, input);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Color>(result);
            var color = (Color)result;
            Assert.AreEqual(r, color.R);
            Assert.AreEqual(g, color.G);
            Assert.AreEqual(b, color.B);
            Assert.AreEqual(a, color.A);
        }

        [TestCase("{R:255 G:255 B:255 A:255}", 255, 255, 255, 255)]
        [TestCase("{R:255 G:0 B:255 A:255}", 255, 0, 255, 255)]
        [TestCase("{R:0 G:0 B:0 A:0}", 0, 0, 0, 0)]
        [TestCase("{R:100 G:149 B:237 A:255}", 100, 149, 237, 255)]
        [TestCase("{R:231 G:60 B:0 A:255}", 231, 60, 0, 255)]
        public void ConvertFromXNAString(string input, int r, int g, int b, int a)
        {
            StringToColorConverter _converter = new StringToColorConverter();

            var result = _converter.ConvertFrom(null, null, input);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Color>(result);
            var color = (Color)result;
            Assert.AreEqual(r, color.R);
            Assert.AreEqual(g, color.G);
            Assert.AreEqual(b, color.B);
            Assert.AreEqual(a, color.A);
        }

        [TestCase("255,255,255")]
        [TestCase("255,255,255,255,255")]
        [TestCase("{R:255G:255B:255A:255}")]
        [TestCase("{R:255 G:255 B:255}")]
        [TestCase("{R:255 G:255 B:255 A:255 Q:255}")]
        public void InvalidStringThrowsArgumentException(string input)
        {
            StringToColorConverter _converter = new StringToColorConverter();

            Assert.Throws<ArgumentException>(() => _converter.ConvertFrom(null, null, input));
        }

    }
}
