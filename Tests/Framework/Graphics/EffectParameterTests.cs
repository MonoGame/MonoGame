// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System.IO;

namespace MonoGame.Tests.Graphics
{
    // TODO: Bring this suite of tests to the other APIs - it's a good check that they all handle params similarly.
#if VULKAN
    [NonParallelizable]
    [RunOnUiTestFixture]
    class EffectParameterTests : GraphicsDeviceTestFixtureBase
    {
        private Effect _effect;

        class ColorGridShaderTestHarness
        {
            int _width;
            int _height;
            GraphicsDevice _graphicsDevice;
            Vector4[] _capturedData;
            RenderTarget2D _renderTarget;
            VertexBuffer _quad;

            public ColorGridShaderTestHarness(int width, int height, GraphicsDevice graphicsDevice)
            {
                // Make each colour cell a 3x3 grid that we can sample from the middle
                // This prevents some issues with FP precision
                _width = width * 3;
                _height = height * 3;

                _capturedData = new Vector4[_width * _height];
                _renderTarget = new RenderTarget2D(graphicsDevice, _width, _height, false, SurfaceFormat.Vector4, DepthFormat.None);

                _quad = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
                _quad.SetData([
                    new VertexPositionTexture(new Vector3(-1,  1,  0), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3( 1,  1,  0), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(-1, -1,  0), new Vector2(0, 1)),
                    new VertexPositionTexture(new Vector3( 1, -1,  0), new Vector2(1, 1)),
                ]);
                _graphicsDevice = graphicsDevice;
            }

            public void CaptureShaderOutput(Effect effect)
            {
                _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                _graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
                _graphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

                _graphicsDevice.SetRenderTarget(_renderTarget);
                _graphicsDevice.Clear(Color.Black);
                _graphicsDevice.SetVertexBuffer(_quad);
                effect.CurrentTechnique.Passes[0].Apply();
                _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

                _graphicsDevice.SetVertexBuffer(null);
                _graphicsDevice.SetRenderTarget(null);

                _renderTarget.GetData(_capturedData);
            }

            public Vector4 SampleAt(int x, int y)
            {
                // get the index of the center of the sample
                int index = _width + (3 * _width * y) + 1 + (3 * x);
                return _capturedData[index];
            }

            public void Dispose()
            {
                _renderTarget.Dispose();
                _quad.Dispose();
            }
        }

        public override void SetUp()
        {
            base.SetUp();

            _effect = AssetTestUtility.LoadEffect(content, "ParameterTypes");
            _effect.Parameters["PosOffset"].SetValue(Vector4.Zero);
        }

        public override void TearDown()
        {
            _effect.Dispose();
            base.TearDown();
        }

        [Test]
        public void BooleanParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestBoolean"];
            _effect.Parameters["Boolean"].SetValue(true);

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.2f, 1.0f, 0.3f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void IntParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestInt"];
            _effect.Parameters["Int"].SetValue(200);

            float expectedVal = 200 / 255f;
            Vector4 expected = new(expectedVal, expectedVal, expectedVal, 1.0f);

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(expected, Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void IntArrayParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestIntArray"];
            _effect.Parameters["IntArray"].SetValue(new int[] { 150, 200, 100 });

            Vector4 expected = new(150 / 255f, 200 / 255f, 100 / 255f, 1.0f);

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(expected, Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void MatrixParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestMat"];
            _effect.Parameters["Mat"].SetValue(new Matrix(
                new Vector4(1.0f, 0.5f, 0.5f, 1.0f),
                new Vector4(0.5f, 1.0f, 0.5f, 1.0f),
                new Vector4(1.0f, 0.5f, 1.0f, 1.0f),
                new Vector4(0.5f, 0.5f, 0.5f, 1.0f)
            ));

            var grid = new ColorGridShaderTestHarness(4, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(1.0f, 0.5f, 0.5f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.5f, 1.0f, 0.5f, 1.0f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(1.0f, 0.5f, 1.0f, 1.0f), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), Is.EqualTo(grid.SampleAt(3, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void MatrixArrayParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestMatArray"];
            _effect.Parameters["MatArray"].SetValue([
                new Matrix(
                    new Vector4(1.00f, 0.50f, 0.50f, 1.0f),
                    new Vector4(0.50f, 1.00f, 0.50f, 1.0f),
                    new Vector4(1.00f, 0.50f, 1.00f, 1.0f),
                    new Vector4(0.75f, 0.75f, 0.75f, 1.0f)
                ),
                new Matrix(
                    new Vector4(0.50f, 0.25f, 0.25f, 1.0f),
                    new Vector4(0.25f, 0.50f, 0.25f, 1.0f),
                    new Vector4(0.50f, 0.25f, 0.50f, 1.0f),
                    new Vector4(0.50f, 0.50f, 0.50f, 1.0f)
                ),
                new Matrix(
                    new Vector4(0.25f, 0.10f, 0.10f, 1.0f),
                    new Vector4(0.10f, 0.25f, 0.10f, 1.0f),
                    new Vector4(0.25f, 0.10f, 0.25f, 1.0f),
                    new Vector4(0.25f, 0.25f, 0.25f, 1.0f)
                )
            ]);

            var grid = new ColorGridShaderTestHarness(4, 3, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(1.00f, 0.50f, 0.50f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.50f, 1.00f, 0.50f, 1.0f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(1.00f, 0.50f, 1.00f, 1.0f), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.75f, 0.75f, 0.75f, 1.0f), Is.EqualTo(grid.SampleAt(3, 0)).Using(Vector4Comparer.Epsilon));

            Assert.That(new Vector4(0.50f, 0.25f, 0.25f, 1.0f), Is.EqualTo(grid.SampleAt(0, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.25f, 0.50f, 0.25f, 1.0f), Is.EqualTo(grid.SampleAt(1, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.50f, 0.25f, 0.50f, 1.0f), Is.EqualTo(grid.SampleAt(2, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.50f, 0.50f, 0.50f, 1.0f), Is.EqualTo(grid.SampleAt(3, 1)).Using(Vector4Comparer.Epsilon));

            Assert.That(new Vector4(0.25f, 0.10f, 0.10f, 1.0f), Is.EqualTo(grid.SampleAt(0, 2)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.10f, 0.25f, 0.10f, 1.0f), Is.EqualTo(grid.SampleAt(1, 2)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.25f, 0.10f, 0.25f, 1.0f), Is.EqualTo(grid.SampleAt(2, 2)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.25f, 0.25f, 0.25f, 1.0f), Is.EqualTo(grid.SampleAt(3, 2)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void Matrix3x3Parameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestMat3x3"];
            _effect.Parameters["Mat3x3"].SetValue(new Matrix(
                new Vector4(1.0f, 0.5f, 0.5f, 0.3f),
                new Vector4(0.5f, 1.0f, 0.5f, 0.3f),
                new Vector4(1.0f, 0.5f, 1.0f, 0.3f),
                new Vector4(0.3f, 0.3f, 0.3f, 0.3f)
            ));

            var grid = new ColorGridShaderTestHarness(3, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(1.0f, 0.5f, 0.5f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.5f, 1.0f, 0.5f, 1.0f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(1.0f, 0.5f, 1.0f, 1.0f), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void QuaternionParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestQuat"];
            _effect.Parameters["Quaternion"].SetValue(new Quaternion(0.25f, 0.75f, 0.75f, 0.25f));

            var grid = new ColorGridShaderTestHarness(2, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.25f, 0.75f, 0.0f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.75f, 0.25f, 0.0f, 1.0f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void FloatParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestFloat"];
            _effect.Parameters["Float"].SetValue(0.66f);

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.66f, 0.66f, 0.66f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void FloatArrayParameter()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestFloatArray"];
            _effect.Parameters["FloatArray"].SetValue([0.66f, 0.33f, 0.25f]);

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.66f, 0.33f, 0.25f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void Texture2DParameter()
        {
            Texture2D tex2d = new Texture2D(gd, 2, 2);
            tex2d.SetData([Color.Magenta, Color.Cyan, Color.Yellow, Color.CornflowerBlue]);

            _effect.CurrentTechnique = _effect.Techniques["TestTex2D"];
            _effect.Parameters["Tex2D"].SetValue(tex2d);

            var grid = new ColorGridShaderTestHarness(2, 2, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(Color.Magenta.ToVector4(), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Cyan.ToVector4(), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Yellow.ToVector4(), Is.EqualTo(grid.SampleAt(0, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.CornflowerBlue.ToVector4(), Is.EqualTo(grid.SampleAt(1, 1)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void Texture3DParameter()
        {
            Texture3D tex3d = new Texture3D(gd, 2, 2, 2, false, SurfaceFormat.Color);
            tex3d.SetData([
                Color.Magenta,  Color.Cyan,     Color.Yellow,   Color.CornflowerBlue,
                Color.Red,      Color.Green,    Color.Blue,     Color.MonoGameOrange
            ]);

            _effect.CurrentTechnique = _effect.Techniques["TestTex3D"];
            _effect.Parameters["Tex3D"].SetValue(tex3d);

            var grid = new ColorGridShaderTestHarness(4, 2, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(Color.Magenta.ToVector4(), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Cyan.ToVector4(), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Red.ToVector4(), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Green.ToVector4(), Is.EqualTo(grid.SampleAt(3, 0)).Using(Vector4Comparer.Epsilon));

            Assert.That(Color.Yellow.ToVector4(), Is.EqualTo(grid.SampleAt(0, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.CornflowerBlue.ToVector4(), Is.EqualTo(grid.SampleAt(1, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Blue.ToVector4(), Is.EqualTo(grid.SampleAt(2, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.MonoGameOrange.ToVector4(), Is.EqualTo(grid.SampleAt(3, 1)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TextureCubeParameter()
        {
            TextureCube textureCube = new TextureCube(gd, 2, false, SurfaceFormat.Color);

            textureCube.SetData(CubeMapFace.PositiveX, [Color.Red, Color.Red, Color.Red, Color.Red]);
            textureCube.SetData(CubeMapFace.PositiveY, [Color.Green, Color.Green, Color.Green, Color.Green]);
            textureCube.SetData(CubeMapFace.PositiveZ, [Color.Blue, Color.Blue, Color.Blue, Color.Blue]);

            textureCube.SetData(CubeMapFace.NegativeX, [Color.Cyan, Color.Cyan, Color.Cyan, Color.Cyan]);
            textureCube.SetData(CubeMapFace.NegativeY, [Color.Magenta, Color.Magenta, Color.Magenta, Color.Magenta]);
            textureCube.SetData(CubeMapFace.NegativeZ, [Color.Yellow, Color.Yellow, Color.Yellow, Color.Yellow]);

            _effect.CurrentTechnique = _effect.Techniques["TestTexCube"];
            _effect.Parameters["TexCube"].SetValue(textureCube);

            var grid = new ColorGridShaderTestHarness(3, 2, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(Color.Red.ToVector4(), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Green.ToVector4(), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Blue.ToVector4(), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));

            Assert.That(Color.Cyan.ToVector4(), Is.EqualTo(grid.SampleAt(0, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Magenta.ToVector4(), Is.EqualTo(grid.SampleAt(1, 1)).Using(Vector4Comparer.Epsilon));
            Assert.That(Color.Yellow.ToVector4(), Is.EqualTo(grid.SampleAt(2, 1)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TestVec2()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestVec2"];
            _effect.Parameters["Vec2"].SetValue(new Vector2(0.6f, 0.2f));

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.6f, 0.2f, 0.0f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TestVec2Array()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestVec2Array"];
            _effect.Parameters["Vec2Array"].SetValue([
                new Vector2(0.6f, 0.2f),
                new Vector2(0.2f, 0.2f),
                new Vector2(0.6f, 0.9f)
            ]);

            var grid = new ColorGridShaderTestHarness(3, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.6f, 0.2f, 0.0f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.2f, 0.2f, 0.0f, 1.0f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.6f, 0.9f, 0.0f, 1.0f), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TestVec3()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestVec3"];
            _effect.Parameters["Vec3"].SetValue(new Vector3(0.1f, 0.3f, 0.9f));

            var grid = new ColorGridShaderTestHarness(1, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.1f, 0.3f, 0.9f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TestVec3Array()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestVec3Array"];
            _effect.Parameters["Vec3Array"].SetValue([
                new Vector3(0.6f, 0.2f, 0.2f),
                new Vector3(0.2f, 0.2f, 0.7f),
                new Vector3(0.6f, 0.9f, 0.2f)
            ]);

            var grid = new ColorGridShaderTestHarness(3, 1, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.6f, 0.2f, 0.2f, 1.0f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.2f, 0.2f, 0.7f, 1.0f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.6f, 0.9f, 0.2f, 1.0f), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TestVec4()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestVec4"];
            _effect.Parameters["Vec4"].SetValue(new Vector4(0.5f, 0.75f, 0.75f, 0.5f));

            var grid = new ColorGridShaderTestHarness(1, 2, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.50f, 0.75f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.75f, 0.50f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(0, 1)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }

        [Test]
        public void TestVec4Array()
        {
            _effect.CurrentTechnique = _effect.Techniques["TestVec4Array"];
            _effect.Parameters["Vec4Array"].SetValue([
                new Vector4(0.5f, 0.75f, 0.75f, 0.5f),
                new Vector4(0.25f, 0.5f, 0.5f, 0.25f),
                new Vector4(0.0f, 0.25f, 0.25f, 0.0f)
            ]);

            var grid = new ColorGridShaderTestHarness(3, 2, gd);
            grid.CaptureShaderOutput(_effect);

            Assert.That(new Vector4(0.50f, 0.75f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(0, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.75f, 0.50f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(0, 1)).Using(Vector4Comparer.Epsilon));

            Assert.That(new Vector4(0.25f, 0.50f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(1, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.50f, 0.25f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(1, 1)).Using(Vector4Comparer.Epsilon));

            Assert.That(new Vector4(0.00f, 0.25f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(2, 0)).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0.25f, 0.00f, 0.0f, 1f), Is.EqualTo(grid.SampleAt(2, 1)).Using(Vector4Comparer.Epsilon));

            grid.Dispose();
        }
    }
#endif
}
