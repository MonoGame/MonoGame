// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class RenderTarget2D : Texture2D, IRenderTarget
	{
		public DepthFormat DepthStencilFormat { get; private set; }
		
		public int MultiSampleCount { get; private set; }
		
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		public bool IsContentLost { get { return false; } }
		
		public event EventHandler<EventArgs> ContentLost;
		
        private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
        {
            return ContentLost != null;
        }

	    public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared, int arraySize)
	        : this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, SurfaceType.RenderTarget, shared, arraySize)
	    {
	    }

        public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
			: this(graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, shared, 1)
        {
			
        }

		public RenderTarget2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, preferredMultiSampleCount, usage, false)
        {}

		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this (graphicsDevice, width, height, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
		{}
		
		public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
			: this(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
		{}

        internal RenderTarget2D(GraphicsDevice graphicsDevice,
                        int width,
                        int height,
                        bool mipMap,
                        SurfaceFormat format,
                        DepthFormat depthFormat,
                        int preferredMultiSampleCount,
                        RenderTargetUsage usage,
                        SurfaceType surfaceType)
            : this(graphicsDevice, width, height, mipMap, format, depthFormat, preferredMultiSampleCount, usage, surfaceType, false, 1)
        {
        }

        /// <summary>
        /// Allows child class to specify the surface type, eg: a swap chain.
        /// </summary>        
        internal RenderTarget2D(GraphicsDevice graphicsDevice,
                        int width,
                        int height,
                        bool mipMap,
                        SurfaceFormat preferredFormat,
                        DepthFormat depthFormat,
                        int preferredMultiSampleCount,
                        RenderTargetUsage usage,
                        SurfaceType surfaceType,
                        bool shared,
                        int arraySize)
            : base(graphicsDevice, width, height, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), surfaceType, shared, arraySize)
        {
            DepthStencilFormat = depthFormat;
            RenderTargetUsage = usage;
            MultiSampleCount = preferredMultiSampleCount;

            PlatformConstruct(graphicsDevice, width, height, mipMap, depthFormat, preferredMultiSampleCount, usage, shared);
        }

        protected static SurfaceFormat QuerySelectedFormat(GraphicsDevice graphicsDevice, SurfaceFormat preferredFormat)
        {
			SurfaceFormat selectedFormat = preferredFormat;
			DepthFormat selectedDepthFormat;
			int selectedMultiSampleCount;

            if (graphicsDevice != null)
            {
                graphicsDevice.Adapter.QueryRenderTargetFormat(graphicsDevice.GraphicsProfile, preferredFormat, DepthFormat.None, 0,
                    out selectedFormat, out selectedDepthFormat, out selectedMultiSampleCount);
            }

            return selectedFormat;
        }

        protected internal override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
            base.GraphicsDeviceResetting();
        }        
	}
}
