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
        [Test]
        public void ConvertFromMGColorString()
        {
            StringToColorConverter _converter = new StringToColorConverter();
            var input = "255,255,255,255";

            var result = _converter.ConvertFrom(null, null, input);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Color>(result);
            var color = (Color)result;
            Assert.AreEqual(255, color.R);
            Assert.AreEqual(255, color.G);
            Assert.AreEqual(255, color.B);
            Assert.AreEqual(255, color.A);
        }

        [Test]
        public void ConvertFromXNAColorString()
        {            
            StringToColorConverter _converter = new StringToColorConverter();
            var input = "{R:255 G:255 B:255 A:255}";
            
            var result = _converter.ConvertFrom(null, null, input);
         
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Color>(result);
            var color = (Color)result;
            Assert.AreEqual(255, color.R);
            Assert.AreEqual(255, color.G);
            Assert.AreEqual(255, color.B);
            Assert.AreEqual(255, color.A);
        }

        [Test]
        public void InvalidStringThrowsFormatException()
        {
            StringToColorConverter _converter = new StringToColorConverter();

            string inputMGShort = "255,255,255";
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, inputMGShort));

            string inputMGLong = "255,255,255,255,255";
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, inputMGLong));

            string inputXNAInvalid = "{R:255G:255B:255A:255}";
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, inputXNAInvalid));

            string inputXNAShort = "{R:255 G:255 B:255}";
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, inputXNAShort));

            string inputXNALong = "{R:255 G:255 B:255 A:255 Q:255}";
            Assert.Throws<FormatException>(() => _converter.ConvertFrom(null, null, inputXNALong));
        }

    }
}
