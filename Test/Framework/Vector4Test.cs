using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Vector4Test
    {
        [Test]
        public void TypeConverter()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Vector4));
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            
            Assert.AreEqual(new Vector4(32, 64, 128, 255), converter.ConvertFromString(null, invariantCulture, "32, 64, 128, 255"));
            Assert.AreEqual(new Vector4(0.5f, 2.75f, 4.125f, 8.0625f), converter.ConvertFromString(null, invariantCulture, "0.5, 2.75, 4.125, 8.0625"));
            Assert.AreEqual(new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f), converter.ConvertFromString(null, invariantCulture, "1024.5, 2048.75, 4096.125, 8192.0625"));
            Assert.AreEqual("32, 64, 128, 255", converter.ConvertToString(null, invariantCulture, new Vector4(32, 64, 128, 255)));
            Assert.AreEqual("0.5, 2.75, 4.125, 8.0625", converter.ConvertToString(null, invariantCulture, new Vector4(0.5f, 2.75f, 4.125f, 8.0625f)));
            Assert.AreEqual("1024.5, 2048.75, 4096.125, 8192.0625", converter.ConvertToString(null, invariantCulture, new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f)));

            CultureInfo otherCulture = new CultureInfo("el-GR");

            Assert.AreEqual(new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f), converter.ConvertFromString(null, otherCulture, "1024,5; 2048,75; 4096,125; 8192,0625"));
            Assert.AreEqual("1024,5; 2048,75; 4096,125; 8192,0625", converter.ConvertToString(null, otherCulture, new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f)));
        }
    }
}
