// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class GraphicsAdapter
    {
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

                device.Dispose();
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
            selectedMultiSampleCount = multiSampleCount;

            SharpDX.DXGI.Adapter adapter;
            adapter.Outputs[0].

            // Fallbacks for formats that may not be supported for back buffers
            var formatSupport = _d3dDevice.CheckFormatSupport(SharpDXHelper.ToFormat(format));
            if (((long)formatSupport & (long)FormatSupport.Display) == 0)
                selectedFormat = SurfaceFormat.Color;
            if (depthFormat != DepthFormat.None)
            {
                formatSupport = _d3dDevice.CheckFormatSupport(SharpDXHelper.ToFormat(depthFormat));
                if (((long)formatSupport & (long)FormatSupport.DepthStencil) == 0)
                    selectedDepthFormat = DepthFormat.Depth24Stencil8;
            }

            return true;
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
            bool result = true;
            selectedFormat = format;
            selectedDepthFormat = depthFormat;
            selectedMultiSampleCount = multiSampleCount;

            if (multiSampleCount > 0)
            {
                // Fallbacks for formats that may not be supported for render targets
                var formatSupport = _d3dDevice.CheckFormatSupport(SharpDXHelper.ToFormat(format));
                if (((long)formatSupport & (long)FormatSupport.RenderTarget) == 0)
                {
                    selectedFormat = SurfaceFormat.Color;
                    result = false;
                }
            }
            else
            {
                // Fallbacks for formats that may not be supported for render targets
                var formatSupport = _d3dDevice.CheckFormatSupport(SharpDXHelper.ToFormat(format));
                if (((long)formatSupport & (long)FormatSupport.MultisampleRenderTarget) == 0)
                {
                    selectedFormat = SurfaceFormat.Color;
                    result = false;
                }
            }

            if (depthFormat != DepthFormat.None)
            {
                var formatSupport = _d3dDevice.CheckFormatSupport(SharpDXHelper.ToFormat(depthFormat));
                if (((long)formatSupport & (long)FormatSupport.DepthStencil) == 0)
                {
                    selectedDepthFormat = DepthFormat.Depth24Stencil8;
                    result = false;
                }
            }

            return result;
        }
    }
}
