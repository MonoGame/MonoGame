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

using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework.Graphics
{
    public class PresentationParameters : IDisposable
    {
        #region Constants

        public const int DefaultPresentRate = 60;

        #endregion Constants

        #region Private Fields

        private DepthFormat autoDepthStencilFormat;
		private DepthFormat depthStencilFormat; // Added for XNA 4.0
        private int backBufferCount;
        private SurfaceFormat backBufferFormat;
        private int backBufferHeight;
        private int backBufferWidth;
        private IntPtr deviceWindowHandle;
        private bool enableAutoDepthStencil;
        private int fullScreenRefreshRateInHz;
        private bool isFullScreen;
		private int multiSampleCount; // Added for XNA 4.0
        private int multiSampleQuality;
        private MultiSampleType multiSampleType;
        private SwapEffect swapEffect;
        private bool disposed;

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

        public DepthFormat AutoDepthStencilFormat
        {
            get { return autoDepthStencilFormat; }
            set { autoDepthStencilFormat = value; }
        }

        public int BackBufferCount
        {
            get { return backBufferCount; }
            set { backBufferCount = value; }
        }

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

        public IntPtr DeviceWindowHandle
        {
            get { return deviceWindowHandle; }
            set { deviceWindowHandle = value; }
        }
		
		public DepthFormat DepthStencilFormat
        {
            get { return depthStencilFormat; }
            set { depthStencilFormat = value; }
        }

        public bool EnableAutoDepthStencil
        {
            get { return enableAutoDepthStencil; }
            set { enableAutoDepthStencil = value; }
        }

        public int FullScreenRefreshRateInHz
        {
            get { return fullScreenRefreshRateInHz; }
            set { fullScreenRefreshRateInHz = value; }
        }

        public bool IsFullScreen
        {
			get
            {
				 return isFullScreen;
            }
            set
            {
				if ( isFullScreen != value )
				{
					isFullScreen = value;
					UIApplication.SharedApplication.StatusBarHidden = isFullScreen;
				}
            }
        }
		
		public int MultiSampleCount
        {
            get { return multiSampleCount; }
            set { multiSampleCount = value; }
        }

        public int MultiSampleQuality
        {
            get { return multiSampleQuality; }
            set { multiSampleQuality = value; }
        }

        public MultiSampleType MultiSampleType
        {
            get { return multiSampleType; }
            set { multiSampleType = value; }
        }

        public SwapEffect SwapEffect
        {
            get { return swapEffect; }
            set { swapEffect = value; }
        }
		
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
            autoDepthStencilFormat = DepthFormat.None;
            backBufferCount = 0;
            backBufferFormat = SurfaceFormat.Color;
			
			backBufferWidth = (int)(UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale);
            backBufferHeight = (int)(UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale);
			           
            deviceWindowHandle = IntPtr.Zero;
            enableAutoDepthStencil = false;
            fullScreenRefreshRateInHz = 0;
            isFullScreen = UIApplication.SharedApplication.StatusBarHidden;
			depthStencilFormat = DepthFormat.None;
            multiSampleCount = 0;
            multiSampleQuality = 0;
            multiSampleType = MultiSampleType.None;
            swapEffect = SwapEffect.Default;
			this.DisplayOrientation = DisplayOrientation.Default;
        }

        public PresentationParameters Clone()
        {
            PresentationParameters clone = new PresentationParameters();
            clone.autoDepthStencilFormat = this.autoDepthStencilFormat;
            clone.backBufferCount = this.backBufferCount;
            clone.backBufferFormat = this.backBufferFormat;
            clone.backBufferHeight = this.backBufferHeight;
            clone.backBufferWidth = this.backBufferWidth;
            clone.deviceWindowHandle = this.deviceWindowHandle;
            clone.disposed = this.disposed;
            clone.enableAutoDepthStencil = this.enableAutoDepthStencil;
            clone.fullScreenRefreshRateInHz = this.fullScreenRefreshRateInHz;
            clone.IsFullScreen = this.isFullScreen;
            clone.multiSampleQuality = this.multiSampleQuality;
            clone.multiSampleType = this.multiSampleType;
            clone.swapEffect = this.swapEffect;
			clone.depthStencilFormat = this.depthStencilFormat;
            clone.multiSampleCount = this.multiSampleCount;
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
