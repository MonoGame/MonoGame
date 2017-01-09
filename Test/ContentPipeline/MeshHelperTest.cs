// MonoGame - Copyright (C) The MonoGame Team
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
    }
}