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

    [TestFixture]
    internal sealed class ModelTest : GraphicsDeviceTestFixtureBase
    {

        [Test]
        public void ShouldDrawSampleModel()
        {
            // we use a model exported from cube created by default in new blender project.
            var model = content.Load<Model>(Paths.Model("BlenderDefaultCube"));

            // model contains a bit more that only the cube, so let extract the cube
            // to set transformation matrix. It is necessary to make the cube visible.
            var cubeBone = model.Bones[1];
            cubeBone.Transform = Matrix.Identity;

            // let's make the model colored in red and lighted
            var effect = (BasicEffect)model.Meshes[0].Effects[0];
            effect.DiffuseColor = Color.Red.ToVector3();
            effect.EnableDefaultLighting();

            var world = Matrix.Identity;
            var view = Matrix.CreateLookAt(new Vector3(5, 5, 5), new Vector3(0, 0, 0), Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gd.Viewport.AspectRatio, 0.1f, 100.0f);

            PrepareFrameCapture();

            model.Draw(world, view, projection);

            CheckFrames();
        }

#if !XNA // Tests below are valid for Monogame only because.

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

#endif
    }
}
