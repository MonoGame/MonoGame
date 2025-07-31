// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace MonoGame.Tests.ContentPipeline
{
    class OpenAssetImporterTests
    {
        [Test]
        public void Arguments()
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            var importer = new OpenAssetImporter();

            Assert.Throws<ArgumentNullException>(() => importer.Import(null, context));
            Assert.Throws<FileNotFoundException>(() => importer.Import("does_not_exist", context));
            Assert.Throws<ArgumentNullException>(() => importer.Import("file", null));
        }

        [Test]
        [TestCase("Assets/Models/MeshAnimatedCharacter.glb", 26, 0, 6)]
        [TestCase("Assets/Models/MeshAnimatedCharacter.fbx", 28, 0, 6)]
        [TestCase("Assets/Models/SkeletonAnimated.fbx", 25, 10, 2)]
        [TestCase("Assets/Models/NonSkeletonAnimated.fbx", 1, 0, 1)]
        public void MeshAnimatedCharacterImport (string file, int expectAnimations, int expectedBones, int expectedMeshes)
        {
            static int CountNodesOfType<T>(NodeContent node)
            {
                int count = 0;
                if (node is T)
                    count++;
                foreach (var child in node.Children)
                {
                    if (child is NodeContent childNode)
                        count += CountNodesOfType<T>(childNode);
                }
                return count;
            }

            static int CountAnimations(NodeContent node)
            {
                int count = 0;
                foreach (var child in node.Children)
                {
                    if (child is NodeContent childNode)
                        count += CountAnimations(childNode);
                }
                return count + node.Animations.Count;
            }

            var context = new TestImporterContext("TestObj", "TestBin");
            var importer = new OpenAssetImporter();

            var nodeContent = importer.Import(file, context);
            
            Assert.AreEqual(expectAnimations, CountAnimations(nodeContent));
            Assert.AreEqual(expectedBones, CountNodesOfType<BoneContent>(nodeContent));
            Assert.AreEqual(expectedMeshes, CountNodesOfType<MeshContent>(nodeContent));
        }

        [Test]
        public void BlenderTests()
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            var importer = new OpenAssetImporter();

            var nodeContent = importer.Import("Assets/Models/Box.blend", context);

            Assert.NotNull(nodeContent);
            Assert.AreEqual("Cube", nodeContent.Name);
            Assert.AreEqual(0, nodeContent.Children.Count);
            Assert.AreEqual(Matrix.Identity, nodeContent.Transform);
            Assert.AreEqual(Matrix.Identity, nodeContent.AbsoluteTransform);
            Assert.NotNull(nodeContent.Parent);
            Assert.AreEqual("<BlenderRoot>", nodeContent.Parent.Name);

            var meshContent = nodeContent as MeshContent;
            Assert.NotNull(meshContent);
            Assert.AreEqual(1, meshContent.Geometry.Count);
            Assert.AreEqual(0, meshContent.Animations.Count);
            Assert.AreEqual(28, meshContent.Positions.Count);

            var geometry = meshContent.Geometry[0];
            Assert.IsNull(geometry.Name);
            Assert.AreEqual(108, geometry.Indices.Count);
            Assert.AreEqual(28, geometry.Vertices.VertexCount);

            Assert.IsNotNull(geometry.Material);
            Assert.AreEqual("Material", geometry.Material.Name);
            Assert.AreEqual(5, geometry.Material.OpaqueData.Count);
            Assert.AreEqual(new Vector3(1.65732033E-07f, 1, 0), geometry.Material.OpaqueData["DiffuseColor"]);
            Assert.AreEqual(Vector3.Zero, geometry.Material.OpaqueData["AmbientColor"]);
            Assert.AreEqual(Vector3.One, geometry.Material.OpaqueData["ReflectiveColor"]);
            Assert.AreEqual(Vector3.One, geometry.Material.OpaqueData["SpecularColor"]);
            Assert.AreEqual(50.0f, geometry.Material.OpaqueData["Shininess"]);
        }
    }
}
