// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests.Graphics
{
    [NonParallelizable]
    [RunOnUiTestFixture]
    class RenderTargetCubeTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ZeroSizeShouldFailTest()
        {
            RenderTargetCube renderTarget;
            Assert.Throws<ArgumentOutOfRangeException>(() => renderTarget = new RenderTargetCube(gd, 0, false, SurfaceFormat.Color, DepthFormat.None));
        }

        [Test]
        public void NullDeviceShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
            {
                var renderTarget = new RenderTargetCube(null, 16, false, SurfaceFormat.Color, DepthFormat.None);
                renderTarget.Dispose();
            });
            GC.GetTotalMemory(true); // collect uninitialized renderTarget
        }

        [Test]
        [TestCase(1)]
        [TestCase(8)]
        [TestCase(31)]
        public void ShouldClearRenderTargetAndGetData(int size)
        {
            var dataSize = size * size;
            var renderTargetCube = new RenderTargetCube(gd, size, false, SurfaceFormat.Color, DepthFormat.Depth16);

            var colors = new[]
            {
                Color.BlanchedAlmond,
                Color.BlueViolet,
                Color.DarkSeaGreen,
                Color.ForestGreen,
                Color.IndianRed,
                Color.LightGoldenrodYellow
            };

            for (var i = 0; i < 6; i++)
            {
                gd.SetRenderTarget(renderTargetCube, (CubeMapFace) i);
                gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, colors[i], 1.0f, 0);
                gd.SetRenderTarget(null, (CubeMapFace) i);
            }

            for (var i = 0; i < 6; i++)
            {
                var readData = new Color[dataSize];
                renderTargetCube.GetData((CubeMapFace) i, readData);

                for (var j = 0; j < dataSize; j++)
                    Assert.AreEqual(colors[i], readData[j]);
            }

            renderTargetCube.Dispose();
        }

        [Test]
        [TestCase(SurfaceFormat.Color, SurfaceFormat.Color)]
        // unsupported renderTarget formats
        [TestCase(SurfaceFormat.Alpha8, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt1, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt3, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt5, SurfaceFormat.Color)]
#if !XNA        
        [TestCase(SurfaceFormat.Dxt1a, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt1SRgb, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt3SRgb, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.Dxt5SRgb, SurfaceFormat.Color)]
#endif
        [TestCase(SurfaceFormat.NormalizedByte2, SurfaceFormat.Color)]
        [TestCase(SurfaceFormat.NormalizedByte4, SurfaceFormat.Color)]
        public void PreferredSurfaceFormatTest(SurfaceFormat preferredSurfaceFormat, SurfaceFormat expectedSurfaceFormat)
        {                    
            var renderTarget = new RenderTargetCube(gd, 16, false, preferredSurfaceFormat, DepthFormat.None);
                    
            Assert.AreEqual(renderTarget.Format, expectedSurfaceFormat);
        }

        // Disposed render target cubes should not stay referenced by the GraphicsDevice.
        // See issue: https://github.com/MonoGame/MonoGame/issues/9485
        [Test]
        [TestCase(DepthFormat.None, 0)]
        [TestCase(DepthFormat.None, 4)]
        [TestCase(DepthFormat.Depth16, 0)]
        [TestCase(DepthFormat.Depth16, 4)]
        [TestCase(DepthFormat.Depth24, 0)]
        [TestCase(DepthFormat.Depth24, 4)]
        [TestCase(DepthFormat.Depth24Stencil8, 0)]
        [TestCase(DepthFormat.Depth24Stencil8, 4)]
        public void DisposeAfterUse_RenderTargetCube_DoesNotRemainReferencedByGraphicsDevice(DepthFormat depthFormat, int preferredMultiSampleCount)
        {
            WeakReference weakRef = CreateAndDisposeRenderTargetCube(depthFormat, preferredMultiSampleCount);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(
                weakRef.IsAlive,
                "Disposed RenderTargetCube was still strongly referenced by the GraphicsDevice.");
        }

        // Keep creation and disposal out of the test method so the JIT does not extend the local lifetime.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private WeakReference CreateAndDisposeRenderTargetCube(DepthFormat depthFormat, int preferredMultiSampleCount)
        {
            RenderTargetCube renderTarget = new RenderTargetCube(
                gd,
                16,
                false,
                SurfaceFormat.Color,
                depthFormat,
                preferredMultiSampleCount,
                RenderTargetUsage.DiscardContents);

            gd.SetRenderTarget(renderTarget, CubeMapFace.PositiveX);
            gd.Clear(Color.CornflowerBlue);
            gd.SetRenderTarget(null, CubeMapFace.PositiveX);

            renderTarget.Dispose();

            return new WeakReference(renderTarget);
        }

         // Creating a RenderTargetCube on the WindowsDX backend with MSAA enabled and a depth format
         // other than DepthFormat.None failed during construction with an exception
         // See: https://github.com/MonoGame/MonoGame/issues/9489
#if DIRECTX
        [Test]
        [TestCase(DepthFormat.Depth16)]
        [TestCase(DepthFormat.Depth24)]
        [TestCase(DepthFormat.Depth24Stencil8)]
        public void RenderedMultisampledRenderTargetCubeFaceWithDepthBuffer_CanBeReadBack(DepthFormat depthFormat)
        {
            RenderTargetCube rt = null;

            try
            {
                rt = new RenderTargetCube(
                    gd,
                    16,
                    false,
                    SurfaceFormat.Color,
                    depthFormat,
                    4,
                    RenderTargetUsage.DiscardContents);

                gd.SetRenderTarget(rt, CubeMapFace.PositiveX);
                gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);
                gd.SetRenderTarget(null, CubeMapFace.PositiveX);

                Color[] readData = new Color[16 * 16];
                rt.GetData(CubeMapFace.PositiveX, readData);

                for (int i = 0; i < readData.Length; i++)
                    Assert.AreEqual(Color.CornflowerBlue, readData[i]);
            }
            finally
            {
                if (rt != null)
                    rt.Dispose();
            }
        }
#endif
    }
}
