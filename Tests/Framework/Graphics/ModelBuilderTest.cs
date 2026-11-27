using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tests.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.DesktopGL.Framework.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    internal sealed class ModelBuilderTest : GraphicsDeviceTestFixtureBase
    {
        private ModelMesh CreateModelMesh()
        {
            var vertices = new VertexPosition[]
            {
                new VertexPosition(new Vector3(0, 0, 0)),
                new VertexPosition(new Vector3(0, 1, 0)),
                new VertexPosition(new Vector3(1, 0, 0)),
            };

            var vertexBuffer = new VertexBuffer(gd, typeof(VertexPosition), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            var indices = new short[] { 0, 1, 2, 0, 1, 2 };
            var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);

            var meshBuilder = new ModelMeshBuilder();
            meshBuilder.AddModelMeshPart(indexBuffer, 0, vertexBuffer, 0, 3, 1, new BoundingSphere(Vector3.Zero, 1));

            return meshBuilder.Create(gd);
        }

        private static void AssertModelMesh(ModelMesh modelMesh)
        {
            Assert.That(modelMesh, Is.Not.Null);
            Assert.That(modelMesh.MeshParts.Count, Is.EqualTo(1));

            var part = modelMesh.MeshParts[0];
            Assert.That(part, Is.Not.Null);
            Assert.That(part.StartIndex, Is.EqualTo(0));
            Assert.That(part.VertexOffset, Is.EqualTo(0));
            Assert.That(part.NumVertices, Is.EqualTo(3));
            Assert.That(part.PrimitiveCount, Is.EqualTo(1));
        }

        private static void AssertModelBone(ModelBone bone, ModelBone boneByIndex, string name, int index, string rootName, int childrenCount = 0, bool hasMesh = false)
        {
            Assert.That(bone, Is.Not.Null);
            Assert.That(bone, Is.EqualTo(boneByIndex));
            Assert.That(bone.Name, Is.EqualTo(name));
            Assert.That(bone.Index, Is.EqualTo(index));

            if (rootName == null)
            {
                Assert.That(bone.Parent, Is.Null);
            }
            else
            {
                Assert.That(bone.Parent, Is.Not.Null);
                Assert.That(bone.Parent.Name, Is.EqualTo(rootName));
            }

            Assert.That(bone.Children, Is.Not.Null);
            Assert.That(bone.Children.Count, Is.EqualTo(childrenCount));

            Assert.That(bone.Meshes, Is.Not.Null);
            if (!hasMesh)
            {
                Assert.That(bone.Meshes.Count, Is.EqualTo(0));
            }
            else
            {
                Assert.That(bone.Meshes.Count, Is.EqualTo(1));
                AssertModelMesh(bone.Meshes[0]);
            }
        }

        [Test]
        public void ModelMeshBuilderTest()
        {
            var modelMesh = CreateModelMesh();
            AssertModelMesh(modelMesh);
        }

        [Test]
        public void ModelBuildingTest()
        {
            var mesh = CreateModelMesh();

            // Create bones
            var root = new ModelBoneDesc("root");

            var bone1 = new ModelBoneDesc("bone1");
            root.Children.Add(bone1);

            var bone3 = new ModelBoneDesc("bone3");
            bone1.Children.Add(bone3);

            var bone4 = new ModelBoneDesc("bone4");
            bone1.Children.Add(bone4);

            var bone5 = new ModelBoneDesc("bone5");
            bone5.CustomIndex = 15;
            bone1.Children.Add(bone5);

            var bone2 = new ModelBoneDesc("bone2");
            bone2.Meshes.Add(mesh);
            root.Children.Add(bone2);

            var model = ModelBuilder.Create(root);
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Bones, Is.Not.Null);
            Assert.That(model.Bones.Count, Is.EqualTo(6));
            Assert.That(model.Meshes, Is.Not.Null);
            Assert.That(model.Meshes.Count, Is.EqualTo(1));
            AssertModelMesh(model.Meshes[0]);

            AssertModelBone(model.Root, model.Bones[0], "root", 0, null, 2);
            AssertModelBone(model.Root.Children[0], model.Bones[1], "bone1", 1, "root", 3);
            AssertModelBone(model.Root.Children[0].Children[0], model.Bones[2], "bone3", 2, "bone1");
            AssertModelBone(model.Root.Children[0].Children[1], model.Bones[3], "bone4", 3, "bone1");
            AssertModelBone(model.Root.Children[0].Children[2], model.Bones[4], "bone5", 15, "bone1");
            AssertModelBone(model.Root.Children[1], model.Bones[5], "bone2", 4, "root", 0, true);
        }
    }
}
