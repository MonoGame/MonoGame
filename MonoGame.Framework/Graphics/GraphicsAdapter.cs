#region License
// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
using MonoMac.Foundation;
#else
using AppKit;
using Foundation;
#endif
#elif IOS
using UIKit;
#elif ANDROID
using Android.Views;
using Android.Runtime;
#endif
#if DIRECTX
using SharpDX.DXGI;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsAdapter : IDisposable
    {
        /// <summary>
        /// Defines the driver type for graphics adapter. Usable only on DirectX platforms for now.
        /// </summary>
        public enum DriverType
        {
            /// <summary>Hardware device been used for rendering. Maximum speed and performance.</summary>
            Hardware,
            /// <summary>Emulates the hardware device on CPU. Slowly, only for testing.</summary>
            Reference,
            /// <summary>Useful when <see cref="Hardware"/> acceleration does not work.</summary>
            FastSoftware
        }

        private static ReadOnlyCollection<GraphicsAdapter> adapters;

        private DisplayModeCollection supportedDisplayModes;

        private const int PrimaryAdapterIndex = 0;
#if MONOMAC
        private NSScreen screen;
        internal GraphicsAdapter(NSScreen screen)
        {
            this.screen = screen;
        }
#elif IOS
        private UIScreen screen;
        internal GraphicsAdapter(UIScreen screen)
        {
            this.screen = screen;
        }
#elif DESKTOPGL
        int displayIndex;
#elif DIRECTX
        private const int PrimaryOutputIndex = 0;
        private readonly Adapter1 adapter;
        internal GraphicsAdapter(Adapter1 adapter)
        {
            this.adapter = adapter;
        }
#else
        internal GraphicsAdapter()
        {
        }
#endif

        public void Dispose()
        {
        }

        public DisplayMode CurrentDisplayMode
        {
            get
            {
                var width = 800;
                var height = 600;
                const SurfaceFormat defaultSurfaceFormat = SurfaceFormat.Color;
#if MONOMAC
                // Dummy values until MonoMac implements Quartz Display Services
                width = (int)screen.Frame.Width;
                height = (int)screen.Frame.Height;
#elif IOS
                width = (int)(screen.Bounds.Width * screen.Scale);
                height = (int)(screen.Bounds.Height * screen.Scale);
#elif ANDROID
                View view = ((AndroidGameWindow)Game.Instance.Window).GameView;
                width = view.Width;
                height = view.Height;
#elif DESKTOPGL
                var displayIndex = Sdl.Display.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);

                Sdl.Display.Mode mode;
                Sdl.Display.GetCurrentDisplayMode(displayIndex, out mode);

                width = mode.Width;
                height = mode.Height;
#elif WINDOWS
                using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    var dc = graphics.GetHdc();
                    width = GetDeviceCaps(dc, HORZRES);
                    height = GetDeviceCaps(dc, VERTRES);
                    graphics.ReleaseHdc(dc);
                }
#elif DIRECTX
                using (var factory = new Factory1())
                {
                    // Returns primary display from primary adapter since DXGI 1.1!
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/bb205075%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396#WARP_new_for_Win8
                    using (var primaryAdapter = factory.GetAdapter1(PrimaryAdapterIndex))
                    {
                        using (var output = primaryAdapter.GetOutput(PrimaryOutputIndex))
                        {
                            width = output.Description.DesktopBounds.Right;
                            height = output.Description.DesktopBounds.Bottom;
                        }
                    }
                }
#endif
                return new DisplayMode(width, height, defaultSurfaceFormat);
            }
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return Adapters[PrimaryAdapterIndex]; }
        }

        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get
            {
                if (adapters == null)
                {
#if MONOMAC
                    GraphicsAdapter[] tmpAdapters = new GraphicsAdapter[NSScreen.Screens.Length];
                    for (int i=0; i<NSScreen.Screens.Length; ++i)
                    {
                        tmpAdapters[i] = new GraphicsAdapter(NSScreen.Screens[i]);
                    }
                    
                    adapters = new ReadOnlyCollection<GraphicsAdapter>(tmpAdapters);
#elif IOS
                    adapters = new ReadOnlyCollection<GraphicsAdapter>(
                        new [] {new GraphicsAdapter(UIScreen.MainScreen)});
#elif DIRECTX
                    GraphicsAdapter[] results;
                    using (var factory = new Factory1())
                    {
                        var adapterCount = factory.GetAdapterCount();

                        results = new GraphicsAdapter[adapterCount];
                        for (var i = 0; i < adapterCount; ++i)
                        {
                            using (var tmpAdapter = factory.GetAdapter1(i))
                            {
                                // Filter out software adapters
                                //if ((tmpAdapter.Description1.Flags & AdapterFlags.Software) == 0)
                                //{
                                results[i] = new GraphicsAdapter(tmpAdapter);
                                //}
                            }
                        }
                    }

                    adapters = new ReadOnlyCollection<GraphicsAdapter>(results);
#else
                    adapters = new ReadOnlyCollection<GraphicsAdapter>(new[] {new GraphicsAdapter()});
#endif
                }

                return adapters;
            }
        }

        /// <summary>
        /// Used to request creation of the reference graphics device, 
        /// or the default hardware accelerated device (when set to false).
        /// </summary>
        /// <remarks>
        /// This only works on DirectX platforms where a reference graphics
        /// device is available and must be defined before the graphics device
        /// is created. It defaults to false.
        /// </remarks>
        public static bool UseReferenceDevice
        {
            get { return UseDriverType == DriverType.Reference; }
            set { UseDriverType = value ? DriverType.Reference : DriverType.Hardware; }
        }

        /// <summary>
        /// Used to request creation of a specific kind of driver.
        /// </summary>
        /// <remarks>
        /// These values only work on DirectX platforms and must be defined before the graphics device
        /// is created. <see cref="DriverType.Hardware"/> by default.
        /// </remarks>
        public static DriverType UseDriverType { get; set; }

#if DIRECTX
        /*
        public bool QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount)
        {
            throw new NotImplementedException();
        }
        */
        public string Description
        {
            get { return adapter.Description1.Description; }
        }

        public int DeviceId
        {
            get { return adapter.Description1.DeviceId; }
        }

        /*
        public Guid DeviceIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        */
        public string DeviceName
        {
            get { return adapter.Outputs[PrimaryOutputIndex].Description.DeviceName; }
        }

        /*
        public string DriverDll
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version DriverVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        */
        public bool IsDefaultAdapter
        {
            get
            {
                bool isResult;
                using (var factory = new Factory1())
                {
                    using (var primaryAdapter = factory.GetAdapter1(PrimaryAdapterIndex))
                    {
                        isResult = adapter.Equals(primaryAdapter);
                    }
                }

                return isResult;
            }
        }

        public IntPtr MonitorHandle
        {
            get { return adapter.Outputs[PrimaryOutputIndex].Description.MonitorHandle; }
        }

        public int Revision
        {
            get { return adapter.Description1.Revision; }
        }

        public int SubSystemId
        {
            get { return adapter.Description1.SubsystemId; }
        }

        public int VendorId
        {
            get { return adapter.Description1.VendorId; }
        }

        public long DeviceIdentifier1
        {
            get { return adapter.Description1.Luid; }
        }

        public IntPtr Handle
        {
            get { return adapter.NativePointer; }
        }
#endif

        /*
        public bool QueryRenderTargetFormat(
            GraphicsProfile graphicsProfile,
            SurfaceFormat format,
            DepthFormat depthFormat,
            int multiSampleCount,
            out SurfaceFormat selectedFormat,
            out DepthFormat selectedDepthFormat,
            out int selectedMultiSampleCount)
        {
            throw new NotImplementedException();
        }

        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int DeviceId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Guid DeviceIdentifier
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DeviceName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string DriverDll
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version DriverVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDefaultAdapter
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsWideScreen
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IntPtr MonitorHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Revision
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int SubSystemId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int VendorId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        */

#if DIRECTX && !WINDOWS_PHONE
        private static readonly Dictionary<Format, SurfaceFormat> FormatTranslations = new Dictionary<Format, SurfaceFormat>
            {
                { Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
                { Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
                { Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
            };
#endif

        public DisplayModeCollection SupportedDisplayModes
        {
            get
            {
#if DESKTOPGL
                var newDisplayIndex = Sdl.Display.GetWindowDisplayIndex (SdlGameWindow.Instance.Handle);
                if (supportedDisplayModes == null || newDisplayIndex != displayIndex)
                {
#else
                if (supportedDisplayModes == null)
                {
#endif
                    var modes = new List<DisplayMode>();
#if DESKTOPGL
                    displayIndex = newDisplayIndex;
                    
                    var modeCount = Sdl.Display.GetNumDisplayModes(displayIndex);

                    for (var i = 0; i < modeCount; ++i)
                    {
                        Sdl.Display.Mode mode;
                        Sdl.Display.GetDisplayMode(displayIndex, i, out mode);

                        // We are only using one format, Color
                        // mode.Format gets the Color format from SDL
                        var displayMode = new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
                        if (!modes.Contains(displayMode))
                            modes.Add(displayMode);
                    }
#elif DIRECTX && !WINDOWS_PHONE
                    using (var dxgiFactory = new Factory1())
                    {
                        using (var primaryAdapter = dxgiFactory.GetAdapter(PrimaryAdapterIndex))
                        {
                            using (var primaryOutput = primaryAdapter.Outputs[PrimaryOutputIndex])
                            {
                                foreach (var formatTranslation in FormatTranslations)
                                {
                                    var displayModes = primaryOutput.GetDisplayModeList(formatTranslation.Key, 0);
                                    foreach (var displayMode in displayModes)
                                    {
                                        var mode = new DisplayMode(displayMode.Width, displayMode.Height, formatTranslation.Value);
                                        if (!modes.Contains(mode))
                                        {
                                            modes.Add(mode);
                                        }
                                    }
                                }
                            }
                        }
                    }
#endif
                    modes.Sort(delegate (DisplayMode a, DisplayMode b)
                    {
                        if (a.Equals(b))
                        {
                            return 0;
                        }
                        // FIXME: XNA did this in opposite order on PC with DirectX! It would also align better when primary display index = 0 compares with the highest possible screen resolution index = 0.
                        if (a.Format <= b.Format &&
                            a.Width <= b.Width &&
                            a.Height <= b.Height)
                        {
                            return -1;
                        }

                        return 1;
                    });

                    supportedDisplayModes = new DisplayModeCollection(modes);
                }

                return supportedDisplayModes;
            }
        }

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> indicating whether
        /// <see cref="GraphicsAdapter.CurrentDisplayMode"/> has a
        /// Width:Height ratio corresponding to a widescreen <see cref="DisplayMode"/>.
        /// Common widescreen modes include 21:9, 16:9, 16:10 and 2:1.
        /// </summary>
        public bool IsWideScreen
        {
            get
            {
                // Common non-widescreen modes: 4:3 (1.3333), 5:4 (1.25), 1:1 (1)
                // Common widescreen modes: 21:9 (2.3333), 16:9 (1.7777), 16:10 (1.6), 2:1 (2)
                // XNA does not appear to account for rotated displays on the desktop
                const float limit = 4.0f / 3.0f;
                return CurrentDisplayMode.AspectRatio > limit;
            }
        }

#if WINDOWS && !OPENGL
        [System.Runtime.InteropServices.DllImport("gdi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        private const int HORZRES = 8;
        private const int VERTRES = 10;
#endif
    }
}
