// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    public class BuilderTest
    {
        string NormalizePath(string path, string append = null)
        {
            if (append != null)
                path = Path.Combine(path, append);
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        [Test]
        public void CommandLineParserTests()
        {
            var args = ContentBuilderParams.Parse(null);
            Assert.AreEqual(ContentBuilderMode.None, args.Mode);

            args = ContentBuilderParams.Parse("");
            Assert.AreEqual(ContentBuilderMode.None, args.Mode);

            args = ContentBuilderParams.Parse("build");
            Assert.AreEqual(ContentBuilderMode.Builder, args.Mode);
            Assert.AreEqual(TargetPlatform.DesktopGL, args.Platform);
            Assert.AreEqual(false, args.CompressContent);
            Assert.AreEqual(false, args.SkipClean);
            Assert.IsTrue(Path.IsPathRooted(args.WorkingDirectory));
            Assert.AreEqual(Directory.GetCurrentDirectory(), args.WorkingDirectory);
            Assert.AreEqual("Content", args.SourceDirectory);
            Assert.AreEqual(NormalizePath(Directory.GetCurrentDirectory(), "Content"), args.RootedSourceDirectory);
            Assert.AreEqual(NormalizePath("bin/Content"), args.OutputDirectory);
            Assert.AreEqual(NormalizePath(Directory.GetCurrentDirectory(), "bin\\Content"), args.RootedOutputDirectory);
            Assert.AreEqual(NormalizePath("obj/Content"), args.IntermediateDirectory);
            Assert.AreEqual(NormalizePath(Directory.GetCurrentDirectory(), "obj\\Content"), args.RootedIntermediateDirectory);

            args = ContentBuilderParams.Parse("build", "-s", "C:/This/Does/Not/Exist");
            Assert.AreEqual(NormalizePath("C:/This/Does/Not/Exist"), args.SourceDirectory);

            args = ContentBuilderParams.Parse(
                "build",
                "-s", NormalizePath(Directory.GetCurrentDirectory(), "../Some/Folder"),
                "-o", NormalizePath(Directory.GetCurrentDirectory(), "Other/Folder"),
                "-i", NormalizePath(Directory.GetCurrentDirectory(), "Folder")
            );
            Assert.AreEqual(NormalizePath("../Some/Folder"), args.SourceDirectory);
            Assert.AreEqual(NormalizePath("Other/Folder"), args.OutputDirectory);
            Assert.AreEqual("Folder", args.IntermediateDirectory);

            args = ContentBuilderParams.Parse("server");
            Assert.AreEqual(ContentBuilderMode.Server, args.Mode);
        }
    } 
}
