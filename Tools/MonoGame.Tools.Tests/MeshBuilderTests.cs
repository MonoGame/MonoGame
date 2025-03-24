// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tests.ContentPipeline
{
    internal class MeshBuilderTests
    {
        [Test]
        public void NodeContentInitializesTransformToIdentity()
        {
            var nodeContent = new NodeContent();
            Assert.AreEqual(Matrix.Identity, nodeContent.Transform);
        }

        private readonly BasicMaterialContent material1 = new BasicMaterialContent
        {
            Name = "Material1",
            Alpha = 0.5f,
            DiffuseColor = Color.Red.ToVector3(),
            VertexColorEnabled = true,
        };

        private readonly BasicMaterialContent material2 = new BasicMaterialContent
        {
            Name = "Material2",
            Alpha = 0.5f,
            DiffuseColor = Color.Blue.ToVector3(),
            VertexColorEnabled = true,
        };

        private static MeshContent CreateBasicMesh(MaterialContent material)
        {
            var builder = MeshBuilder.StartMesh("Mesh1");
            builder.SetMaterial(material);
            CreatePositions(builder);
            AddVertices(builder);
            return builder.FinishMesh();
        }

        private static void CreatePositions(MeshBuilder builder)
        {
            builder.CreatePosition(new Vector3(0, 0, 0));
            builder.CreatePosition(new Vector3(1, 0, 0));
            builder.CreatePosition(new Vector3(1, 1, 1));
        }

        private static void AddVertices(MeshBuilder builder)
        {
            builder.AddTriangleVertex(0);
            builder.AddTriangleVertex(1);
            builder.AddTriangleVertex(2);
        }

        private static void AddVertices(MeshBuilder builder, int channelIndex, object[] channelData, int indexOffset = 0)
        {
            builder.SetVertexChannelData(channelIndex, channelData[0]);
            builder.AddTriangleVertex(0 + indexOffset);
            builder.SetVertexChannelData(channelIndex, channelData[1]);
            builder.AddTriangleVertex(1 + indexOffset);
            builder.SetVertexChannelData(channelIndex, channelData[2]);
            builder.AddTriangleVertex(2 + indexOffset);
        }

        private MeshContent CreateTwoMaterialsMesh()
        {
            var builder = MeshBuilder.StartMesh("Mesh2");

            var firstPos = builder.CreatePosition(new Vector3(0, 0, 0));
            var secondPos =  builder.CreatePosition(new Vector3(1, 0, 0));
            var thirdPos = builder.CreatePosition(new Vector3(1, 1, 1));

            builder.SetMaterial(material1);

            builder.AddTriangleVertex(firstPos);
            builder.AddTriangleVertex(secondPos);
            builder.AddTriangleVertex(thirdPos);

            builder.SetMaterial(material2);

            builder.AddTriangleVertex(firstPos);
            builder.AddTriangleVertex(thirdPos);
            builder.AddTriangleVertex(secondPos);

            return builder.FinishMesh();
        }

        [Test]
        public void BasicMeshBuilderTest()
        {
            var output = CreateBasicMesh(material1);

            Assert.NotNull(output);
            Assert.NotNull(output.Geometry);
            Assert.NotNull(output.Positions);

            Assert.AreEqual(new Vector3(0, 0, 0), output.Positions[0]);
            Assert.AreEqual(new Vector3(1, 0, 0), output.Positions[1]);
            Assert.AreEqual(new Vector3(1, 1, 1), output.Positions[2]);

            Assert.AreEqual(1, output.Geometry.Count);

            Assert.AreEqual(0, output.Geometry[0].Indices[0]);
            Assert.AreEqual(1, output.Geometry[0].Indices[1]);
            Assert.AreEqual(2, output.Geometry[0].Indices[2]);

            Assert.AreEqual(0, output.Geometry[0].Vertices.PositionIndices[0]);
            Assert.AreEqual(1, output.Geometry[0].Vertices.PositionIndices[1]);
            Assert.AreEqual(2, output.Geometry[0].Vertices.PositionIndices[2]);

            Assert.AreEqual(new Vector3(0, 0, 0), output.Geometry[0].Vertices.Positions[0]);
            Assert.AreEqual(new Vector3(1, 0, 0), output.Geometry[0].Vertices.Positions[1]);
            Assert.AreEqual(new Vector3(1, 1, 1), output.Geometry[0].Vertices.Positions[2]);

            //Check if normals are generated
            Assert.NotNull(output.Geometry[0].Vertices.Channels[VertexChannelNames.Normal(0)]);

            Assert.AreEqual(material1, output.Geometry[0].Material);
            Assert.AreEqual(Matrix.Identity, output.Transform);

            Assert.AreEqual(3, output.Positions.Count);
            Assert.AreEqual("Mesh1", output.Name);
        }

        [Test]
        public void TwoMaterialsMeshBuilderTest()
        {
            var output = CreateTwoMaterialsMesh();

            Assert.NotNull(output);
            Assert.NotNull(output.Geometry);
            Assert.NotNull(output.Positions);

            Assert.AreEqual(2, output.Geometry.Count);

            Assert.AreEqual(0, output.Geometry[0].Indices[0]);
            Assert.AreEqual(1, output.Geometry[0].Indices[1]);
            Assert.AreEqual(2, output.Geometry[0].Indices[2]);
            Assert.AreSame(material1, output.Geometry[0].Material);

            Assert.AreEqual(0, output.Geometry[1].Indices[0]);
            Assert.AreEqual(1, output.Geometry[1].Indices[1]);
            Assert.AreEqual(2, output.Geometry[1].Indices[2]);
            Assert.AreSame(material2, output.Geometry[1].Material);

            Assert.AreEqual(3, output.Positions.Count);
            Assert.AreEqual("Mesh2", output.Name);
        }

        [Test]
        public void CannotCallCreateMethodsAfterAddingVertex()
        {
            var builder = MeshBuilder.StartMesh("Mesh1");
            builder.CreatePosition(new Vector3(0, 0, 0));
            builder.AddTriangleVertex(0);
            Assert.Throws<InvalidOperationException>(
                () => builder.CreatePosition(new Vector3(1, 2, 3)));
            Assert.Throws<InvalidOperationException>(
                () => builder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0)));
        }

        [Test]
        public void TestDefaultPropertyValues()
        {
            var mb = MeshBuilder.StartMesh("Test");
            Assert.IsFalse(mb.MergeDuplicatePositions);
            Assert.IsFalse(mb.SwapWindingOrder);
            Assert.AreEqual(0f, mb.MergePositionTolerance);
        }

        [Test]
        public void CreateMeshWithoutMaterial()
        {
            var mb = MeshBuilder.StartMesh("Test");

            CreatePositions(mb);
            AddVertices(mb);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(1, mesh.Geometry.Count);
            var geom = mesh.Geometry[0];
            Assert.IsNull(geom.Material);
        }

        [Test]
        public void CreateEmptyMesh()
        {
            const string name = "Test";
            var mb = MeshBuilder.StartMesh(name);
            var mesh = mb.FinishMesh();
            Assert.AreEqual(name, mesh.Name);
            Assert.IsEmpty(mesh.Positions);
            Assert.IsEmpty(mesh.Geometry);
            Assert.IsEmpty(mesh.Animations);
            Assert.IsEmpty(mesh.Children);
            Assert.IsEmpty(mesh.OpaqueData);
            Assert.AreEqual(Matrix.Identity, mesh.AbsoluteTransform);
            Assert.AreEqual(Matrix.Identity, mesh.Transform);
            Assert.IsNull(mesh.Parent);
            Assert.IsNull(mesh.Identity);
        }

        [Test]
        public void SetMaterialDoesNotClearChannels()
        {
            var mb = MeshBuilder.StartMesh("Test");
            var mat = new BasicMaterialContent();

            mb.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
            mb.SetMaterial(mat);
            Assert.DoesNotThrow(() => mb.SetVertexChannelData(0, Vector2.Zero));
        }

        [Test]
        public void SetMaterialOrOpaqueDataCreatesNewGeometry()
        {
            var mb = MeshBuilder.StartMesh("Test");
            CreatePositions(mb);
            AddVertices(mb);

            var d1 = new OpaqueDataDictionary {{"Name", "Data1"}};
            mb.SetOpaqueData(d1);
            AddVertices(mb);
            
            mb.SetMaterial(material1);
            AddVertices(mb);

            var d2 = new OpaqueDataDictionary {{"Name", "Data2"}};
            mb.SetOpaqueData(d2);
            AddVertices(mb);

            mb.SetMaterial(material2);
            AddVertices(mb);

            // this one won't get added because we don't add any vertices anymore
            var d3 = new OpaqueDataDictionary {{"Name", "Data3"}};
            mb.SetOpaqueData(d3);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(5, mesh.Geometry.Count);

            Assert.IsNull(mesh.Geometry[0].Material);
            Assert.IsEmpty(mesh.Geometry[0].OpaqueData);

            Assert.IsNull(mesh.Geometry[1].Material);
            Assert.AreEqual(d1, mesh.Geometry[1].OpaqueData);
            Assert.AreNotSame(d1, mesh.Geometry[1].OpaqueData);

            Assert.AreSame(material1, mesh.Geometry[2].Material);
            Assert.AreEqual(d1, mesh.Geometry[2].OpaqueData);
            Assert.AreNotSame(d1, mesh.Geometry[2].OpaqueData);

            Assert.AreSame(material1, mesh.Geometry[3].Material);
            Assert.AreEqual(d2, mesh.Geometry[3].OpaqueData);
            Assert.AreNotSame(d2, mesh.Geometry[3].OpaqueData);

            Assert.AreSame(material2, mesh.Geometry[4].Material);
            Assert.AreEqual(d2, mesh.Geometry[4].OpaqueData);
            Assert.AreNotSame(d2, mesh.Geometry[4].OpaqueData);
        }

        [Test]
        public void DuplicateGeometryIsOnlyAddedOnce()
        {
            var mb = MeshBuilder.StartMesh("Test");
            CreatePositions(mb);

            mb.SetMaterial(material1);
            var d1 = new OpaqueDataDictionary {{"Name", "Data1"}};
            mb.SetOpaqueData(d1);
            AddVertices(mb);

            mb.SetMaterial(material1);
            mb.SetOpaqueData(d1);
            AddVertices(mb);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(1, mesh.Geometry.Count);
            Assert.AreSame(material1, mesh.Geometry[0].Material);
        }

        [Test]
        public void EmptyGeometryIsNotAdded()
        {
            var mb = MeshBuilder.StartMesh("Test");
            CreatePositions(mb);

            var d2 = new OpaqueDataDictionary {{"Name", "Data2"}};
            mb.SetOpaqueData(d2);
            var d1 = new OpaqueDataDictionary {{"Name", "Data1"}};
            mb.SetOpaqueData(d1);
            mb.SetMaterial(material1);
            AddVertices(mb);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(1, mesh.Geometry.Count);
            Assert.AreEqual(d1, mesh.Geometry[0].OpaqueData);
            Assert.AreEqual(material1, mesh.Geometry[0].Material);
        }

        [Test]
        public void SetChannelData()
        {
            var mb = MeshBuilder.StartMesh("Test");
            mb.SetMaterial(material1);

            var channelIndex = mb.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
            var p1 = mb.CreatePosition(0f, 0f, 0f);
            var p2 = mb.CreatePosition(1f, 0f, 0f);
            var p3 = mb.CreatePosition(0f, 1f, 0f);
            var t1 = Vector2.Zero;
            var t2 = Vector2.UnitX;
            var t3 = Vector2.UnitY;

            // this should be overwritten by the next call
            mb.SetVertexChannelData(channelIndex, t3);
            mb.SetVertexChannelData(channelIndex, t1);

            // setting the material here should not reset the channel data
            mb.SetMaterial(material2);

            mb.AddTriangleVertex(p1);
            mb.SetVertexChannelData(channelIndex, t2);
            mb.AddTriangleVertex(p2);
            mb.SetVertexChannelData(channelIndex, t3);
            mb.AddTriangleVertex(p3);

            var mesh = mb.FinishMesh();
            var geom = mesh.Geometry[0];
            var texChannel = geom.Vertices.Channels[channelIndex];
            Assert.AreEqual(t1, texChannel[0]);
            Assert.AreEqual(t2, texChannel[1]);
            Assert.AreEqual(t3, texChannel[2]);
        }

        // XNA sets the default value for the channel data for a value type channel
        // and to null for a reference type. This will cause a NullReferenceException when you try 
        // to finish the mesh. MonoGame only allows value types.
        [Test]
        public void MissingChannelData()
        {
            var mb = MeshBuilder.StartMesh("Test");
            mb.SetMaterial(material1);

            var channelIndex = mb.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
            var p1 = mb.CreatePosition(0f, 0f, 0f);
            var p2 = mb.CreatePosition(1f, 0f, 0f);
            var p3 = mb.CreatePosition(0f, 1f, 0f);

            mb.AddTriangleVertex(p1);
            mb.AddTriangleVertex(p2);
            mb.AddTriangleVertex(p3);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(default(Vector2), mesh.Geometry[0].Vertices.Channels[channelIndex][0]);
            Assert.AreEqual(default(Vector2), mesh.Geometry[0].Vertices.Channels[channelIndex][1]);
            Assert.AreEqual(default(Vector2), mesh.Geometry[0].Vertices.Channels[channelIndex][2]);
        }

        [Test]
        public void OnlyUseOnce()
        {
            var mb = MeshBuilder.StartMesh("Test");
            var mat = new BasicMaterialContent();

            mb.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));
            mb.CreatePosition(0f, 0f, 0f);

            var mesh = mb.FinishMesh();
            Assert.DoesNotThrow(() => mb.SetMaterial(mat));
            Assert.DoesNotThrow(() => mb.SetVertexChannelData(0, Vector2.Zero));
            Assert.AreSame(mesh, mb.FinishMesh());

            Assert.Throws<InvalidOperationException>(() => mb.CreatePosition(1f, 2f, 3f));
            Assert.Throws<InvalidOperationException>(() => mb.AddTriangleVertex(0));
            Assert.Throws<InvalidOperationException>(() => mb.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(1)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DoNotMergePositionsUntilFinish(bool value)
        {
            var mb = MeshBuilder.StartMesh("Test");
            mb.SetMaterial(material1);
            mb.MergePositionTolerance = 5f;

            var i = mb.CreatePosition(new Vector3(0f, 0f, 0f));
            // this should be out of range.
            Assert.AreNotEqual(i, mb.CreatePosition(new Vector3(4f, 4f, 4f)));
            Assert.AreNotEqual(i, mb.CreatePosition(new Vector3(-1f, -1f, -1f)));

            mb.MergeDuplicatePositions = value;
            var mesh = mb.FinishMesh();
            Assert.AreEqual(value ? 2 : 3, mesh.Positions.Count);
        }

        [Test]
        public void MergeVertices()
        {
            var mb = MeshBuilder.StartMesh("Test");
            mb.MergeDuplicatePositions = true;
            mb.MergePositionTolerance = 0.2f;
            mb.SetMaterial(material1);

            // add positions twice, they should get merged too
            CreatePositions(mb);
            CreatePositions(mb);

            var channel = mb.CreateVertexChannel<float>(VertexChannelNames.TextureCoordinate(0));
            object[] channelData1 = { 10f, 11f, 12f };
            object[] channelData2 = { 20f, 21f, 22f };

            var currentChannelData = channelData1;

            const int iterations = 10;
            for (var i = 0; i < iterations; i++)
            {
                if (currentChannelData == channelData1)
                {
                    currentChannelData = channelData2;
                    AddVertices(mb, channel, currentChannelData);
                }
                else
                {
                    currentChannelData = channelData1;
                    AddVertices(mb, channel, currentChannelData, 3);
                }
            }

            var mesh = mb.FinishMesh();
            var geom = mesh.Geometry[0];
            Assert.AreEqual(3 * iterations, geom.Indices.Count);
            Assert.AreEqual(6, geom.Vertices.VertexCount);
        }

        [Test]
        public void RemoveVertices()
        {
            MeshContent output;
            
            output = CreateBasicMesh(material1);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveAt(-1));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveAt(3));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveRange(-1,1));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveRange(0,-1));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveRange(0,4));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveRange(3, 1));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveRange(4, 0));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);

            // NOTE: XNA seems to have a bug where you can specifiy one pased
            // the last index when removing a range of zero.  We fixed this in MG.
#if !XNA
            Assert.Throws<ArgumentOutOfRangeException>(() => output.Geometry[0].Vertices.RemoveRange(3, 0));
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
#endif

            // RemoveAt(1)
            output = CreateBasicMesh(material1);
            output.Geometry[0].Vertices.RemoveAt(1);
            Assert.AreEqual(2, output.Geometry[0].Vertices.VertexCount);
            Assert.AreEqual(2, output.Geometry[0].Vertices.Positions.Count);
            Assert.AreEqual(2, output.Geometry[0].Vertices.PositionIndices.Count);
            Assert.AreEqual(2, output.Geometry[0].Vertices.Channels[0].Count);
            Assert.AreEqual(new Vector3(0, 0, 0), output.Geometry[0].Vertices.Positions[0]);
            Assert.AreEqual(new Vector3(1, 1, 1), output.Geometry[0].Vertices.Positions[1]);
            Assert.AreEqual(0, output.Geometry[0].Vertices.PositionIndices[0]);
            Assert.AreEqual(2, output.Geometry[0].Vertices.PositionIndices[1]);

            // RemoveRange(0, 0)
            output = CreateBasicMesh(material1);
            output.Geometry[0].Vertices.RemoveRange(0, 0);
            Assert.AreEqual(3, output.Geometry[0].Vertices.VertexCount);
            Assert.AreEqual(3, output.Geometry[0].Vertices.Positions.Count);
            Assert.AreEqual(3, output.Geometry[0].Vertices.PositionIndices.Count);
            Assert.AreEqual(3, output.Geometry[0].Vertices.Channels[0].Count);
            Assert.AreEqual(new Vector3(0, 0, 0), output.Geometry[0].Vertices.Positions[0]);
            Assert.AreEqual(new Vector3(1, 0, 0), output.Geometry[0].Vertices.Positions[1]);
            Assert.AreEqual(new Vector3(1, 1, 1), output.Geometry[0].Vertices.Positions[2]);
            Assert.AreEqual(0, output.Geometry[0].Vertices.PositionIndices[0]);
            Assert.AreEqual(1, output.Geometry[0].Vertices.PositionIndices[1]);
            Assert.AreEqual(2, output.Geometry[0].Vertices.PositionIndices[2]);

            // RemoveRange(0, 1)
            output = CreateBasicMesh(material1);
            output.Geometry[0].Vertices.RemoveRange(0, 1);
            Assert.AreEqual(2, output.Geometry[0].Vertices.VertexCount);
            Assert.AreEqual(2, output.Geometry[0].Vertices.Positions.Count);
            Assert.AreEqual(2, output.Geometry[0].Vertices.PositionIndices.Count);
            Assert.AreEqual(2, output.Geometry[0].Vertices.Channels[0].Count);
            Assert.AreEqual(new Vector3(1, 0, 0), output.Geometry[0].Vertices.Positions[0]);
            Assert.AreEqual(new Vector3(1, 1, 1), output.Geometry[0].Vertices.Positions[1]);
            Assert.AreEqual(1, output.Geometry[0].Vertices.PositionIndices[0]);
            Assert.AreEqual(2, output.Geometry[0].Vertices.PositionIndices[1]);

            // RemoveRange(1, 2)
            output = CreateBasicMesh(material1);
            output.Geometry[0].Vertices.RemoveRange(1, 2);
            Assert.AreEqual(1, output.Geometry[0].Vertices.VertexCount);
            Assert.AreEqual(1, output.Geometry[0].Vertices.Positions.Count);
            Assert.AreEqual(1, output.Geometry[0].Vertices.PositionIndices.Count);
            Assert.AreEqual(1, output.Geometry[0].Vertices.Channels[0].Count);
            Assert.AreEqual(new Vector3(0, 0, 0), output.Geometry[0].Vertices.Positions[0]);
            Assert.AreEqual(0, output.Geometry[0].Vertices.PositionIndices[0]);

            // RemoveRange(0, 3)
            output = CreateBasicMesh(material1);
            output.Geometry[0].Vertices.RemoveRange(0, 3);
            Assert.AreEqual(0, output.Geometry[0].Vertices.VertexCount);
            Assert.AreEqual(0, output.Geometry[0].Vertices.Positions.Count);
            Assert.AreEqual(0, output.Geometry[0].Vertices.PositionIndices.Count);
            Assert.AreEqual(0, output.Geometry[0].Vertices.Channels[0].Count);
        }
    }
}
