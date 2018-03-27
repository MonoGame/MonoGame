// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

// HACK: Only enable for XNA and DirectX which are the 
// only platforms which currently correctly implement 
// the GraphicsAdapter API.
#if XNA || DIRECTX

namespace MonoGame.Tests.Graphics
{
    class GraphicsAdapterTest
    {
        private static bool Equals(DisplayMode m1, DisplayMode m2)
        {
            return m1.Width == m2.Width &&
                   m1.Height == m2.Height &&
                   m1.Format == m2.Format &&
                   m1.AspectRatio == m2.AspectRatio &&
                   m1.TitleSafeArea == m2.TitleSafeArea;
        }

        [Test]
        public static void Adapters()
        {
            var adapters = GraphicsAdapter.Adapters;
            Assert.IsNotNull(adapters);
            Assert.GreaterOrEqual(adapters.Count, 1);

            var defaultAdapterCount = 0;

            foreach (var adapter in adapters)
            {
                Assert.IsNotNull(adapter);

                if (adapter.IsDefaultAdapter)
                    defaultAdapterCount++;
                Assert.LessOrEqual(defaultAdapterCount, 1);

                Assert.That(adapter.DeviceName, Is.Not.Null.Or.Empty);
                Assert.That(adapter.Description, Is.Not.Null.Or.Empty);
                Assert.AreNotEqual(0, adapter.DeviceId);
                Assert.AreNotEqual(IntPtr.Zero, adapter.MonitorHandle);
                Assert.AreNotEqual(0, adapter.VendorId);
                Assert.AreNotEqual(adapter.SubSystemId, 0);
                Assert.GreaterOrEqual(adapter.Revision, 0);

                Assert.IsNotNull(adapter.CurrentDisplayMode); 
                Assert.IsNotNull(adapter.SupportedDisplayModes);
                Assert.GreaterOrEqual(adapter.SupportedDisplayModes.Count(), 1);
                Assert.AreEqual(1, adapter.SupportedDisplayModes.Count(m => Equals(m, adapter.CurrentDisplayMode)));

                // Seems like XNA treats aspect ratios above 16:10 as wide screen. A 1680x1050 display (exactly 16:10) was considered not to be wide screen.
                // MonoGame considers ratios equal or greater than 16:10 to be wide screen.
                const float minWideScreenAspect = 16.0f / 10.0f;
#if XNA
                var isWidescreen = adapter.CurrentDisplayMode.AspectRatio > minWideScreenAspect;
#else
                var isWidescreen = adapter.CurrentDisplayMode.AspectRatio >= minWideScreenAspect;
#endif
                Assert.AreEqual(isWidescreen, adapter.IsWideScreen); 

                foreach (var mode in adapter.SupportedDisplayModes)
                {
                    Assert.Greater(mode.Width, 0);
                    Assert.LessOrEqual(mode.Width, 3840);
                    Assert.Greater(mode.Height, 0);
                    Assert.LessOrEqual(mode.Height, 3840);

                    var aspect = mode.Width / (float)mode.Height;
                    Assert.AreEqual(aspect, mode.AspectRatio);

                    Assert.GreaterOrEqual((int)mode.Format, 0);

                    Assert.GreaterOrEqual(mode.TitleSafeArea.Left, 0);
                    Assert.GreaterOrEqual(mode.TitleSafeArea.Top, 0);
                    Assert.Less(mode.TitleSafeArea.Left, mode.TitleSafeArea.Right);
                    Assert.Less(mode.TitleSafeArea.Top, mode.TitleSafeArea.Bottom);
                    Assert.LessOrEqual(mode.TitleSafeArea.Width, mode.Width);
                    Assert.LessOrEqual(mode.TitleSafeArea.Height, mode.Height);
                }
            }
        }

        [Test]
        public static void DefaultAdapter()
        {
            var adapter = GraphicsAdapter.DefaultAdapter;
            Assert.IsNotNull(adapter);
            Assert.IsTrue(adapter.IsDefaultAdapter);
            Assert.Contains(adapter, GraphicsAdapter.Adapters);
        }

        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Color, SurfaceFormat.Color, true)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Color, SurfaceFormat.Color, true)]
        // unsupported renderTarget formats
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Alpha8, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Alpha8, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt1, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt1, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt3, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt3, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt5, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt5, SurfaceFormat.Color, false)]
#if !XNA        
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt1a, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt1a, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt1SRgb, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt1SRgb, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt3SRgb, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt3SRgb, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.Dxt5SRgb, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.Dxt5SRgb, SurfaceFormat.Color, false)]
#endif
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.NormalizedByte2, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.NormalizedByte2, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.Reach, SurfaceFormat.NormalizedByte4, SurfaceFormat.Color, false)]
        [TestCase(GraphicsProfile.HiDef, SurfaceFormat.NormalizedByte4, SurfaceFormat.Color, false)]
        public static void QueryRenderTargetFormat_preferredSurface(GraphicsProfile graphicsProfile, SurfaceFormat preferredSurfaceFormat, SurfaceFormat expectedSurfaceFormat, bool expectedIsSupported)
        {
            var adapter = GraphicsAdapter.DefaultAdapter;

            SurfaceFormat selectedFormat;
            DepthFormat selectedDepthFormat;
            int selectedMultiSampleCount;
            bool isSupported = adapter.QueryRenderTargetFormat(graphicsProfile, preferredSurfaceFormat, DepthFormat.None, 0,
                out selectedFormat, out selectedDepthFormat, out selectedMultiSampleCount);

            Assert.AreEqual(isSupported, expectedIsSupported);
            Assert.AreEqual(selectedFormat, expectedSurfaceFormat);
            Assert.AreEqual(selectedDepthFormat, DepthFormat.None);
            Assert.AreEqual(selectedMultiSampleCount, 0);
        }
    }
}

#endif // XNA || DIRECTX