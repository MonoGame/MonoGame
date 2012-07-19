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

#if WINRT 
using Windows.UI.Xaml.Controls;
#endif

#if MONOMAC
using MonoMac.AppKit;
#elif IPHONE
using MonoTouch.UIKit;
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
        private int backBufferHeight = _defaultBackBufferHeight;
        private int backBufferWidth = _defaultBackBufferWidth;
        private IntPtr deviceWindowHandle;
        private bool isFullScreen;
        private int multiSampleCount;
        private bool disposed;
        
        internal static readonly int _defaultBackBufferHeight = 480;
        internal static readonly int _defaultBackBufferWidth = 800;		

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
		
#if WINRT 
        public SwapChainBackgroundPanel SwapChainPanel { get; set; }
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
				 return isFullScreen;
            }
            set
            {
				isFullScreen = value;				
#if IPHONE
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
#if IPHONE
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
			backBufferWidth = _defaultBackBufferWidth;
            backBufferHeight = _defaultBackBufferHeight;     
#endif
            deviceWindowHandle = IntPtr.Zero;
#if IPHONE
			isFullScreen = UIApplication.SharedApplication.StatusBarHidden;
#else
            // isFullScreen = false;
#endif
            depthStencilFormat = DepthFormat.None;
            multiSampleCount = 0;
            PresentationInterval = PresentInterval.Default;
            DisplayOrientation = DisplayOrientation.Default;
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
