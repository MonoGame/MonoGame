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

namespace MonoGame.Tests.Framework
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

                Assert.IsNotNullOrEmpty(adapter.DeviceName);
                Assert.IsNotNullOrEmpty(adapter.Description);
                Assert.AreNotEqual(0, adapter.DeviceId);
                Assert.AreNotEqual(IntPtr.Zero, adapter.MonitorHandle);
                Assert.AreNotEqual(0, adapter.VendorId);
                Assert.GreaterOrEqual(adapter.SubSystemId, 0);
                Assert.GreaterOrEqual(adapter.Revision, 0);

                Assert.IsNotNull(adapter.CurrentDisplayMode); 
                Assert.IsNotNull(adapter.SupportedDisplayModes);
                Assert.GreaterOrEqual(adapter.SupportedDisplayModes.Count(), 1);
                Assert.AreEqual(1, adapter.SupportedDisplayModes.Count(m => Equals(m, adapter.CurrentDisplayMode)));

                // Seems like XNA treats aspect ratios above 16:10 as wide screen.
                const float minWideScreenAspect = 16.0f / 10.0f;
                var isWidescreen = adapter.CurrentDisplayMode.AspectRatio >= minWideScreenAspect;
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
    }
}

#endif // XNA || DIRECTX