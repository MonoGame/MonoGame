// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif IOS
using MonoTouch.UIKit;
#elif ANDROID
using Android.Views;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsAdapter : IDisposable
    {
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
                int refreshRate = 60;
                SurfaceFormat format = SurfaceFormat.Color;
                
                return new DisplayMode((int)_screen.Frame.Width,
                                       (int)_screen.Frame.Height,
                                       refreshRate,
                                       format);
#elif IOS
                return new DisplayMode((int)(_screen.Bounds.Width * _screen.Scale),
                       (int)(_screen.Bounds.Height * _screen.Scale),
                       60,
                       SurfaceFormat.Color);
#elif ANDROID
                View view = ((AndroidGameWindow)Game.Instance.Window).GameView;
                return new DisplayMode(view.Width, view.Height, 60, SurfaceFormat.Color);
#elif (WINDOWS && OPENGL) || LINUX

                return new DisplayMode(OpenTK.DisplayDevice.Default.Width, OpenTK.DisplayDevice.Default.Height, (int)OpenTK.DisplayDevice.Default.RefreshRate, SurfaceFormat.Color);
#elif WINDOWS
                var dc = System.Drawing.Graphics.FromHwnd(IntPtr.Zero).GetHdc();
                return new DisplayMode(GetDeviceCaps(dc, HORZRES), GetDeviceCaps(dc, VERTRES), GetDeviceCaps(dc, VREFRESH), SurfaceFormat.Color);
#else
                return new DisplayMode(800, 600, 60, SurfaceFormat.Color);
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
        /// Used to request creation of the reference graphics device.
        /// </summary>
        /// <remarks>
        /// This only works on DirectX platforms where a reference graphics
        /// device is available and must be defined before the graphics device
        /// is created.  It defaults to false.
        /// </remarks>
        public static bool UseReferenceDevice { get; set; }

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
       
        public DisplayModeCollection SupportedDisplayModes
        {
            get
            {

                if (_supportedDisplayModes == null)
                {
                    var modes = new List<DisplayMode>(new[] { CurrentDisplayMode, });

#if (WINDOWS && OPENGL) || LINUX
                    
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
                                    modes.Add(new DisplayMode(resolution.Width, resolution.Height, (int)resolution.RefreshRate, format));
                                }
                            }

                        }
                    }
#elif DIRECTX && !WINDOWS_PHONE
                    var dxgiFactory = new SharpDX.DXGI.Factory1();
                    var adapter = dxgiFactory.GetAdapter(0);
                    var output = adapter.Outputs[0];
                    var displayModes = output.GetDisplayModeList(SharpDX.DXGI.Format.R8G8B8A8_UNorm, 0);

                    modes.Clear();
                    foreach (var displayMode in displayModes)
                    {
                        int refreshRate = (int)Math.Round(displayMode.RefreshRate.Numerator / (float)displayMode.RefreshRate.Denominator);
                        modes.Add(new DisplayMode(displayMode.Width, displayMode.Height, refreshRate, SurfaceFormat.Color));
                    }
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
        private const int VREFRESH = 116;
#endif
    }
}
