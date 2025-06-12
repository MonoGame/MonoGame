// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if IOS
using UIKit;
#elif ANDROID
using Android.Views;
using Android.Runtime;
#endif

// NOTE: This is the legacy graphics adapter implementation
// which should no longer be updated.  All new development
// should go into the new one.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides methods to retrieve and manipulate graphics adapters.
    /// </summary>
    public sealed class GraphicsAdapter : IDisposable
    {
        /// <summary>
        /// Defines the driver type for graphics adapter. Usable only on DirectX platforms for now.
        /// </summary>
        public enum DriverType
        {
            /// <summary>
            /// Hardware device been used for rendering. Maximum speed and performance.
            /// </summary>
            Hardware,
            /// <summary>
            /// Emulates the hardware device on CPU. Slowly, only for testing.
            /// </summary>
            Reference,
            /// <summary>
            /// Useful when <see cref="DriverType.Hardware"/> acceleration does not work.
            /// </summary>
            FastSoftware
        }

        private static ReadOnlyCollection<GraphicsAdapter> _adapters;

        private DisplayModeCollection _supportedDisplayModes;


#if IOS
		private UIScreen _screen;
        internal GraphicsAdapter(UIScreen screen)
        {
            _screen = screen;
        }
#elif DESKTOPGL
        int _displayIndex;
#else
        internal GraphicsAdapter()
        {
        }
#endif

        /// <inheritdoc />
        public void Dispose()
        {
        }

#if DESKTOPGL
        /// <summary>
        /// Gets a string used for presentation to the user.
        /// </summary>
        public string Description {
            get {
                try {
                    return MonoGame.OpenGL.GL.GetString(MonoGame.OpenGL.StringName.Renderer);
                } catch {
                    return string.Empty;
                }
            }
            private set { }
        }
#else
        string _description = string.Empty;

        /// <summary>
        /// Gets a string used for presentation to the user.
        /// </summary>
        public string Description { get { return _description; } private set { _description = value; } }
#endif

        /// <summary>
        /// Gets the current display mode.
        /// </summary>
        public DisplayMode CurrentDisplayMode
        {
            get
            {
#if IOS
                return new DisplayMode((int)(_screen.Bounds.Width * _screen.Scale),
                       (int)(_screen.Bounds.Height * _screen.Scale),
                       SurfaceFormat.Color);
#elif ANDROID
                View view = ((AndroidGameWindow)Game.Instance.Window).GameView;
                return new DisplayMode(view.Width, view.Height, SurfaceFormat.Color);
#elif DESKTOPGL
                var displayIndex = Sdl.Display.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);

                Sdl.Display.Mode mode;
                Sdl.Display.GetCurrentDisplayMode(displayIndex, out mode);

                return new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
#elif WINDOWS
                using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    var dc = graphics.GetHdc();
                    int width = GetDeviceCaps(dc, HORZRES);
                    int height = GetDeviceCaps(dc, VERTRES);
                    graphics.ReleaseHdc(dc);
                    return new DisplayMode(width, height, SurfaceFormat.Color);
                }
#else
                return new DisplayMode(800, 600, SurfaceFormat.Color);
#endif
            }
        }

        /// <summary>
        /// Gets the default adapter.
        /// </summary>
        public static GraphicsAdapter DefaultAdapter
        {
            get { return Adapters[0]; }
        }

        /// <summary>
        /// Gets a read-only collection of available adapters on the system.
        /// </summary>
        public static ReadOnlyCollection<GraphicsAdapter> Adapters
        {
            get
            {
                if (_adapters == null)
                {
#if IOS
					_adapters = new ReadOnlyCollection<GraphicsAdapter>(
						new [] {new GraphicsAdapter(UIScreen.MainScreen)});
#else
                    _adapters = new ReadOnlyCollection<GraphicsAdapter>(new[] { new GraphicsAdapter() });
#endif
                }

                return _adapters;
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

        /// <summary>
        /// Queries for support of the requested render target format on the adaptor.
        /// </summary>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="format">The requested surface format.</param>
        /// <param name="depthFormat">The requested depth stencil format.</param>
        /// <param name="multiSampleCount">The requested multisample count.</param>
        /// <param name="selectedFormat">Set to the best format supported by the adaptor for the requested surface format.</param>
        /// <param name="selectedDepthFormat">Set to the best format supported by the adaptor for the requested depth stencil format.</param>
        /// <param name="selectedMultiSampleCount">Set to the best count supported by the adaptor for the requested multisample count.</param>
        /// <returns>True if the requested format is supported by the adaptor. False if one or more of the values was changed.</returns>
		public bool QueryRenderTargetFormat(
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

            // fallback for unsupported renderTarget surface formats.
            if (selectedFormat == SurfaceFormat.Alpha8 ||
                selectedFormat == SurfaceFormat.NormalizedByte2 ||
                selectedFormat == SurfaceFormat.NormalizedByte4 ||
                selectedFormat == SurfaceFormat.Dxt1 ||
                selectedFormat == SurfaceFormat.Dxt3 ||
                selectedFormat == SurfaceFormat.Dxt5 ||
                selectedFormat == SurfaceFormat.Dxt1a ||
                selectedFormat == SurfaceFormat.Dxt1SRgb ||
                selectedFormat == SurfaceFormat.Dxt3SRgb ||
                selectedFormat == SurfaceFormat.Dxt5SRgb)
                selectedFormat = SurfaceFormat.Color;


            return (format == selectedFormat) && (depthFormat == selectedDepthFormat) && (multiSampleCount == selectedMultiSampleCount);
		}

        /*
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
        */

#if DIRECTX
        private static readonly Dictionary<SharpDX.DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<SharpDX.DXGI.Format, SurfaceFormat>
            {
                { SharpDX.DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
                { SharpDX.DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
                { SharpDX.DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
            };
#endif

        /// <summary>
        /// Gets a collection of supported display modes for the current adapter.
        /// </summary>
        public DisplayModeCollection SupportedDisplayModes
        {
            get
            {
                bool displayChanged = false;
#if DESKTOPGL
                var displayIndex = Sdl.Display.GetWindowDisplayIndex (SdlGameWindow.Instance.Handle);
                displayChanged = displayIndex != _displayIndex;
#endif
                if (_supportedDisplayModes == null || displayChanged)
                {
                    var modes = new List<DisplayMode>(new[] { CurrentDisplayMode, });

#if DESKTOPGL
                    _displayIndex = displayIndex;
                    modes.Clear();

                    var modeCount = Sdl.Display.GetNumDisplayModes(displayIndex);

                    for (int i = 0;i < modeCount;i++)
                    {
                        Sdl.Display.Mode mode;
                        Sdl.Display.GetDisplayMode(displayIndex, i, out mode);

                        // We are only using one format, Color
                        // mode.Format gets the Color format from SDL
                        var displayMode = new DisplayMode(mode.Width, mode.Height, SurfaceFormat.Color);
                        if (!modes.Contains(displayMode))
                            modes.Add(displayMode);
                    }
#elif DIRECTX
                    var dxgiFactory = new SharpDX.DXGI.Factory1();
                    var adapter = dxgiFactory.GetAdapter(0);
                    var output = adapter.Outputs[0];

                    modes.Clear();
                    foreach (var formatTranslation in FormatTranslations)
                    {
                        var displayModes = output.GetDisplayModeList(formatTranslation.Key, 0);
                        foreach (var displayMode in displayModes)
                        {
                            var xnaDisplayMode = new DisplayMode(displayMode.Width, displayMode.Height, formatTranslation.Value);
                            if (!modes.Contains(xnaDisplayMode))
                                modes.Add(xnaDisplayMode);
                        }
                    }

                    output.Dispose();
                    adapter.Dispose();
                    dxgiFactory.Dispose();
#endif
                    modes.Sort(delegate(DisplayMode a, DisplayMode b)
                    {
                        if (a == b) return 0;
                        if (a.Format <= b.Format && a.Width <= b.Width && a.Height <= b.Height) return -1;
                        else return 1;
                    });
                    _supportedDisplayModes = new DisplayModeCollection(modes);
                }

                return _supportedDisplayModes;
            }
        }

        /*
        public int VendorId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        */

        /// <summary>
        /// Gets a <see cref="System.Boolean"/> indicating whether
        /// <see cref="GraphicsAdapter.CurrentDisplayMode"/> has a
        /// Width:Height ratio corresponding to a widescreen <see cref="DisplayMode"/>.
        /// Common widescreen modes include 16:9, 16:10 and 2:1.
        /// </summary>
        public bool IsWideScreen
        {
            get
            {
                // Common non-widescreen modes: 4:3, 5:4, 1:1
                // Common widescreen modes: 16:9, 16:10, 2:1
                // XNA does not appear to account for rotated displays on the desktop
                const float limit = 4.0f / 3.0f;
                var aspect = CurrentDisplayMode.AspectRatio;
                return aspect > limit;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the specified graphics profile is supported by the current adapter.
        /// </summary>
        /// <param name="graphicsProfile">The graphics profile to check for support.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// The <paramref name="graphicsProfile"/> parameter is not a valid <see cref="GraphicsProfile"/> enum value.
        /// </exception>
        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            if(UseReferenceDevice)
                return true;

            switch(graphicsProfile)
            {
                case GraphicsProfile.Reach:
                    return true;
                case GraphicsProfile.HiDef:
                    bool result = true;
                    // TODO: check adapter capabilities...
                    return result;
                default:
                    throw new InvalidOperationException();
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
