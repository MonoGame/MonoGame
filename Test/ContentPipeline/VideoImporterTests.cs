// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using NUnit.Framework;
using System.IO;

namespace MonoGame.Tests.ContentPipeline
{
    class VideoImporterTests
    {
        const string intermediateDirectory = "TestObj";
        const string outputDirectory = "TestBin";

        void ImportStandard(string filename)
        {
            var importer = new WmvImporter();
            var context = new TestImporterContext(intermediateDirectory, outputDirectory);
            var content = importer.Import(filename, context);
            Assert.NotNull(content);

            // Clean-up the directories it may have produced, ignoring DirectoryNotFound exceptions
            try
            {
                Directory.Delete(intermediateDirectory, true);
                Directory.Delete(outputDirectory, true);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        [Test]
        public void ImportMp4()
        {
            ImportStandard("Assets/Videos/SampleVideo_360x240_5s.mp4");
        }

        [Test]
        public void ImportMkv()
        {
            ImportStandard("Assets/Videos/SampleVideo_360x240_5s.mkv");
        }

        [Test]
        public void ImportFlv()
        {
            ImportStandard("Assets/Videos/SampleVideo_360x240_5s.flv");
        }

        [Test]
        public void Import3gp()
        {
            ImportStandard("Assets/Videos/SampleVideo_320x240_5s.3gp");
        }
    }
}
