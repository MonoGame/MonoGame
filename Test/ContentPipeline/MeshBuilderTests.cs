using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.ContentPipeline
{
    class MeshBuilderTests
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

        private MeshContent CreateBasicMesh()
        {
            MeshBuilder builder = MeshBuilder.StartMesh("Mesh1");

            builder.CreatePosition(new Vector3(0, 0, 0));
            builder.CreatePosition(new Vector3(1, 0, 0));
            builder.CreatePosition(new Vector3(1, 1, 1));

            builder.SetMaterial(material1);

            builder.AddTriangleVertex(0);
            builder.AddTriangleVertex(1);
            builder.AddTriangleVertex(2);

            return builder.FinishMesh();
        }

        private MeshContent CreateTwoMaterialsMesh()
        {
            MeshBuilder builder = MeshBuilder.StartMesh("Mesh2");

            int firstPos = builder.CreatePosition(new Vector3(0, 0, 0));
            int secondPos =  builder.CreatePosition(new Vector3(1, 0, 0));
            int thirdPos = builder.CreatePosition(new Vector3(1, 1, 1));

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
            var output = CreateBasicMesh();

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

            Assert.AreEqual(material1, output.Geometry[0].Material);

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

            Assert.AreEqual(material1, output.Geometry[0].Material);

            Assert.AreEqual(0, output.Geometry[1].Indices[0]);
            Assert.AreEqual(1, output.Geometry[1].Indices[1]);
            Assert.AreEqual(2, output.Geometry[1].Indices[2]);
            Assert.AreEqual(material2, output.Geometry[1].Material);

            Assert.AreEqual(3, output.Positions.Count);
            Assert.AreEqual("Mesh2", output.Name);
        }

        [Test]
        public void ThrowsInvalidOperationExceptionMeshBuilderTest()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                MeshBuilder builder = MeshBuilder.StartMesh("Mesh1");

                builder.CreatePosition(new Vector3(0, 0, 0));

                builder.AddTriangleVertex(0);

                builder.CreatePosition(new Vector3(0, 0, 0));
            });
        }
    }
}
