#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif IPHONE
using MonoTouch.UIKit;
#elif ANDROID
using Android.Views;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsAdapter : IDisposable
    {
        private static ReadOnlyCollection<GraphicsAdapter> adapters;
        
        
#if MONOMAC
		private NSScreen _screen;
        internal GraphicsAdapter(NSScreen screen)
        {
            _screen = screen;
        }
#elif IPHONE
		private UIScreen _screen;
        internal GraphicsAdapter(UIScreen screen)
        {
            _screen = screen;
        }
#elif ANDROID
        private View _view;
        internal GraphicsAdapter(View screen)
        {
            _view = screen;
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
#elif IPHONE
                return new DisplayMode((int)(_screen.Bounds.Width * _screen.Scale),
                       (int)(_screen.Bounds.Height * _screen.Scale),
                       60,
                       SurfaceFormat.Color);
#elif ANDROID
                return new DisplayMode(_view.Width, _view.Height, 60, SurfaceFormat.Color);
#elif WINDOWS || LINUX

                return new DisplayMode(OpenTK.DisplayDevice.Default.Width, OpenTK.DisplayDevice.Default.Height, (int)OpenTK.DisplayDevice.Default.RefreshRate, SurfaceFormat.Color);
#else
                return new DisplayMode(800, 600, 60, SurfaceFormat.Color);
#endif
            }
        }

        public static GraphicsAdapter DefaultAdapter
        {
            get { return Adapters[0]; }
        }
        
        public static ReadOnlyCollection<GraphicsAdapter> Adapters {
            get {
                if (adapters == null) {
#if MONOMAC
                    GraphicsAdapter[] tmpAdapters = new GraphicsAdapter[NSScreen.Screens.Length];
                    for (int i=0; i<NSScreen.Screens.Length; i++) {
                        tmpAdapters[i] = new GraphicsAdapter(NSScreen.Screens[i]);
                    }
                    
                    adapters = new ReadOnlyCollection<GraphicsAdapter>(tmpAdapters);
#elif IPHONE
					adapters = new ReadOnlyCollection<GraphicsAdapter>(
						new GraphicsAdapter[] {new GraphicsAdapter(UIScreen.MainScreen)});
#elif ANDROID
                    adapters = new ReadOnlyCollection<GraphicsAdapter>(new GraphicsAdapter[] { new GraphicsAdapter(Game.Instance.Window) });
#else
                    adapters = new ReadOnlyCollection<GraphicsAdapter>(
						new GraphicsAdapter[] {new GraphicsAdapter()});
#endif
                }
                return adapters;
            }
        } 
		
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

        private DisplayModeCollection supportedDisplayModes = null;
        
        public DisplayModeCollection SupportedDisplayModes
        {
            get
            {

                if (supportedDisplayModes == null)
                {
                    List<DisplayMode> modes = new List<DisplayMode>(new DisplayMode[] { CurrentDisplayMode, });
#if WINDOWS || LINUX
                    IList<OpenTK.DisplayDevice> displays = OpenTK.DisplayDevice.AvailableDisplays;
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
#endif
                    supportedDisplayModes = new DisplayModeCollection(modes);
                }
                return supportedDisplayModes;
            }
        }

        public int VendorId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

