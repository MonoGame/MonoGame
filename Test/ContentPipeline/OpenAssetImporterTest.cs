// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MonoGame.Tests.ContentPipeline
{
    class OpenAssetImporterTest
    {
        [Test]
        public void FbxNonSkeletonAnimationTest()
        {
            var importer = new FbxImporter();
            var nodeContent = importer.Import("Assets/Models/NonSkeletonAnimated.fbx", null);
            Assert.AreEqual(1, nodeContent.Animations.Count);
        }
    }
}