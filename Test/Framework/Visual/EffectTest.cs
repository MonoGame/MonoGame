// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Visual
{
    internal class EffectTest : VisualTestFixtureBase
    {
        [Test]
        public void EffectPassShouldSetTexture()
        {
            Game.DrawWith += (sender, e) =>
            {
                var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Game.GraphicsDevice.Textures[0] = null;

                var effect = new BasicEffect(Game.GraphicsDevice);
                effect.TextureEnabled = true;
                effect.Texture = texture;

                Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);

                var effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();

                Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));
            };
            Game.Run();
        }

        [Test]
        public void EffectPassShouldSetTextureOnSubsequentCalls()
        {
            Game.DrawWith += (sender, e) =>
            {
                var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Game.GraphicsDevice.Textures[0] = null;

                var effect = new BasicEffect(Game.GraphicsDevice);
                effect.TextureEnabled = true;
                effect.Texture = texture;

                Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);

                var effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();

                Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));

                Game.GraphicsDevice.Textures[0] = null;

                effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();

                Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));
            };
            Game.Run();
        }

        [Test]
        public void EffectPassShouldSetTextureEvenIfNull()
        {
            Game.DrawWith += (sender, e) =>
            {
                var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Game.GraphicsDevice.Textures[0] = texture;

                var effect = new BasicEffect(Game.GraphicsDevice);
                effect.TextureEnabled = true;
                effect.Texture = null;

                Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));

                var effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();

                Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);
            };
            Game.Run();
        }

        [Test]
        public void EffectPassShouldOverrideTextureIfNotExplicitlySet()
        {
            Game.DrawWith += (sender, e) =>
            {
                var texture = new Texture2D(Game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                Game.GraphicsDevice.Textures[0] = texture;

                var effect = new BasicEffect(Game.GraphicsDevice);
                effect.TextureEnabled = true;

                Assert.That(Game.GraphicsDevice.Textures[0], Is.SameAs(texture));

                var effectPass = effect.CurrentTechnique.Passes[0];
                effectPass.Apply();

                Assert.That(Game.GraphicsDevice.Textures[0], Is.Null);
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldReturnEmptyWhenNotSet()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat1 = effect.Parameters["TestFloat"];

                // before set, get defaults
                Assert.AreEqual(0.0f, testFloat1.GetValueSingle());
                Assert.AreEqual(new Vector2(), testFloat1.GetValueVector2());
                Assert.AreEqual(new Vector3(), testFloat1.GetValueVector3());
                Assert.AreEqual(new Vector4(), testFloat1.GetValueVector4());
                Assert.AreEqual(new Matrix(), testFloat1.GetValueMatrix());
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithSingle()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat1 = effect.Parameters["TestFloat"];
                var vector1 = 1f;

                // setting a float
                testFloat1.SetValue(vector1);
                Assert.AreEqual(vector1, testFloat1.GetValueSingle());
                Assert.Throws<ArgumentOutOfRangeException>(() => testFloat1.GetValueSingleArray(0));
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat1.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[5] { 1f, 0f, 0f, 0f, 0f }, testFloat1.GetValueSingleArray(5));
                Assert.AreEqual(new Vector2(1f, 1f), testFloat1.GetValueVector2());
                Assert.AreEqual(new Vector3(1f, 1f, 1f), testFloat1.GetValueVector3());
                Assert.AreEqual(new Vector4(1f, 1f, 1f, 1f), testFloat1.GetValueVector4());
                Assert.AreEqual(new Matrix(1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f), testFloat1.GetValueMatrix());
                Assert.Throws<InvalidCastException>(() => testFloat1.GetValueMatrixArray(1));
                Assert.Throws<InvalidCastException>(() => testFloat1.GetValueMatrixArray(3));
                CollectionAssert.AreEqual(new Vector2[2] { new Vector2(1, 0), new Vector2() }, testFloat1.GetValueVector2Array(2));
                CollectionAssert.AreEqual(new Vector3[2] { new Vector3(1, 0, 0), new Vector3() }, testFloat1.GetValueVector3Array(2));
                CollectionAssert.AreEqual(new Vector4[2] { new Vector4(1, 0, 0, 0), new Vector4() }, testFloat1.GetValueVector4Array(2));
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithSingle2()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat2 = effect.Parameters["TestFloat2"];
                var vector2 = new Vector2(1f, 2f);

                // setting a float2
                testFloat2.SetValue(vector2);
                Assert.Throws<InvalidCastException>(() => testFloat2.GetValueSingle());
                Assert.Throws<ArgumentOutOfRangeException>(() => testFloat2.GetValueSingleArray(0));
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat2.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[5] { 1f, 2f, 0f, 0f, 0f }, testFloat2.GetValueSingleArray(5));
                Assert.AreEqual(vector2, testFloat2.GetValueVector2());
                Assert.Throws<InvalidCastException>(() => testFloat2.GetValueVector3());
                Assert.Throws<InvalidCastException>(() => testFloat2.GetValueVector4());
                Assert.Throws<InvalidCastException>(() => testFloat2.GetValueMatrix());
                Assert.Throws<InvalidCastException>(() => testFloat2.GetValueMatrixArray(1));
                Assert.Throws<InvalidCastException>(() => testFloat2.GetValueMatrixArray(3));
                CollectionAssert.AreEqual(new Vector2[2] { new Vector2(1, 2), new Vector2() }, testFloat2.GetValueVector2Array(2));
                CollectionAssert.AreEqual(new Vector3[2] { new Vector3(1, 2, 0), new Vector3() }, testFloat2.GetValueVector3Array(2));
                CollectionAssert.AreEqual(new Vector4[2] { new Vector4(1, 2, 0, 0), new Vector4() }, testFloat2.GetValueVector4Array(2));
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithSingle3()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat3 = effect.Parameters["TestFloat3"];
                var vector3 = new Vector3(1f, 2f, 3f);

                // setting a float3
                testFloat3.SetValue(vector3);
                Assert.Throws<InvalidCastException>(() => testFloat3.GetValueSingle());
                Assert.Throws<ArgumentOutOfRangeException>(() => testFloat3.GetValueSingleArray(0));
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat3.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[5] { 1f, 2f, 3f, 0f, 0f }, testFloat3.GetValueSingleArray(5));
                Assert.Throws<InvalidCastException>(() => testFloat3.GetValueVector2());
                Assert.AreEqual(vector3, testFloat3.GetValueVector3());
                Assert.Throws<InvalidCastException>(() => testFloat3.GetValueVector4());
                Assert.Throws<InvalidCastException>(() => testFloat3.GetValueMatrix());
                Assert.Throws<InvalidCastException>(() => testFloat3.GetValueMatrixArray(1));
                Assert.Throws<InvalidCastException>(() => testFloat3.GetValueMatrixArray(3));
                CollectionAssert.AreEqual(new Vector2[2] { new Vector2(1, 2), new Vector2(3, 0) }, testFloat3.GetValueVector2Array(2));
                CollectionAssert.AreEqual(new Vector3[2] { new Vector3(1, 2, 3), new Vector3() }, testFloat3.GetValueVector3Array(2));
                CollectionAssert.AreEqual(new Vector4[2] { new Vector4(1, 2, 3, 0), new Vector4() }, testFloat3.GetValueVector4Array(2));
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithSingle4()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat4 = effect.Parameters["TestFloat4"];
                var vector4 = new Vector4(1f, 2f, 3f, 4f);

                // setting a float4
                testFloat4.SetValue(vector4);
                Assert.Throws<InvalidCastException>(() => testFloat4.GetValueSingle());
                Assert.Throws<ArgumentOutOfRangeException>(() => testFloat4.GetValueSingleArray(0));
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat4.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[5] { 1f, 2f, 3f, 4f, 0f }, testFloat4.GetValueSingleArray(5));
                Assert.Throws<InvalidCastException>(() => testFloat4.GetValueVector2());
                Assert.Throws<InvalidCastException>(() => testFloat4.GetValueVector3());
                Assert.AreEqual(vector4, testFloat4.GetValueVector4());
                Assert.Throws<InvalidCastException>(() => testFloat4.GetValueMatrix());
                Assert.Throws<InvalidCastException>(() => testFloat4.GetValueMatrixArray(1));
                Assert.Throws<InvalidCastException>(() => testFloat4.GetValueMatrixArray(3));
                CollectionAssert.AreEqual(new Vector2[2] { new Vector2(1, 2), new Vector2(3, 4) }, testFloat4.GetValueVector2Array(2));
                CollectionAssert.AreEqual(new Vector3[2] { new Vector3(1, 2, 3), new Vector3(4, 0, 0) }, testFloat4.GetValueVector3Array(2));
                CollectionAssert.AreEqual(new Vector4[2] { new Vector4(1, 2, 3, 4), new Vector4() }, testFloat4.GetValueVector4Array(2));
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithSingleArray()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat3Array = effect.Parameters["TestFloat3Array"];
                var vector3Array = new[]
                {
                    new Vector3(1, 2, 3),
                    new Vector3(4, 5, 6),
                    new Vector3(7, 8, 9)
                };

#if XNA
                var invalidException = typeof(InvalidOperationException);
#else
                var invalidException = typeof(InvalidCastException);
#endif

                //setting a float3[]
                testFloat3Array.SetValue(vector3Array);
                Assert.Throws(invalidException, () => testFloat3Array.GetValueSingle());
                Assert.Throws<ArgumentOutOfRangeException>(() => testFloat3Array.GetValueSingleArray(0));
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat3Array.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[5] { 1f, 2f, 3f, 4f, 5f }, testFloat3Array.GetValueSingleArray(5));
                Assert.Throws(invalidException, () => testFloat3Array.GetValueVector2());
                Assert.Throws(invalidException, () => testFloat3Array.GetValueVector3());
                Assert.Throws(invalidException, () => testFloat3Array.GetValueVector4());
                CollectionAssert.AreEqual(new Vector2[3] { new Vector2(1, 2), new Vector2(3, 4), new Vector2(5, 6) }, testFloat3Array.GetValueVector2Array(3));
                CollectionAssert.AreEqual(vector3Array, testFloat3Array.GetValueVector3Array(3));
                CollectionAssert.AreEqual(new Vector4[3] { new Vector4(1, 2, 3, 4), new Vector4(5, 6, 7, 8), new Vector4(9, 0, 0, 0) }, testFloat3Array.GetValueVector4Array(3));
                Assert.Throws(invalidException, () => testFloat3Array.GetValueMatrix());
                Assert.Throws<InvalidCastException>(() => testFloat3Array.GetValueMatrixArray(1));
                Assert.Throws<InvalidCastException>(() => testFloat3Array.GetValueMatrixArray(3));
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithMatrix()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat4x3 = effect.Parameters["TestFloat4x3"];
                var matrix = new Matrix(
                    1, 2, 3, 4,
                    5, 6, 7, 8,
                    9, 10, 11, 12,
                    13, 14, 15, 16);
                var outMatrix = new Matrix(
                    1, 2, 3, 0,
                    5, 6, 7, 0,
                    9, 10, 11, 0,
                    13, 14, 15, 0);

                // setting a matrix
                testFloat4x3.SetValue(matrix);
                Assert.Throws<InvalidCastException>(() => testFloat4x3.GetValueSingle());
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat4x3.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[3] { 1f, 2f, 3f }, testFloat4x3.GetValueSingleArray(3));
                CollectionAssert.AreEqual(new float[5] { 1f, 2f, 3f, 5f, 6f }, testFloat4x3.GetValueSingleArray(5));
                CollectionAssert.AreEqual(new float[20] { 1f, 2f, 3f, 5f, 6f, 7f, 9f, 10, 11f, 13f, 14f, 15f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, testFloat4x3.GetValueSingleArray(20));
                Assert.Throws<InvalidCastException>(() => testFloat4x3.GetValueVector2());
                Assert.Throws<InvalidCastException>(() => testFloat4x3.GetValueVector3());
                Assert.Throws<InvalidCastException>(() => testFloat4x3.GetValueVector4());
                Assert.AreEqual(outMatrix, testFloat4x3.GetValueMatrix());
                Assert.Throws<InvalidCastException>(() => testFloat4x3.GetValueMatrixArray(1));
                Assert.Throws<InvalidCastException>(() => testFloat4x3.GetValueMatrixArray(3));
                CollectionAssert.AreEqual(new Vector2[5] { new Vector2(1, 2), new Vector2(3, 5), new Vector2(6, 7), new Vector2(9, 10), new Vector2(11, 13) }, testFloat4x3.GetValueVector2Array(5));
                CollectionAssert.AreEqual(new Vector3[5] { new Vector3(1, 2, 3), new Vector3(5, 6, 7), new Vector3(9, 10, 11), new Vector3(13, 14, 15), new Vector3() }, testFloat4x3.GetValueVector3Array(5));
                CollectionAssert.AreEqual(new Vector4[5] { new Vector4(1, 2, 3, 5), new Vector4(6, 7, 9, 10), new Vector4(11, 13, 14, 15), new Vector4(), new Vector4() }, testFloat4x3.GetValueVector4Array(5));
            };
            Game.Run();
        }

        [Test]
        public void EffectParameterShouldMatchXNAWithMatrixArray()
        {
            var effectName = "ReallySimpleEffect";
#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif
            effectName = Paths.Effect(effectName);

            Effect effect = null;
            Game.LoadContentWith += (sender, e) =>
            {
                effect = Game.Content.Load<Effect>(effectName);
            };
            Game.DrawWith += (sender, e) =>
            {
                var testFloat4x3Array = effect.Parameters["TestFloat4x3Array"];

                var matrix = new Matrix(
                    1, 2, 3, 4,
                    5, 6, 7, 8,
                    9, 10, 11, 12,
                    13, 14, 15, 16);
                var matrix2 = new Matrix(
                     17, 18, 19, 20,
                     21, 22, 23, 24,
                     25, 26, 27, 28,
                     29, 30, 31, 32);
                var outMatrix = new Matrix(
                    1, 2, 3, 0,
                    5, 6, 7, 0,
                    9, 10, 11, 0,
                    13, 14, 15, 0);
                var outMatrix2 = new Matrix(
                     17, 18, 19, 0,
                     21, 22, 23, 0,
                     25, 26, 27, 0,
                     29, 30, 31, 0);
                var matrixArray = new[]
                {
                    matrix,
                    matrix2
                };
                var outMatrixArray = new[]
                {
                    outMatrix,
                    outMatrix2,
                    new Matrix()
                };

#if XNA
                var invalidException = typeof(InvalidOperationException);
#else
                var invalidException = typeof(InvalidCastException);
#endif

                // setting a matrix[]
                testFloat4x3Array.SetValue(matrixArray);
                Assert.Throws(invalidException, () => testFloat4x3Array.GetValueSingle());
                CollectionAssert.AreEqual(new float[1] { 1f }, testFloat4x3Array.GetValueSingleArray(1));
                CollectionAssert.AreEqual(new float[3] { 1f, 2f, 3f }, testFloat4x3Array.GetValueSingleArray(3));
                CollectionAssert.AreEqual(new float[5] { 1f, 2f, 3f, 5f, 6f }, testFloat4x3Array.GetValueSingleArray(5));
                CollectionAssert.AreEqual(new float[20] { 1f, 2f, 3f, 5f, 6f, 7f, 9f, 10, 11f, 13f, 14f, 15f, 17f, 18f, 19f, 21f, 22f, 23f, 25f, 26f }, testFloat4x3Array.GetValueSingleArray(20));
                Assert.Throws(invalidException, () => testFloat4x3Array.GetValueVector2());
                Assert.Throws(invalidException, () => testFloat4x3Array.GetValueVector3());
                Assert.Throws(invalidException, () => testFloat4x3Array.GetValueVector4());
                Assert.Throws(invalidException, () => testFloat4x3Array.GetValueMatrix());
                CollectionAssert.AreEqual(new[] { outMatrix }, testFloat4x3Array.GetValueMatrixArray(1));
                CollectionAssert.AreEqual(outMatrixArray, testFloat4x3Array.GetValueMatrixArray(3));
                CollectionAssert.AreEqual(new Vector2[5] { new Vector2(1, 2), new Vector2(3, 5), new Vector2(6, 7), new Vector2(9, 10), new Vector2(11, 13) }, testFloat4x3Array.GetValueVector2Array(5));
                CollectionAssert.AreEqual(new Vector3[5] { new Vector3(1, 2, 3), new Vector3(5, 6, 7), new Vector3(9, 10, 11), new Vector3(13, 14, 15), new Vector3(17, 18, 19) }, testFloat4x3Array.GetValueVector3Array(5));
                CollectionAssert.AreEqual(new Vector4[7] { new Vector4(1, 2, 3, 5), new Vector4(6, 7, 9, 10), new Vector4(11, 13, 14, 15), new Vector4(17, 18, 19, 21), new Vector4(22, 23, 25, 26), new Vector4(27, 29, 30, 31), new Vector4() }, testFloat4x3Array.GetValueVector4Array(7));
            };
            Game.Run();
        }
    }
}