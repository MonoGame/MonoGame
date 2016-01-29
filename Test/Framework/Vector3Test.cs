using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Vector3Test
    {
        [Test]
        public void TypeConverter()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Vector3));
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;

            Assert.AreEqual(new Vector3(32, 64, 128), converter.ConvertFromString(null, invariantCulture, "32, 64, 128"));
            Assert.AreEqual(new Vector3(0.5f, 2.75f, 4.125f), converter.ConvertFromString(null, invariantCulture, "0.5, 2.75, 4.125"));
            Assert.AreEqual(new Vector3(1024.5f, 2048.75f, 4096.125f), converter.ConvertFromString(null, invariantCulture, "1024.5, 2048.75, 4096.125"));
            Assert.AreEqual("32, 64, 128", converter.ConvertToString(null, invariantCulture, new Vector3(32, 64, 128)));
            Assert.AreEqual("0.5, 2.75, 4.125", converter.ConvertToString(null, invariantCulture, new Vector3(0.5f, 2.75f, 4.125f)));
            Assert.AreEqual("1024.5, 2048.75, 4096.125", converter.ConvertToString(null, invariantCulture, new Vector3(1024.5f, 2048.75f, 4096.125f)));

            CultureInfo otherCulture = new CultureInfo("el-GR");

            Assert.AreEqual(new Vector3(1024.5f, 2048.75f, 4096.125f), converter.ConvertFromString(null, otherCulture, "1024,5; 2048,75; 4096,125"));
            Assert.AreEqual("1024,5; 2048,75; 4096,125", converter.ConvertToString(null, otherCulture, new Vector3(1024.5f, 2048.75f, 4096.125f)));
        }
    }
}
