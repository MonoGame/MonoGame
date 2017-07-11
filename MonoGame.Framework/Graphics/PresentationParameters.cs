// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if WINDOWS_UAP
using Windows.UI.Xaml.Controls;
#endif

#if IOS
using UIKit;
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class PresentationParameters
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
        private bool isFullScreen;
        private bool hardwareModeSwitch = true;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Create a <see cref="PresentationParameters"/> instance with default values for all properties.
        /// </summary>
        public PresentationParameters()
        {
            Clear();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Get or set the format of the back buffer.
        /// </summary>
        public SurfaceFormat BackBufferFormat
        {
            get { return backBufferFormat; }
            set { backBufferFormat = value; }
        }

        /// <summary>
        /// Get or set the height of the back buffer.
        /// </summary>
        public int BackBufferHeight
        {
            get { return backBufferHeight; }
            set { backBufferHeight = value; }
        }

        /// <summary>
        /// Get or set the width of the back buffer.
        /// </summary>
        public int BackBufferWidth
        {
            get { return backBufferWidth; }
            set { backBufferWidth = value; }
        }

        /// <summary>
        /// Get the bounds of the back buffer.
        /// </summary>
        public Rectangle Bounds 
        {
            get { return new Rectangle(0, 0, backBufferWidth, backBufferHeight); }
        }

        /// <summary>
        /// Get or set the handle of the window that will present the back buffer.
        /// </summary>
        public IntPtr DeviceWindowHandle
        {
            get { return deviceWindowHandle; }
            set { deviceWindowHandle = value; }
        }

#if WINDOWS_UAP
        [CLSCompliant(false)]
        public SwapChainPanel SwapChainPanel { get; set; }
#endif

        /// <summary>
        /// Get or set the depth stencil format for the back buffer.
        /// </summary>
		public DepthFormat DepthStencilFormat
        {
            get { return depthStencilFormat; }
            set { depthStencilFormat = value; }
        }

        /// <summary>
        /// Get or set a value indicating if we are in full screen mode.
        /// </summary>
        public bool IsFullScreen
        {
			get
            {
				 return isFullScreen;
            }
            set
            {
                isFullScreen = value;				
#if IOS && !TVOS
				UIApplication.SharedApplication.StatusBarHidden = isFullScreen;
#endif

			}
        }
		
        /// <summary>
        /// If <code>true</code> the <see cref="GraphicsDevice"/> will do a mode switch
        /// when going to full screen mode. If <code>false</code> it will instead do a
        /// soft full screen by maximizing the window and making it borderless.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return hardwareModeSwitch; }
            set { hardwareModeSwitch = value; }
        }

        /// <summary>
        /// Get or set the multisample count for the back buffer.
        /// </summary>
        public int MultiSampleCount
        {
            get { return multiSampleCount; }
            set { multiSampleCount = value; }
        }
		
        /// <summary>
        /// Get or set the presentation interval.
        /// </summary>
        public PresentInterval PresentationInterval { get; set; }

        /// <summary>
        /// Get or set the display orientation.
        /// </summary>
		public DisplayOrientation DisplayOrientation 
		{ 
			get; 
			set; 
		}
		
        /// <summary>
        /// Get or set the RenderTargetUsage for the back buffer.
        /// Determines if the back buffer is cleared when it is set as the
        /// render target by the <see cref="GraphicsDevice"/>.
        /// <see cref="GraphicsDevice"/> target.
        /// </summary>
		public RenderTargetUsage RenderTargetUsage { get; set; }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Reset all properties to their default values.
        /// </summary>
        public void Clear()
        {
            backBufferFormat = SurfaceFormat.Color;
#if IOS
			// Mainscreen.Bounds does not account for the device's orientation. it ALWAYS assumes portrait
			var width = (int)(UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale);
			var height = (int)(UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale);
			
			// Flip the dimensions if we need to.
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
#if IOS && !TVOS
			isFullScreen = UIApplication.SharedApplication.StatusBarHidden;
#else
            // isFullScreen = false;
#endif
            depthStencilFormat = DepthFormat.None;
            multiSampleCount = 0;
            PresentationInterval = PresentInterval.Default;
            DisplayOrientation = Microsoft.Xna.Framework.DisplayOrientation.Default;
        }

        /// <summary>
        /// Create a copy of this <see cref="PresentationParameters"/> instance.
        /// </summary>
        /// <returns></returns>
        public PresentationParameters Clone()
        {
            PresentationParameters clone = new PresentationParameters();
            clone.backBufferFormat = this.backBufferFormat;
            clone.backBufferHeight = this.backBufferHeight;
            clone.backBufferWidth = this.backBufferWidth;
            clone.deviceWindowHandle = this.deviceWindowHandle;
            clone.depthStencilFormat = this.depthStencilFormat;
            clone.IsFullScreen = this.IsFullScreen;
            clone.HardwareModeSwitch = this.HardwareModeSwitch;
            clone.multiSampleCount = this.multiSampleCount;
            clone.PresentationInterval = this.PresentationInterval;
            clone.DisplayOrientation = this.DisplayOrientation;
            clone.RenderTargetUsage = this.RenderTargetUsage;
            return clone;
        }

        #endregion Methods

    }
}
