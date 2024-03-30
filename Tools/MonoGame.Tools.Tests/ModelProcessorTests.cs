// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.ContentPipeline
{
    class ModelProcessorTests
    {
        private NodeContent CreateBasicMesh()
        {
            var input = new NodeContent
            {
                Name = "Root",
                Identity = new ContentIdentity("dummy", GetType().Name),
                Transform = Matrix.CreateRotationZ(MathHelper.ToRadians(60)) *
                            Matrix.CreateRotationX(MathHelper.ToRadians(40)) *
                            Matrix.CreateRotationY(MathHelper.ToRadians(50)),
            };

            {
                var mesh = new MeshContent()
                {
                    Name = "Mesh1",
                    Transform = Matrix.Identity,
                };
                var geom = new GeometryContent()
                {
                    Name = "Geom1",
                };
                mesh.Geometry.Add(geom);
                input.Children.Add(mesh);
            }

            {
                var mesh2 = new MeshContent()
                {
                    Name = "Mesh2",
                    Transform = Matrix.Identity,
                };
                mesh2.Positions.Add(new Vector3(0, 0, 0));
                mesh2.Positions.Add(new Vector3(1, 0, 0));
                mesh2.Positions.Add(new Vector3(1, 1, 1));

                var material = new BasicMaterialContent
                {
                    Name = "Material1",
                    Alpha = 0.5f,
                    DiffuseColor = Color.Red.ToVector3(),
                    VertexColorEnabled = true,
                };

                var geom2 = new GeometryContent()
                {
                    Name = "Geom2",
                    Material = material,
                };
                geom2.Vertices.Add(0);
                geom2.Vertices.Add(1);
                geom2.Vertices.Add(2);
                geom2.Indices.Add(0);
                geom2.Indices.Add(1);
                geom2.Indices.Add(2);

                mesh2.Geometry.Add(geom2);
                input.Children.Add(mesh2);
            }

            return input;
        }

        [Test]
        public void BasicMeshTests()
        {
            var input = CreateBasicMesh();

            var processorContext = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            var processor = new ModelProcessor
            {
                RotationX = 10, 
                RotationY = 20,
                RotationZ = 30
            };
            var output = processor.Process(input, processorContext);

            // The transform the processor above is applying to the model.
            var processorXform =    Matrix.CreateRotationZ(MathHelper.ToRadians(30))*
                                    Matrix.CreateRotationX(MathHelper.ToRadians(10))*
                                    Matrix.CreateRotationY(MathHelper.ToRadians(20));

            // Test some basics.
            Assert.NotNull(output);
            Assert.NotNull(output.Meshes);
            Assert.AreEqual(2, output.Meshes.Count);
            Assert.NotNull(output.Bones);
            Assert.AreEqual(3, output.Bones.Count);
            Assert.NotNull(output.Root);
            Assert.AreEqual(output.Root, output.Bones[0]);

            // Stuff to make the tests below cleaner.
            var inputMesh1 = input.Children[0] as MeshContent;
            Assert.NotNull(inputMesh1);
            var inputMesh2 = input.Children[1] as MeshContent;
            Assert.NotNull(inputMesh2);

            // Test the bones.
            Assert.AreEqual("Root", output.Bones[0].Name);
            Assert.That(input.Transform, Is.EqualTo(output.Bones[0].Transform).Using(MatrixComparer.Epsilon));
            Assert.AreEqual("Mesh1", output.Bones[1].Name);
            Assert.That(Matrix.Identity, Is.EqualTo(output.Bones[1].Transform).Using(MatrixComparer.Epsilon));
            Assert.AreEqual("Mesh2", output.Bones[2].Name);
            Assert.That(Matrix.Identity, Is.EqualTo(output.Bones[2].Transform).Using(MatrixComparer.Epsilon));

            // Test the first mesh.
            {
                var mesh = output.Meshes[0];
                Assert.AreEqual("Mesh1", mesh.Name);
                Assert.AreEqual(output.Bones[1], mesh.ParentBone);
                Assert.AreEqual(inputMesh1, mesh.SourceMesh);
                Assert.AreEqual(new BoundingSphere(Vector3.Zero, 0), mesh.BoundingSphere);

                Assert.NotNull(mesh.MeshParts);
                Assert.AreEqual(1, mesh.MeshParts.Count);

                var part = mesh.MeshParts[0];
                Assert.NotNull(part);
                Assert.IsNull(part.IndexBuffer);
                Assert.IsNull(part.VertexBuffer);
                Assert.AreEqual(0, part.NumVertices);
                Assert.AreEqual(0, part.PrimitiveCount);
                Assert.AreEqual(0, part.StartIndex);
                Assert.AreEqual(0, part.VertexOffset);

                Assert.IsAssignableFrom<BasicMaterialContent>(part.Material);
                var material = part.Material as BasicMaterialContent;
                Assert.NotNull(material);
                Assert.IsNotEmpty(material.OpaqueData);
                Assert.IsNull(material.Name);
                Assert.IsNull(material.Identity);
                Assert.IsNull(material.Alpha);
                Assert.IsNull(material.DiffuseColor);
                Assert.IsNull(material.EmissiveColor);
                Assert.IsNull(material.SpecularColor);
                Assert.IsNull(material.SpecularPower);
                Assert.IsNull(material.Texture);
                Assert.IsEmpty(material.Textures);
                Assert.IsTrue(material.OpaqueData.ContainsKey("VertexColorEnabled"));
                Assert.IsNotNull(material.VertexColorEnabled);
                Assert.IsFalse(material.VertexColorEnabled.Value);
            }

            // Test the second mesh.
            {
                var mesh = output.Meshes[1];
                Assert.AreEqual("Mesh2", mesh.Name);
                Assert.AreEqual(output.Bones[2], mesh.ParentBone);
                Assert.AreEqual(inputMesh2, mesh.SourceMesh);
                Assert.That(new BoundingSphere(new Vector3(0.3809527f, 0.5858122f, 0.5115654f), 0.8660253f),
                Is.EqualTo(mesh.BoundingSphere).Using(BoundingSphereComparer.Epsilon));

                Assert.NotNull(mesh.MeshParts);
                Assert.AreEqual(1, mesh.MeshParts.Count);

                var part = mesh.MeshParts[0];
                Assert.NotNull(part);
                Assert.AreEqual(1, part.PrimitiveCount);
                Assert.AreEqual(0, part.StartIndex);
                Assert.AreEqual(0, part.VertexOffset);
                Assert.AreEqual(3, part.NumVertices);
                
                Assert.NotNull(part.IndexBuffer);
                Assert.AreEqual(3, part.IndexBuffer.Count);
                Assert.AreEqual(0, part.IndexBuffer[0]);
                Assert.AreEqual(1, part.IndexBuffer[1]);
                Assert.AreEqual(2, part.IndexBuffer[2]);

                Assert.NotNull(part.VertexBuffer);
                Assert.NotNull(part.VertexBuffer.VertexData);
                var vertexData = part.VertexBuffer.VertexData;
                Assert.AreEqual(36, vertexData.Length);
                var positionArray = ArrayUtil.ConvertTo<Vector3>(vertexData);
                Assert.AreEqual(3, positionArray.Length);
                Assert.AreEqual(Vector3.Transform(new Vector3(0, 0, 0), processorXform), positionArray[0]);
                Assert.AreEqual(Vector3.Transform(new Vector3(1, 0, 0), processorXform), positionArray[1]);
                Assert.AreEqual(Vector3.Transform(new Vector3(1, 1, 1), processorXform), positionArray[2]);

                Assert.IsAssignableFrom<BasicMaterialContent>(part.Material);
                var material = part.Material as BasicMaterialContent;
                Assert.NotNull(material);
                Assert.IsNotEmpty(material.OpaqueData);
                Assert.AreEqual("Material1", material.Name);
                Assert.IsNull(material.Identity);
                Assert.IsTrue(material.OpaqueData.ContainsKey("Alpha"));
                Assert.NotNull(material.Alpha);
                Assert.AreEqual(0.5f, material.Alpha.Value);
                Assert.IsTrue(material.OpaqueData.ContainsKey("DiffuseColor"));
                Assert.NotNull(material.DiffuseColor);
                Assert.AreEqual(Color.Red.ToVector3(), material.DiffuseColor.Value);
                Assert.IsNull(material.EmissiveColor);
                Assert.IsNull(material.SpecularColor);
                Assert.IsNull(material.SpecularPower);
                Assert.IsNull(material.Texture);
                Assert.IsEmpty(material.Textures);
                Assert.IsTrue(material.OpaqueData.ContainsKey("VertexColorEnabled"));
                Assert.IsNotNull(material.VertexColorEnabled);
                Assert.IsFalse(material.VertexColorEnabled.Value);
            }
        }

        [Test]
        public void DefaultEffectTest()
        {
            NodeContent input;
            {
                input = new NodeContent();

                var mesh = new MeshContent()
                {
                    Name = "Mesh1"
                };
                mesh.Positions.Add(new Vector3(0, 0, 0));
                mesh.Positions.Add(new Vector3(1, 0, 0));
                mesh.Positions.Add(new Vector3(1, 1, 1));

                var geom = new GeometryContent();
                geom.Vertices.Add(0);
                geom.Vertices.Add(1);
                geom.Vertices.Add(2);
                geom.Indices.Add(0);
                geom.Indices.Add(1);
                geom.Indices.Add(2);

                geom.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(0), new[]
                {
                    new Vector2(0,0),
                    new Vector2(1,0),
                    new Vector2(1,1),
                });

                var wieghts = new BoneWeightCollection();
                wieghts.Add(new BoneWeight("bone1", 0.5f));
                geom.Vertices.Channels.Add(VertexChannelNames.Weights(0), new[]
                {
                    wieghts, 
                    wieghts, 
                    wieghts
                });

                mesh.Geometry.Add(geom);
                input.Children.Add(mesh);

                var bone1 = new BoneContent { Name = "bone1", Transform = Matrix.CreateTranslation(0,1,0) };
                input.Children.Add(bone1);

                var anim = new AnimationContent()
                {
                    Name = "anim1",
                    Duration = TimeSpan.Zero
                };
                input.Animations.Add(anim.Name, anim);
            }

            var processorContext = new TestProcessorContext(TargetPlatform.Windows, "dummy.xnb");
            var processor = new ModelProcessor
            {
                DefaultEffect = MaterialProcessorDefaultEffect.SkinnedEffect,                
            };

            var output = processor.Process(input, processorContext);

            // TODO: Not sure why, but XNA always returns a BasicMaterialContent 
            // even when we specify SkinnedEffect as the default.  We need to fix
            // the test first before we can enable the assert here.

            //Assert.IsInstanceOf(typeof(SkinnedMaterialContent), output.Meshes[0].MeshParts[0].Material);
        }

        [Test]
        /// <summary>
        /// Test to validate a model with missing normals does not throw an exception using the default ModelProcessor.
        /// </summary>
        public void MissingNormalsTestDefault()
        {
            string level1fbx = "Assets/Models/level1.fbx";
            var importer = new FbxImporter();
            var context = new TestImporterContext("TestObj", "TestBin");
            var nodeContent = importer.Import(level1fbx, context);

            ModelProcessor processor = new ModelProcessor();
            var processorContext = new TestProcessorContext(TargetPlatform.Windows, "level1.xnb");

            ModelContent output = null;
            // Validate that the processor does not throw an exception when normals are missing from the mesh
            Assert.DoesNotThrow(() => output = processor.Process(nodeContent, processorContext));

            // Test some basics.
            Assert.NotNull(output);
            Assert.NotNull(output.Meshes);
        }

        [Test]
        /// <summary>
        /// Test to validate a model with missing normals does not throw an exception using a custom ModelProcessor using MeshHelper.CalculateTangentFrames directly.
        /// </summary>
        public void MissingNormalsTestCustom()
        {
            string level1fbx = "Assets/Models/level1.fbx";
            var importer = new FbxImporter();
            var context = new TestImporterContext("TestObj", "TestBin");
            var nodeContent = importer.Import(level1fbx, context);

            NormalMappingModelProcessor processor = new NormalMappingModelProcessor();
            var processorContext = new TestProcessorContext(TargetPlatform.Windows, "level1_costum.xnb");

            ModelContent output = null;
            // Validate that the custom processor does not throw an exception when normals are missing from the mesh
            Assert.DoesNotThrow(() => output = processor.Process(nodeContent, processorContext));

            // Test some basics.
            Assert.NotNull(output);
            Assert.NotNull(output.Meshes);
        }
    }
}
