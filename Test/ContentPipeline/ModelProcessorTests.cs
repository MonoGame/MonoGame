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
    class ModelProcessorTests
    {
        class ProcessorContext : ContentProcessorContext
        {
            private readonly TargetPlatform _targetPlatform;
            private readonly string _outputFilename;

            public ProcessorContext(TargetPlatform targetPlatform,
                                        string outputFilename)
            {
                _targetPlatform = targetPlatform;
                _outputFilename = outputFilename;
            }

            public override string BuildConfiguration
            {
                get { throw new NotImplementedException(); }
            }

            public override string IntermediateDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override ContentBuildLogger Logger
            {
                get { throw new NotImplementedException(); }
            }

            public override string OutputDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override string OutputFilename
            {
                get { return _outputFilename; }
            }

            public override OpaqueDataDictionary Parameters
            {
                get { throw new NotImplementedException(); }
            }

            public override TargetPlatform TargetPlatform
            {
                get { return _targetPlatform; }
            }

            public override GraphicsProfile TargetProfile
            {
                get { throw new NotImplementedException(); }
            }

            public override void AddDependency(string filename)
            {
            }

            public override void AddOutputFile(string filename)
            {
            }

            public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
            {
                return default(TOutput);
            }

            public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
            {
                throw new NotImplementedException();
            }

            public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
            {
                // MaterialProcessor essentially transforms its
                // input and returns it... not a copy.  So this
                // seems like a reasonable shortcut for testing.
                if (typeof(TOutput) == typeof(MaterialContent) && typeof(TInput).IsAssignableFrom(typeof(MaterialContent)))
                    return (TOutput)((object)input);

                throw new NotImplementedException();
            }
        }

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

            var processorContext = new ProcessorContext(TargetPlatform.Windows, "dummy.xnb");
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
    }
}