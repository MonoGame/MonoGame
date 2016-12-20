// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SharpDX.Direct3D;
using SharpDX.DXGI;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class GraphicsAdapter
    {
        SharpDX.DXGI.Adapter1 _adapter;

        private static void PlatformInitializeAdapters(out ReadOnlyCollection<GraphicsAdapter> adapters)
        {
            var factory = new SharpDX.DXGI.Factory1();

            var adapterCount = factory.GetAdapterCount();
            var adapterList = new List<GraphicsAdapter>(adapterCount);

            for (var i = 0; i < adapterCount; i++)
            {
                var device = factory.GetAdapter1(i);

                var monitorCount = device.GetOutputCount();
                for (var j = 0; j < monitorCount; j++)
                {
                    var monitor = device.GetOutput(j);

                    var adapter = CreateAdapter(device, monitor);
                    adapterList.Add(adapter);

                    monitor.Dispose();
                }
            }

            factory.Dispose();

            adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
        }

        private static readonly Dictionary<SharpDX.DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<SharpDX.DXGI.Format, SurfaceFormat>
        {
            { SharpDX.DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
            { SharpDX.DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
            { SharpDX.DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
        };

        private static GraphicsAdapter CreateAdapter(SharpDX.DXGI.Adapter1 device, SharpDX.DXGI.Output monitor)
        {            
            var adapter = new GraphicsAdapter();
            adapter._adapter = device;

            adapter.DeviceName = monitor.Description.DeviceName.TrimEnd(new char[] {'\0'});
            adapter.Description = device.Description1.Description.TrimEnd(new char[] {'\0'});
            adapter.DeviceId = device.Description1.DeviceId;
            adapter.Revision = device.Description1.Revision;
            adapter.VendorId = device.Description1.VendorId;
            adapter.SubSystemId = device.Description1.SubsystemId;
            adapter.MonitorHandle = monitor.Description.MonitorHandle;

#if WINDOWS_UAP
            var desktopWidth = monitor.Description.DesktopBounds.Right - monitor.Description.DesktopBounds.Left;
            var desktopHeight = monitor.Description.DesktopBounds.Bottom - monitor.Description.DesktopBounds.Top;
#else
            var desktopWidth = monitor.Description.DesktopBounds.Width;
            var desktopHeight = monitor.Description.DesktopBounds.Height;
#endif

            var modes = new List<DisplayMode>();

            foreach (var formatTranslation in FormatTranslations)
            {
                SharpDX.DXGI.ModeDescription[] displayModes;

                // This can fail on headless machines, so just assume the desktop size
                // is a valid mode and return that... so at least our unit tests work.
                try
                {
                    displayModes = monitor.GetDisplayModeList(formatTranslation.Key, 0);
                }
                catch (SharpDX.SharpDXException)
                {
                    var mode = new DisplayMode(desktopWidth, desktopHeight, SurfaceFormat.Color);
                    modes.Add(mode);
                    adapter._currentDisplayMode = mode;
                    break;
                }


                foreach (var displayMode in displayModes)
                {
                    var mode = new DisplayMode(displayMode.Width, displayMode.Height, formatTranslation.Value);

                    // Skip duplicate modes with the same width/height/formats.
                    if (modes.Contains(mode))
                        continue;

                    modes.Add(mode);

                    if (mode.Width == desktopWidth && mode.Height == desktopHeight && mode.Format == SurfaceFormat.Color)
                    {
                        if (adapter._currentDisplayMode == null)
                            adapter._currentDisplayMode = mode;
                    }
                }
            }

            adapter._supportedDisplayModes = new DisplayModeCollection(modes);

            return adapter;
        }

        protected bool PlatformIsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if(UseReferenceDevice)
                return true;

            switch(graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(_adapter, FeatureLevel.Level_9_1);
                case GraphicsProfile.HiDef:
                    return SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(_adapter, FeatureLevel.Level_10_0);
                default:
                    throw new InvalidOperationException();
            }
        }

        static int NextLowestPowerOf2(int x)
        {
            x = x | (x >> 1);
            x = x | (x >> 2);
            x = x | (x >> 4);
            x = x | (x >> 8);
            x = x | (x >> 16);
            return x - (x >> 1);
        }

        bool PlatformQueryBackBufferFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount)
        {
            selectedFormat = format;
            selectedDepthFormat = depthFormat;

            // 16-bit formats are not supported for displays
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ff471325(v=vs.85).aspx
            switch (format)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Rgba1010102:
                    break;

                default:
                    selectedFormat = SurfaceFormat.Color;
                    break;
            }

            // Direct3D 11 does not support a 24-bit only depth buffer. It has the D24S8 format.
            if (depthFormat == DepthFormat.Depth24)
                selectedDepthFormat = DepthFormat.Depth24Stencil8;

            // Set to a power of two less than or equal to 8
            selectedMultiSampleCount = NextLowestPowerOf2(multiSampleCount);
            if (selectedMultiSampleCount > 8)
                selectedMultiSampleCount = 8;

            return (format == selectedFormat) && (depthFormat == selectedDepthFormat) && (multiSampleCount == selectedMultiSampleCount);
        }

        bool PlatformQueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount)
        {
            selectedFormat = format;
            selectedDepthFormat = depthFormat;

            // 16-bit formats are not supported until DXGI 1.2 (Direct3D 11.1)
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ff471325(v=vs.85).aspx
            switch (format)
            {
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                    selectedFormat = SurfaceFormat.Color;
                    break;
            }

            // Direct3D 11 does not support a 24-bit only depth buffer. It has the D24S8 format.
            if (depthFormat == DepthFormat.Depth24)
                selectedDepthFormat = DepthFormat.Depth24Stencil8;

            // Set to a power of two less than or equal to 8
            selectedMultiSampleCount = NextLowestPowerOf2(multiSampleCount);
            if (selectedMultiSampleCount > 8)
                selectedMultiSampleCount = 8;

            return (format == selectedFormat) && (depthFormat == selectedDepthFormat) && (multiSampleCount == selectedMultiSampleCount);
        }
    }
}
