// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

namespace Microsoft.Xna.Framework.Graphics
{
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

        
#if MONOMAC
		private NSScreen _screen;
        internal GraphicsAdapter(NSScreen screen)
        {
            _screen = screen;
        }
#elif IOS
		private UIScreen _screen;
        internal GraphicsAdapter(UIScreen screen)
        {
            _screen = screen;
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
#if MONOMAC
                //Dummy values until MonoMac implements Quartz Display Services
                SurfaceFormat format = SurfaceFormat.Color;
                
                return new DisplayMode((int)_screen.Frame.Width,
                                       (int)_screen.Frame.Height,
                                       format);
#elif IOS
                return new DisplayMode((int)(_screen.Bounds.Width * _screen.Scale),
                       (int)(_screen.Bounds.Height * _screen.Scale),
                       SurfaceFormat.Color);
#elif ANDROID
                View view = ((AndroidGameWindow)Game.Instance.Window).GameView;
                return new DisplayMode(view.Width, view.Height, SurfaceFormat.Color);
#elif DESKTOPGL

                return new DisplayMode(OpenTK.DisplayDevice.Default.Width, OpenTK.DisplayDevice.Default.Height, SurfaceFormat.Color);
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

        public static GraphicsAdapter DefaultAdapter
        {
            get { return Adapters[0]; }
        }
        
        public static ReadOnlyCollection<GraphicsAdapter> Adapters 
        {
            get 
            {
                if (_adapters == null) 
                {
#if MONOMAC
                    GraphicsAdapter[] tmpAdapters = new GraphicsAdapter[NSScreen.Screens.Length];
                    for (int i=0; i<NSScreen.Screens.Length; i++) {
                        tmpAdapters[i] = new GraphicsAdapter(NSScreen.Screens[i]);
                    }
                    
                    _adapters = new ReadOnlyCollection<GraphicsAdapter>(tmpAdapters);
#elif IOS
					_adapters = new ReadOnlyCollection<GraphicsAdapter>(
						new [] {new GraphicsAdapter(UIScreen.MainScreen)});
#else
                    _adapters = new ReadOnlyCollection<GraphicsAdapter>(new[] {new GraphicsAdapter()});
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
            get { return UseDriverType==DriverType.Reference; }
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
        */
       
#if DIRECTX && !WINDOWS_PHONE
        private static readonly Dictionary<SharpDX.DXGI.Format, SurfaceFormat> FormatTranslations = new Dictionary<SharpDX.DXGI.Format, SurfaceFormat>
            {
                { SharpDX.DXGI.Format.R8G8B8A8_UNorm, SurfaceFormat.Color },
                { SharpDX.DXGI.Format.B8G8R8A8_UNorm, SurfaceFormat.Color },
                { SharpDX.DXGI.Format.B5G6R5_UNorm, SurfaceFormat.Bgr565 },
            };
#endif

        public DisplayModeCollection SupportedDisplayModes
        {
            get
            {

                if (_supportedDisplayModes == null)
                {
                    var modes = new List<DisplayMode>(new[] { CurrentDisplayMode, });

#if DESKTOPGL
                    
					//IList<OpenTK.DisplayDevice> displays = OpenTK.DisplayDevice.AvailableDisplays;
					var displays = new List<OpenTK.DisplayDevice>();

					OpenTK.DisplayIndex[] displayIndices = {
						OpenTK.DisplayIndex.First,
						OpenTK.DisplayIndex.Second,
						OpenTK.DisplayIndex.Third,
						OpenTK.DisplayIndex.Fourth,
						OpenTK.DisplayIndex.Fifth,
						OpenTK.DisplayIndex.Sixth,
					};

					foreach(var displayIndex in displayIndices) 
					{
						var currentDisplay = OpenTK.DisplayDevice.GetDisplay(displayIndex);
						if(currentDisplay!= null) displays.Add(currentDisplay);
					}

                    if (displays.Count > 0)
                    {
                        modes.Clear();
                        foreach (OpenTK.DisplayDevice display in displays)
                        {
                            foreach (OpenTK.DisplayResolution resolution in display.AvailableResolutions)
                            {                                
                                SurfaceFormat format = SurfaceFormat.Color;
                                switch (resolution.BitsPerPixel)
                                {
                                    case 32: format = SurfaceFormat.Color; break;
                                    case 16: format = SurfaceFormat.Bgr565; break;
                                    case 8: format = SurfaceFormat.Bgr565; break;
                                    default:
                                        break;
                                }
                                // Just report the 32 bit surfaces for now
                                // Need to decide what to do about other surface formats
                                if (format == SurfaceFormat.Color)
                                {
                                    var displayMode = new DisplayMode(resolution.Width, resolution.Height, format);
                                    if (!modes.Contains(displayMode))
                                        modes.Add(displayMode);
                                }
                            }

                        }
                    }
#elif DIRECTX && !WINDOWS_PHONE
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

#if WINDOWS && !OPENGL
        [System.Runtime.InteropServices.DllImport("gdi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        private const int HORZRES = 8;
        private const int VERTRES = 10;
#endif
    }
}
