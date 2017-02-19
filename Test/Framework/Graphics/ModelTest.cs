// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Tests.Graphics
{

#if !XNA // Disabled because XNA doesn't support manual Model creation


    [TestFixture]
    internal sealed class ModelTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldConstructAndInitialize()
        {
            var actual = new Model(gd, new List<ModelBone>(), new List<ModelMesh>());

            Assert.That(actual.Bones, Is.Empty, "bones initial list is converted to Bones collection");
            Assert.That(actual.Meshes, Is.Empty, "meshes initial list is converted to Meshes collection");
        }

        [Test]
        public void ShouldNotConstructWhenParamsAreNotValid()
        {
            // simple empty collections to make code more readable.
            var emptyBonesList = new List<ModelBone>();
            var emptyMeshesList = new List<ModelMesh>();

            // testing constructor's defined exceptions.
            Assert.Throws<ArgumentNullException>(() => new Model(null, emptyBonesList, emptyMeshesList));
            Assert.Throws<ArgumentNullException>(() => new Model(gd, null, emptyMeshesList));
            Assert.Throws<ArgumentNullException>(() => new Model(gd, emptyBonesList, null));
        }

        [Test]
        public void ShouldReadTransformationsFromBones()
        {
            var someBones = new[] { new ModelBone(), new ModelBone() }.ToList();
            var model = new Model(gd, someBones, new List<ModelMesh>());

            var expected = new[] { Matrix.Identity * 1, Matrix.Identity * 2 };
            var actual = new Matrix[2];
            Assume.That(actual, Is.Not.EqualTo(expected));

            model.CopyBoneTransformsFrom(expected);
            model.CopyBoneTransformsTo(actual);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CopyBoneTransformsFrom_Exceptions()
        {
            var someBones = new[] { new ModelBone() }.ToList();
            var model = new Model(gd, someBones, new List<ModelMesh>());

            Assert.Throws<ArgumentNullException>(() => model.CopyBoneTransformsFrom(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.CopyBoneTransformsFrom(new Matrix[0]));
        }

        [Test]
        public void CopyBoneTransformsTo_Exceptions()
        {
            var someBones = new[] { new ModelBone() }.ToList();
            var model = new Model(gd, someBones, new List<ModelMesh>());
            Assert.Throws<ArgumentNullException>(() => model.CopyBoneTransformsTo(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.CopyBoneTransformsTo(new Matrix[0]));
        }

        [Test]
        public void ShouldDrawSampleModel()
        {
            var model = new SquareModel(gd);

            PrepareFrameCapture();

            model.Draw();

            CheckFrames();
        }

        /// <summary>
        /// Simple Model definition for a square. Created for testing purposes only.
        /// 
        /// The simple model is manually created with Model* related classes to cover them with unit tests.
        /// </summary>
        private sealed class SquareModel
        {
            // centre point of the model - used to point camera and calculate corners
            private Vector3 centre = new Vector3(1, 5, 1);

            // the model
            private readonly Model model;

            // matrixes required for visualisation.
            private readonly Matrix view, projection;

            /// <summary>
            /// Creates a new instance of SquareModel.
            /// </summary>
            /// <param name="gd">GraphicsDevice uset to render visualisation.</param>
            public SquareModel(GraphicsDevice gd)
            {
                view = Matrix.CreateLookAt(Vector3.Zero, centre, Vector3.Up);

                projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gd.Viewport.AspectRatio, 0.1f, 100.0f);

                // 4 vertices allow to construct a square.
                // 1---2
                // |  /|
                // | c |
                // |/  |
                // 0---3
                // where 'c' stands for 'centre'.
                var vertices = new[]
                {
                    new VertexPosition(centre + new Vector3(-1,0,-1)),
                    new VertexPosition(centre + new Vector3(-1,0,+1)),
                    new VertexPosition(centre + new Vector3(+1,0,+1)),
                    new VertexPosition(centre + new Vector3(+1,0,-1))
                };
                var vertexBuffer = new VertexBuffer(gd, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(vertices);

                var verticesPointers = new ushort[]
                {
                    0, 1, 2,
                    0, 2, 3
                };
                var indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, verticesPointers.Length, BufferUsage.None);
                indexBuffer.SetData(verticesPointers);

                var meshPart = new ModelMeshPart();
                meshPart.NumVertices = vertices.Length;
                meshPart.PrimitiveCount = verticesPointers.Length / 3;
                meshPart.VertexBuffer = vertexBuffer;
                meshPart.IndexBuffer = indexBuffer;

                var mesh = new ModelMesh(gd, new[] { meshPart }.ToList());
                meshPart.Effect = new BasicEffect(gd) { DiffuseColor = Color.Green.ToVector3() };

                var cube = new ModelBone();
                cube.Transform = Matrix.Identity;
                //cube.ModelTransform = Matrix.Identity;
                mesh.ParentBone = cube;
                cube.AddMesh(mesh);

                model = new Model(gd, new[] { cube }.ToList(), new[] { mesh }.ToList());
            }

            public void Draw()
            {
                model.Draw(Matrix.Identity, view, projection);
            }

        }
    }
#endif
}
