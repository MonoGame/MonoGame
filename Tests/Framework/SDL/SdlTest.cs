using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    internal class SdlTest
    {
        [Test]
        public void Sdl2VersionComparison()
        {
            // Ensure SDL 2 version comparison works across shift in SDL version introduced
            // post version 2.0.22

            var oldVersionNomenclature = new Sdl.Version() { Major = 2, Minor = 0, Patch = 22 };
            var newVersionNomenclature = new Sdl.Version() { Major = 2, Minor = 23, Patch = 0 };

            var sameVersion = new Sdl.Version() { Major = 2, Minor = 0, Patch = 22 };

            Assert.IsTrue(oldVersionNomenclature == sameVersion);
            sameVersion.Equals(oldVersionNomenclature);

            Assert.IsTrue(oldVersionNomenclature != newVersionNomenclature);

            Assert.IsTrue(oldVersionNomenclature < newVersionNomenclature);
            Assert.IsTrue(oldVersionNomenclature <= newVersionNomenclature);

            Assert.IsTrue(newVersionNomenclature > oldVersionNomenclature);
            Assert.IsTrue(newVersionNomenclature >= oldVersionNomenclature);
        }

        [Test]
        public void Sdl2VersionString()
        {
            var version = new Sdl.Version() { Major = 2, Minor = 0, Patch = 5 };

            Assert.AreEqual(version.ToString(), "2.0.5");
        }
    }
}
