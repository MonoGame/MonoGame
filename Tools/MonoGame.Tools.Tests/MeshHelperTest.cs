// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    public class MeshHelperTest
    {
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


        [Test]
        public void TestMergePositions()
        {
            var mb = MeshBuilder.StartMesh("Test");
            mb.SetMaterial(material1);
            mb.CreatePosition(new Vector3(0f, 0f, 0f));
            mb.CreatePosition(new Vector3(4f, 4f, 4f));
            mb.CreatePosition(new Vector3(-1f, -1f, -1f));

            mb.AddTriangleVertex(0);
            mb.AddTriangleVertex(1);
            mb.AddTriangleVertex(2);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(3, mesh.Positions.Count);

            MeshHelper.MergeDuplicatePositions(mesh, 5f);
            Assert.AreEqual(2, mesh.Positions.Count);

            var geom = mesh.Geometry[0];
            Assert.AreEqual(3, geom.Vertices.Positions.Count);
            Assert.AreEqual(3, geom.Vertices.PositionIndices.Count);
            Assert.AreEqual(0, geom.Vertices.PositionIndices[0]);
            Assert.AreEqual(1, geom.Vertices.PositionIndices[1]);
            Assert.AreEqual(0, geom.Vertices.PositionIndices[2]);
        }

[Test]
public void TestNormalGeneration()
{
    var mb = MeshBuilder.StartMesh("Test");

    // A 5x5 grid of points around the origin, with the centerpoint raised a bit.
    mb.CreatePosition(new Vector3(-2, 0, -2));
    mb.CreatePosition(new Vector3(-1, 0, -2));
    mb.CreatePosition(new Vector3( 0, 0, -2));
    mb.CreatePosition(new Vector3(+1, 0, -2));
    mb.CreatePosition(new Vector3(+2, 0, -2));
    mb.CreatePosition(new Vector3(-2, 0, -1));
    mb.CreatePosition(new Vector3(-1, 0, -1));
    mb.CreatePosition(new Vector3( 0, 0, -1));
    mb.CreatePosition(new Vector3(+1, 0, -1));
    mb.CreatePosition(new Vector3(+2, 0, -1));
    mb.CreatePosition(new Vector3(-2, 0,  0));
    mb.CreatePosition(new Vector3(-1, 0,  0));
    mb.CreatePosition(new Vector3( 0,0.5f,0)); // The bump in the center.
    mb.CreatePosition(new Vector3(+1, 0,  0));
    mb.CreatePosition(new Vector3(+2, 0,  0));
    mb.CreatePosition(new Vector3(-2, 0, +1));
    mb.CreatePosition(new Vector3(-1, 0, +1));
    mb.CreatePosition(new Vector3( 0, 0, +1));
    mb.CreatePosition(new Vector3(+1, 0, +1));
    mb.CreatePosition(new Vector3(+2, 0, +1));
    mb.CreatePosition(new Vector3(-2, 0, +2));
    mb.CreatePosition(new Vector3(-1, 0, +2));
    mb.CreatePosition(new Vector3( 0, 0, +2));
    mb.CreatePosition(new Vector3(+1, 0, +2));
    mb.CreatePosition(new Vector3(+2, 0, +2));

    // Create triangles like this, starting with the quad in the bottom left, then
    // bottom right, then top left, then top right, doing the bottom/right-most triangle
    // first in each quad.
    // 20--21--22--23--24
    // | / | / | / | / |
    // 15--16--17--18--19
    // | / | / | / | / |
    // 10--11--12--13--14
    // | / | / | / | / |
    // 5---6---7---8---9
    // | / | / | / | / |
    // 0---1---2---3---4
    // In this diagram, horizontal is the x-axis, vertical is the z-axis, and the y-axis
    // comes out of the screen.
    void AddTriangle(int a, int b, int c) { mb.AddTriangleVertex(a); mb.AddTriangleVertex(b); mb.AddTriangleVertex(c); }
    AddTriangle( 0,  1,  6); AddTriangle( 0,  6,  5);
    AddTriangle( 1,  2,  7); AddTriangle( 1,  7,  6);
    AddTriangle( 2,  3,  8); AddTriangle( 2,  8,  7);
    AddTriangle( 3,  4,  9); AddTriangle( 3,  9,  8);

    AddTriangle( 5,  6, 11); AddTriangle( 5, 11, 10);
    AddTriangle( 6,  7, 12); AddTriangle( 6, 12, 11);
    AddTriangle( 7,  8, 13); AddTriangle( 7, 13, 12);
    AddTriangle( 8,  9, 14); AddTriangle( 8, 14, 13);
            
    AddTriangle(10, 11, 16); AddTriangle(10, 16, 15);
    AddTriangle(11, 12, 17); AddTriangle(11, 17, 16);
    AddTriangle(12, 13, 18); AddTriangle(12, 18, 17);
    AddTriangle(13, 14, 19); AddTriangle(13, 19, 18);

    AddTriangle(15, 16, 21); AddTriangle(15, 21, 20);
    AddTriangle(16, 17, 22); AddTriangle(16, 22, 21);
    AddTriangle(17, 18, 23); AddTriangle(17, 23, 22);
    AddTriangle(18, 19, 24); AddTriangle(18, 24, 23);

    MeshContent meshContent = mb.FinishMesh();

    // The MergeDuplicateVertices code has reordered these (without merging anything,
    // since they're all different positions), and the new numbering is the order they
    // appeared in the actual indexing:
    // 21--20--22--23--24
    // | / | / | / | / |
    // 16--15--17--18--19
    // | / | / | / | / |
    // 11--10--12--13--14
    // | / | / | / | / |
    // 3---2---5---7---9
    // | / | / | / | / |
    // 0---1---4---6---8

    // At this point, the positions are now reordered, but we can assert that their positions
    // are right.
    var vertexPositions = meshContent.Geometry[0].Vertices.Positions;
    Assert.AreEqual(new Vector3(-2, 0, -2), vertexPositions[ 0]);
    Assert.AreEqual(new Vector3(-1, 0, -2), vertexPositions[ 1]);
    Assert.AreEqual(new Vector3(-1, 0, -1), vertexPositions[ 2]);
    Assert.AreEqual(new Vector3(-2, 0, -1), vertexPositions[ 3]);
    Assert.AreEqual(new Vector3( 0, 0, -2), vertexPositions[ 4]);
    Assert.AreEqual(new Vector3( 0, 0, -1), vertexPositions[ 5]);
    Assert.AreEqual(new Vector3(+1, 0, -2), vertexPositions[ 6]);
    Assert.AreEqual(new Vector3(+1, 0, -1), vertexPositions[ 7]);
    Assert.AreEqual(new Vector3(+2, 0, -2), vertexPositions[ 8]);
    Assert.AreEqual(new Vector3(+2, 0, -1), vertexPositions[ 9]);
    Assert.AreEqual(new Vector3(-1, 0,  0), vertexPositions[10]);
    Assert.AreEqual(new Vector3(-2, 0,  0), vertexPositions[11]);
    Assert.AreEqual(new Vector3( 0,0.5f,0), vertexPositions[12]);
    Assert.AreEqual(new Vector3(+1, 0,  0), vertexPositions[13]);
    Assert.AreEqual(new Vector3(+2, 0,  0), vertexPositions[14]);
    Assert.AreEqual(new Vector3(-1, 0,  1), vertexPositions[15]);
    Assert.AreEqual(new Vector3(-2, 0,  1), vertexPositions[16]);
    Assert.AreEqual(new Vector3( 0, 0,  1), vertexPositions[17]);
    Assert.AreEqual(new Vector3(+1, 0,  1), vertexPositions[18]);
    Assert.AreEqual(new Vector3(+2, 0,  1), vertexPositions[19]);
    Assert.AreEqual(new Vector3(-1, 0,  2), vertexPositions[20]);
    Assert.AreEqual(new Vector3(-2, 0,  2), vertexPositions[21]);
    Assert.AreEqual(new Vector3( 0, 0,  2), vertexPositions[22]);
    Assert.AreEqual(new Vector3(+1, 0,  2), vertexPositions[23]);
    Assert.AreEqual(new Vector3(+2, 0,  2), vertexPositions[24]);

    var normalsChannel = meshContent.Geometry[0].Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());
    // All of the points around the edge should be pointed up. Some of the ones on the interior will be affected
    // by the center point being shifted up, so we won't check those.
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 0]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 1]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 4]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 6]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 8]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 9]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[14]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[19]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[24]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[23]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[22]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[20]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[21]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[16]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[11]);
    Assert.AreEqual(Vector3.UnitY, normalsChannel[ 3]);
}

        [Test]
        public void TestMergePositionsMultipleGeometries()
        {
             var mb = MeshBuilder.StartMesh("Test");

            mb.CreatePosition(new Vector3(0f, 0f, 0f));
            mb.CreatePosition(new Vector3(1f, 1f, 1f));
            mb.CreatePosition(new Vector3(2f, 2f, 2f));
            mb.CreatePosition(new Vector3(0f, 0f, 0f));
            mb.CreatePosition(new Vector3(1f, 1f, 1f));
            mb.CreatePosition(new Vector3(2f, 2f, 2f));

            mb.SetMaterial(material1);

            mb.AddTriangleVertex(2);
            mb.AddTriangleVertex(1);
            mb.AddTriangleVertex(0);
            mb.AddTriangleVertex(5);
            mb.AddTriangleVertex(4);
            mb.AddTriangleVertex(3);

            mb.SetMaterial(material2);
            mb.AddTriangleVertex(5);
            mb.AddTriangleVertex(4);
            mb.AddTriangleVertex(3);

            var mesh = mb.FinishMesh();
            Assert.AreEqual(6, mesh.Positions.Count);

            MeshHelper.MergeDuplicatePositions(mesh, 1f);
            Assert.AreEqual(3, mesh.Positions.Count);

            var geom = mesh.Geometry[0];
            Assert.AreEqual(6, geom.Vertices.Positions.Count);
            Assert.AreEqual(6, geom.Vertices.PositionIndices.Count);
            Assert.AreEqual(2, geom.Vertices.PositionIndices[0]);
            Assert.AreEqual(1, geom.Vertices.PositionIndices[1]);
            Assert.AreEqual(0, geom.Vertices.PositionIndices[2]);
            Assert.AreEqual(2, geom.Vertices.PositionIndices[0]);
            Assert.AreEqual(1, geom.Vertices.PositionIndices[1]);
            Assert.AreEqual(0, geom.Vertices.PositionIndices[2]);

            geom = mesh.Geometry[1];
            Assert.AreEqual(3, geom.Vertices.Positions.Count);
            Assert.AreEqual(3, geom.Vertices.PositionIndices.Count);
            Assert.AreEqual(2, geom.Vertices.PositionIndices[0]);
            Assert.AreEqual(1, geom.Vertices.PositionIndices[1]);
            Assert.AreEqual(0, geom.Vertices.PositionIndices[2]);
        }

        [Test]
        public void TestNormalGenerationMultipleGeometries()
        {
            var mb = MeshBuilder.StartMesh("Test");

            mb.CreatePosition(new Vector3(0f, 0f, 0f));
            mb.CreatePosition(new Vector3(1f, 0f, 0f));
            mb.CreatePosition(new Vector3(2f, 0f, 0f));

            mb.CreatePosition(new Vector3(0f, 0f, 1f));
            mb.CreatePosition(new Vector3(1f, 0f, 1f));
            mb.CreatePosition(new Vector3(2f, 0f, 1f));

            mb.CreatePosition(new Vector3(0f, -1f, 0f));
            mb.CreatePosition(new Vector3(1f, -1f, 0f));
            mb.CreatePosition(new Vector3(2f, -1f, 0f));

            mb.SetMaterial(material1);

            mb.AddTriangleVertex(0);
            mb.AddTriangleVertex(1);
            mb.AddTriangleVertex(4);

            mb.AddTriangleVertex(1);
            mb.AddTriangleVertex(2);
            mb.AddTriangleVertex(5);

            mb.AddTriangleVertex(0);
            mb.AddTriangleVertex(4);
            mb.AddTriangleVertex(3);

            mb.AddTriangleVertex(1);
            mb.AddTriangleVertex(5);
            mb.AddTriangleVertex(4);

            mb.SetMaterial(material2);
            mb.AddTriangleVertex(0);
            mb.AddTriangleVertex(6);
            mb.AddTriangleVertex(7);

            mb.AddTriangleVertex(1);
            mb.AddTriangleVertex(7);
            mb.AddTriangleVertex(2);

            mb.AddTriangleVertex(0);
            mb.AddTriangleVertex(7);
            mb.AddTriangleVertex(1);

            mb.AddTriangleVertex(2);
            mb.AddTriangleVertex(7);
            mb.AddTriangleVertex(8);

            var mesh = mb.FinishMesh();

            var firstGeometryPositions = mesh.Geometry[0].Vertices.Positions;
            var firstNormalChannel = mesh.Geometry[0].Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal());
            Assert.AreEqual(new Vector3(0, 0, 0), firstGeometryPositions[0]);
            Assert.AreEqual(new Vector3(1, 0, 0), firstGeometryPositions[1]);
            Assert.AreEqual(new Vector3(1, 0, 1), firstGeometryPositions[2]);

            Assert.AreEqual(new Vector3(2, 0, 0), firstGeometryPositions[3]);
            Assert.AreEqual(new Vector3(2, 0, 1), firstGeometryPositions[4]);
            Assert.AreEqual(new Vector3(0, 0, 1), firstGeometryPositions[5]);

            Assert.AreEqual(Vector3.UnitY, firstNormalChannel[0]);
            Assert.AreEqual(Vector3.UnitY, firstNormalChannel[1]);
            Assert.AreEqual(Vector3.UnitY, firstNormalChannel[2]);
            Assert.AreEqual(Vector3.UnitY, firstNormalChannel[3]);
            Assert.AreEqual(Vector3.UnitY, firstNormalChannel[4]);
            Assert.AreEqual(Vector3.UnitY, firstNormalChannel[5]);


            var secondGeometryPositions = mesh.Geometry[1].Vertices.Positions;
            var secondNormalChannel = mesh.Geometry[1].Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal()); ;
            Assert.AreEqual(new Vector3(0, 0, 0), secondGeometryPositions[0]);
            Assert.AreEqual(new Vector3(0, -1, 0), secondGeometryPositions[1]);
            Assert.AreEqual(new Vector3(1, -1, 0), secondGeometryPositions[2]);
            Assert.AreEqual(new Vector3(1, 0, 0), secondGeometryPositions[3]);
            Assert.AreEqual(new Vector3(2, 0, 0), secondGeometryPositions[4]);
            Assert.AreEqual(new Vector3(2, -1, 0), secondGeometryPositions[5]);

            Assert.AreEqual(-Vector3.UnitZ, secondNormalChannel[0]);
            Assert.AreEqual(-Vector3.UnitZ, secondNormalChannel[1]);
            Assert.AreEqual(-Vector3.UnitZ, secondNormalChannel[2]);
            Assert.AreEqual(-Vector3.UnitZ, secondNormalChannel[3]);
            Assert.AreEqual(-Vector3.UnitZ, secondNormalChannel[4]);
            Assert.AreEqual(-Vector3.UnitZ, secondNormalChannel[5]);
        }
    }
}
