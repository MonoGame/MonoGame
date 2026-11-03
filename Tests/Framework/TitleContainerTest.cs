// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class TitleContainerTest
    {
        [Test]
        public void OpenStream()
        {
            Assert.Throws<ArgumentNullException>(() => TitleContainer.OpenStream(null));
            Assert.Throws<ArgumentNullException>(() => TitleContainer.OpenStream(string.Empty));
            Assert.Throws<ArgumentNullException>(() => TitleContainer.OpenStream(""));
            Assert.Throws<FileNotFoundException>(() => TitleContainer.OpenStream(" "));
            Assert.Throws<FileNotFoundException>(() => TitleContainer.OpenStream("notfound"));
            // under mono we get a FileNotFoundException for this path.
            Assert.Throws(Is.TypeOf<ArgumentException>().Or.TypeOf<FileNotFoundException>(), () => TitleContainer.OpenStream(@"C:\\"));

            // TODO: This always fails on XNA... even though it shouldn't.  I suspect 
            // this is because internally XNA uses the entry/active assembly as the
            // root of the path.  Since we are launched from some Nunit runner things
            // end up not working as we expect.
            //
            // We need to figure out the trick to hack around this to validate 
            // non-failure tests against XNA.
#if !XNA
            var stream = TitleContainer.OpenStream(@"Assets\Xml\01_TheBasics.xml");
            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(true, stream.CanRead);
            Assert.AreEqual(true, stream.CanSeek);
            Assert.AreEqual(false, stream.CanWrite);
            stream.Dispose();
#endif
        }

        [Test]
        public void OpenStreamReadHonorsBufferOffset()
        {
#if !XNA
            const int count = 16;
            const int offset = 8;

            var expected = new byte[count];
            using (var stream = TitleContainer.OpenStream(@"Assets\Xml\01_TheBasics.xml"))
            {
                Assert.AreEqual(count, stream.Read(expected, 0, count));
                stream.Position = 0;

                // Fill with a sentinel so we can see exactly which bytes get written.
                var actual = new byte[offset + count + offset];
                for (var i = 0; i < actual.Length; i++)
                    actual[i] = 0xAA;

                Assert.AreEqual(count, stream.Read(actual, offset, count));

                for (var i = 0; i < offset; i++)
                    Assert.AreEqual((byte)0xAA, actual[i], $"Byte {i} before the offset was overwritten.");

                for (var i = 0; i < count; i++)
                    Assert.AreEqual(expected[i], actual[offset + i], $"Byte {i} was not written at the offset.");

                for (var i = offset + count; i < actual.Length; i++)
                    Assert.AreEqual((byte)0xAA, actual[i], $"Byte {i} after the data was overwritten.");
            }
#endif
        }
    }
}
