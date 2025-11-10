// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Framework.Content.Pipeline.Builder;


namespace MonoGame.Tests.ContentPipeline
{
    class TestAsset
    {
    }

    class TestContent
    {
    }

    class TestImporter : ContentImporter<TestAsset>
    {
        public override TestAsset Import(string filename, ContentImporterContext context)
        {
            var asset = new TestAsset();
            return asset;
        }
    }

    class TestProcessor : ContentProcessor<TestAsset, TestContent>
    {
        public override TestContent Process(TestAsset input, ContentProcessorContext context)
        {
            var content = new TestContent();

            return content;
        }
    }

    class Builder : ContentBuilder
    {
        public override IContentCollection GetContentCollection()
        {
            var content = new ContentCollection();

            content.Include<RegexRule>(".");

            // This is not content.
            content.Exclude<WildcardRule>("ReferenceImages/**/*.*");

            // These all normally fail per design.
            content.Exclude<WildcardRule>("**/04_ExcludingPublicMembers.xml");
            content.Exclude<WildcardRule>("**/08_AllowNull.xml");
            content.Exclude<WildcardRule>("**/17_ExternalReferences.xml");
            content.Exclude<WildcardRule>("**/23_GetterOnlyPolymorphicArrayProperties.xml");            
            content.Exclude<WildcardRule>("**/bark_mono_44hz_32bit.wav");
            content.Exclude<WildcardRule>("**/bark_mono_88hz_16bit.wav");
            content.Exclude<WildcardRule>("**/rock_loop_stereo.mp3");
            content.Exclude<WildcardRule>("**/rock_loop_stereo.wma");
            content.Exclude("Textures/rgbf.tif");
            content.Exclude("Models/Dude/dude.fbx");
            content.Exclude("Models/level1.fbx");

            // Required for this to build.
            content.Include("Effects/DefinesTest.fx",
                new EffectImporter(),
                new EffectProcessor() { Defines = "MACRO_DEFINE_TEST=3" } ); 

            //content.Include(@"Assets/*.*", new TestImporter(), new TestProcessor());

            return content;
        }
    }

    [TestFixture]
    public class BuilderTest
    {
        string MakePath(string path, string append = null)
        {
            if (append != null)
                path = Path.Combine(path, append);

            return FileHelper.NormalizeDirectorySeparators(path);
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
            Assert.AreEqual(MakePath(Directory.GetCurrentDirectory(), "Content"), args.RootedSourceDirectory);
            Assert.AreEqual(MakePath("bin/Content"), args.OutputDirectory);
            Assert.AreEqual(MakePath(Directory.GetCurrentDirectory(), "bin\\Content"), args.RootedOutputDirectory);
            Assert.AreEqual(MakePath("obj/Content"), args.IntermediateDirectory);
            Assert.AreEqual(MakePath(Directory.GetCurrentDirectory(), "obj\\Content"), args.RootedIntermediateDirectory);

            args = ContentBuilderParams.Parse("build", "-s", "C:/This/Does/Not/Exist");
            Assert.AreEqual(MakePath("C:/This/Does/Not/Exist"), args.SourceDirectory);

            args = ContentBuilderParams.Parse(
                "build",
                "-s", MakePath(Directory.GetCurrentDirectory(), "../Some/Folder"),
                "-o", MakePath(Directory.GetCurrentDirectory(), "Other/Folder"),
                "-i", MakePath(Directory.GetCurrentDirectory(), "Folder")
            );
            Assert.AreEqual(MakePath("../Some/Folder"), args.SourceDirectory);
            Assert.AreEqual(MakePath("Other/Folder"), args.OutputDirectory);
            Assert.AreEqual("Folder", args.IntermediateDirectory);

            args = ContentBuilderParams.Parse("server");
            Assert.AreEqual(ContentBuilderMode.Server, args.Mode);
        }

        [Test]
        public void BuildTest()
        {
            var builder = new Builder();
            builder.Run(new ContentBuilderParams
            {
                CompressContent = false,
                GraphicsProfile = Microsoft.Xna.Framework.Graphics.GraphicsProfile.HiDef,
                LogLevel = LogLevel.Debug,
                Mode = ContentBuilderMode.Builder,
                Platform = TargetPlatform.Windows,
                Rebuild = true,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                SourceDirectory = "Assets",
                IntermediateDirectory = "BuilderIntermediateDir",
                OutputDirectory = "BuilderOutputDir"
            });

            var failures = builder.FailedToBuild;
            Assert.AreEqual(0, failures);
        }
    }
}
