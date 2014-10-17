using Microsoft.Xna.Framework.Utilities;
using NUnit.Framework;
using System.IO;

namespace MonoGame.Tests.Framework
{
    class UtilitiesTest
    {
        [TestCase(@"C:\Game\Content", @"file.extension", "file", @"C:\Game\Content\file.extension")]
        [TestCase(@"C:\Game\Content", @"..\file.extension", "file", @"C:\Game\file.extension")]
        [TestCase(@"C:/Game/Content", @"file.extension", "file", @"C:/Game/Content/file.extension")]
        [TestCase(@"C:/Game/Content", @"../file.extension", "file", @"C:/Game/file.extension")]
        [TestCase(@"\application0\Content", @"file.extension", "file", @"\application0\Content\file.extension")]
        [TestCase(@"\application0\Content", @"..\file.extension", "file", @"\application0\file.extension")]
        [TestCase(@"/application0/Content", @"file.extension", "file", @"/application0/Content/file.extension")]
        [TestCase(@"/application0/Content", @"../file.extension", "file", @"\application0\file.extension")]
        [TestCase(@"Content", @"file.extension", "file", @"\Content\file.extension")]
        [TestCase(@"Content", @"..\file.extension", "file", @"\file.extension")]
        [TestCase(@"Content", @"file.extension", "file", @"/Content/file.extension")]
        [TestCase(@"Content", @"../file.extension", "file", @"/file.extension")]
        public void ResolveRelativePath(string contentRootDir, string relativePath, string assetName, string matchFullPath)
        {
            var rootFilePath = Path.Combine(contentRootDir, assetName);

            var fullPath = FileHelpers.ResolveRelativePath(rootFilePath, relativePath);
            Assert.NotNull(fullPath);

            // Make sure the matching path has the right seperators as well.
            matchFullPath = FileHelpers.NormalizeFilePathSeparators(matchFullPath);
           
            Assert.AreEqual(matchFullPath, fullPath);
            Assert.AreEqual(-1, fullPath.IndexOf(FileHelpers.NotSeparator));
        }
    }
}
