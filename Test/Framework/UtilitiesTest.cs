using Microsoft.Xna.Framework.Utilities;
using NUnit.Framework;
using System.IO;

namespace MonoGame.Tests.Framework
{
    class UtilitiesTest
    {
        [TestCase(  @"C:\Game\Content\file",            @"file.extension",          @"C:\Game\Content\file.extension")]
        [TestCase(  @"C:\Game\Content\file",            @"..\file.extension",       @"C:\Game\file.extension")]
        [TestCase(  @"C:\Game\Content\..\file",         @"file.extension",          @"C:\Game\file.extension")]
        [TestCase(  @"C:\Game\Content\..\file",         @"..\file.extension",       @"C:\file.extension")]
        [TestCase(  @"C:\Game\Content\.\file",          @"file.extension",          @"C:\Game\Content\file.extension")]
        [TestCase(  @"C:\Game\Content\.\file",          @".\file.extension",        @"C:\Game\Content\file.extension")]
        [TestCase(  @"C:/Game/Content/file",            @"file.extension",          @"C:/Game/Content/file.extension")]
        [TestCase(  @"C:/Game/Content/file",            @"../file.extension",       @"C:/Game/file.extension")]
        [TestCase(  @"C:/Game/Content/../file",         @"file.extension",          @"C:/Game/file.extension")]
        [TestCase(  @"C:/Game/Content/../file",         @"../file.extension",       @"C:/file.extension")]
        [TestCase(  @"C:/Game/Content/./file",          @"file.extension",          @"C:/Game/Content/file.extension")]
        [TestCase(  @"C:/Game/Content/./file",          @"./file.extension",        @"C:/Game/Content/file.extension")]
        [TestCase(  @"\application0\Content\file",      @"file.extension",          @"\application0\Content\file.extension")]
        [TestCase(  @"\application0\Content\file",      @"..\file.extension",       @"\application0\file.extension")]
        [TestCase(  @"\application0\Content\..\file",   @"file.extension",          @"\application0\file.extension")]
        [TestCase(  @"\application0\Content\..\file",   @"..\file.extension",       @"\file.extension")]
        [TestCase(  @"\application0\Content\.\file",    @"file.extension",          @"\application0\Content\file.extension")]
        [TestCase(  @"\application0\Content\.\file",    @".\file.extension",        @"\application0\Content\file.extension")]
        [TestCase(  @"/application0/Content/file",      @"file.extension",          @"/application0/Content/file.extension")]
        [TestCase(  @"/application0/Content/file",      @"../file.extension",       @"/application0/file.extension")]
        [TestCase(  @"/application0/Content/../file",   @"file.extension",          @"/application0/file.extension")]
        [TestCase(  @"/application0/Content/../file",   @"../file.extension",       @"/file.extension")]
        [TestCase(  @"/application0/Content/./file",    @"file.extension",          @"/application0/Content/file.extension")]
        [TestCase(  @"/application0/Content/./file",    @"./file.extension",        @"/application0/Content/file.extension")]
        [TestCase(  @"Content\file",                    @"file.extension",          @"Content\file.extension")]
        [TestCase(  @"Content\file",                    @"..\file.extension",       @"file.extension")]
        [TestCase(  @"Content\..\file",                 @"file.extension",          @"file.extension")]
        [TestCase(  @"Content\..\file",                 @"..\file.extension",       @"file.extension")]
        [TestCase(  @"Content\.\file",                  @"file.extension",          @"Content\file.extension")]
        [TestCase(  @"Content\.\file",                  @".\file.extension",        @"Content\file.extension")]
        [TestCase(  @"Content/file",                    @"file.extension",          @"Content/file.extension")]
        [TestCase(  @"Content/file",                    @"../file.extension",       @"file.extension")]
        [TestCase(  @"Content/../file",                 @"file.extension",          @"file.extension")]
        [TestCase(  @"Content/../file",                 @"../file.extension",       @"file.extension")]
        [TestCase(  @"Content/./file",                  @"file.extension",          @"Content/file.extension")]
        [TestCase(  @"Content/./file",                  @"./file.extension",        @"Content/file.extension")]
        public void ResolveRelativePath(
                    string rootFilePath,                string relativePath,        string matchFullPath)
        {
            var fullPath = FileHelpers.ResolveRelativePath(rootFilePath, relativePath);
            Assert.NotNull(fullPath);

            // Make sure the matching path has the right seperators as well.
            matchFullPath = FileHelpers.NormalizeFilePathSeparators(matchFullPath);
           
            Assert.AreEqual(matchFullPath, fullPath);
            Assert.AreEqual(-1, fullPath.IndexOf(FileHelpers.NotSeparator));
        }
    }
}
