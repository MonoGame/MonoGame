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

        /*
         * A disposed render target cube should not be kept alive by references held by the
         * graphics device. This verifies that once a face has been used and the render target
         * has been unbound and disposed, the garbage collector is able to collect it.
         *
         * See issue: https://github.com/MonoGame/MonoGame/issues/9485
         *
         * - Chris <aristurtledev>
         */
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

        /*
         * The render target cube creation and disposal need to be in a separate non-inlined method.
         * If these were done in the actual test method above, the JIT could keep the local reference
         * alive, causing the test to fail even though the graphics device is no longer holding
         * a strong reference to the render target cube.
         *
         * - Chris <aristurtledev>
         */
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
    }
}
