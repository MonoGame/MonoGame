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

#if WINDOWS_STOREAPP
using Windows.UI.Xaml.Controls;
#endif

#if MONOMAC
using MonoMac.AppKit;
#elif IOS
using UIKit;
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class PresentationParameters : IDisposable
    {
        #region Constants

        public const int DefaultPresentRate = 60;

        #endregion Constants

        #region Private Fields

        private DepthFormat depthStencilFormat;
        private SurfaceFormat backBufferFormat;
        private int backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
        private int backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
        private IntPtr deviceWindowHandle;
        private int multiSampleCount;
        private bool disposed;
#if !WINRT
        private bool isFullScreen;
#endif

        #endregion Private Fields

        #region Constructors

        public PresentationParameters()
        {
            Clear();
        }

        ~PresentationParameters()
        {
            Dispose(false);
        }

        #endregion Constructors

        #region Properties

        public SurfaceFormat BackBufferFormat
        {
            get { return backBufferFormat; }
            set { backBufferFormat = value; }
        }

        public int BackBufferHeight
        {
            get { return backBufferHeight; }
            set { backBufferHeight = value; }
        }

        public int BackBufferWidth
        {
            get { return backBufferWidth; }
            set { backBufferWidth = value; }
        }

        public Rectangle Bounds 
        {
            get { return new Rectangle(0, 0, backBufferWidth, backBufferHeight); }
        }

        public IntPtr DeviceWindowHandle
        {
            get { return deviceWindowHandle; }
            set { deviceWindowHandle = value; }
        }

#if WINDOWS_STOREAPP
        [CLSCompliant(false)]
        public SwapChainBackgroundPanel SwapChainBackgroundPanel { get; set; }
#endif

        public DepthFormat DepthStencilFormat
        {
            get { return depthStencilFormat; }
            set { depthStencilFormat = value; }
        }

        public bool IsFullScreen
        {
			get
            {
#if WINRT
                // Always return true for Windows 8
                return true;
#else
				 return isFullScreen;
#endif
            }
            set
            {
#if !WINRT
                // If we are not on windows 8 set the value otherwise ignore it.
				isFullScreen = value;				
#endif
#if IOS
				UIApplication.SharedApplication.StatusBarHidden = isFullScreen;
#endif

			}
        }
		
        public int MultiSampleCount
        {
            get { return multiSampleCount; }
            set { multiSampleCount = value; }
        }
		
        public PresentInterval PresentationInterval { get; set; }

		public DisplayOrientation DisplayOrientation 
		{ 
			get; 
			set; 
		}
		
		public RenderTargetUsage RenderTargetUsage { get; set; }

        #endregion Properties


        #region Methods

        public void Clear()
        {
            backBufferFormat = SurfaceFormat.Color;
#if IOS
			// Mainscreen.Bounds does not account for the device's orientation. it ALWAYS assumes portrait
			var width = (int)(UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale);
			var height = (int)(UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale);
			
			// Flip the dimentions if we need to.
			if (TouchPanel.DisplayOrientation == DisplayOrientation.LandscapeLeft ||
			    TouchPanel.DisplayOrientation == DisplayOrientation.LandscapeRight)
			{
				width = height;
				height = (int)(UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale);
			}
			
			backBufferWidth = width;
            backBufferHeight = height;
#else
            backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
            backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;     
#endif
            deviceWindowHandle = IntPtr.Zero;
#if IOS
			isFullScreen = UIApplication.SharedApplication.StatusBarHidden;
#else
            // isFullScreen = false;
#endif
            depthStencilFormat = DepthFormat.None;
            multiSampleCount = 0;
            PresentationInterval = PresentInterval.Default;
            DisplayOrientation = Microsoft.Xna.Framework.DisplayOrientation.Default;
        }

        public PresentationParameters Clone()
        {
            PresentationParameters clone = new PresentationParameters();
            clone.backBufferFormat = this.backBufferFormat;
            clone.backBufferHeight = this.backBufferHeight;
            clone.backBufferWidth = this.backBufferWidth;
            clone.deviceWindowHandle = this.deviceWindowHandle;
            clone.disposed = this.disposed;
            clone.IsFullScreen = this.IsFullScreen;
            clone.depthStencilFormat = this.depthStencilFormat;
            clone.multiSampleCount = this.multiSampleCount;
            clone.PresentationInterval = this.PresentationInterval;
            clone.DisplayOrientation = this.DisplayOrientation;
            return clone;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                if (disposing)
                {
                    // Dispose managed resources
                }
                // Dispose unmanaged resources
            }
        }

        #endregion Methods

    }
}
