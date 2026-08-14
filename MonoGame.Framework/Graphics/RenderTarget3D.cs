// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Represents a 3D texture resource that will be written to at the end of a render pass.
	/// </summary>
	/// <remarks>
	///		<para>After a render pass the render target contains the color information of a rendered image.</para>
	///		<para>
	///			Render targets represent a linear area of display memory and usually resides in the display memory of
	///			the display card.  Because of this, objects must be recreated when the device is reset.
	///		</para>
	/// </remarks>
	public partial class RenderTarget3D : Texture3D, IRenderTarget
	{
		/// <summary>
		/// Gets the depth format used by this <b>RenderTarget3D</b>
		/// </summary>
		public DepthFormat DepthStencilFormat { get; private set; }
		
		/// <summary>
		/// Gets the number of samples taken per pixel
		/// </summary>
		public int MultiSampleCount { get; private set; }
		
		/// <summary>
		/// Gets or Sets the usage type used by this <b>RenderTarget3D</b>
		/// </summary>
		public RenderTargetUsage RenderTargetUsage { get; private set; }
		
		/// <summary>
		/// Gets a value that indicates whether the contents of this <b>RenderTarget3D</b> has been lost due to a lost
		/// device event.
		/// </summary>
		/// <remarks>
		/// This property will always return <b>false</b>.  It is included for XNA compatibility.
		/// </remarks>
		[Obsolete("This is provided for XNA compatibility only and will always return false")]
		public bool IsContentLost { get { return false; } }
		
		/// <summary>
		/// Occurs when a graphics device lost event is triggered.
		/// </summary>
		/// <remarks>
		/// This event is never called.  It is included for XNA compatibility.
		/// </remarks>
		[Obsolete("This is provided for XNA compatibility is never called by MonoGame")]
		public event EventHandler<EventArgs> ContentLost;

		private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
		{
			return ContentLost != null;
		}

		/// <summary>
		/// Creates a new <b>RenderTarget3D</b> instance with the specified parameters.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device to associate with this render target resource.</param>
		/// <param name="width">Width, in pixels, of the render target.</param>
		/// <param name="height">Height, in pixels, of the render target.</param>
		/// <param name="depth">Depth, in pixels, of the render target.</param>
		/// <param name="mipMap"><b>true</b> if mipmapping is enabled; otherwise, <b>false</b>.</param>
		/// <param name="preferredFormat">The preferred surface format of the render target.</param>
		/// <param name="preferredDepthFormat">The preferred depth format of the render target.</param>
		/// <param name="preferredMultiSampleCount">The preferred number of samples per pixel when multisampling.</param>
		/// <param name="usage">The behavior to use when binding the render target to the graphics device.</param>
		public RenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
			:base (graphicsDevice, width, height, depth, mipMap, QuerySelectedFormat(graphicsDevice, preferredFormat), true)
		{
			DepthStencilFormat = preferredDepthFormat;
			MultiSampleCount = preferredMultiSampleCount;
			RenderTargetUsage = usage;

			// If we don't need a depth buffer then we're done.
			if (preferredDepthFormat == DepthFormat.None)
				return;

			PlatformConstruct(graphicsDevice, width, height, mipMap, preferredDepthFormat, preferredMultiSampleCount, usage);
		}

		/// <summary />
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

		/// <summary>
		/// Creates a new <b>RenderTarget3D</b> instance with the specified parameters.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device to associate with this render target resource.</param>
		/// <param name="width">Width, in pixels, of the render target.</param>
		/// <param name="height">Height, in pixels, of the render target.</param>
		/// <param name="depth">Depth, in pixels, of the render target.</param>
		/// <param name="mipMap"><b>true</b> if mipmapping is enabled; otherwise, <b>false</b>.</param>
		/// <param name="preferredFormat">The preferred surface format of the render target.</param>
		/// <param name="preferredDepthFormat">The preferred depth format of the render target.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="graphicsDevice"/> parameter is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The <paramref name="width"/> and/or <paramref name="height"/> and/or <paramref name="depth"/> parameters less than or equal to zero.
		/// </exception>
		public RenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
			:this (graphicsDevice, width, height, depth, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents) 
		{}
		
		/// <summary>
		/// Creates a new <b>RenderTarget3D</b> instance with the specified parameters.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device to associate with this render target resource.</param>
		/// <param name="width">Width, in pixels, of the render target.</param>
		/// <param name="height">Height, in pixels, of the render target.</param>
		/// <param name="depth">Depth, in pixels, of the render target.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="graphicsDevice"/> parameter is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The <paramref name="width"/> and/or <paramref name="height"/> and/or <paramref name="depth"/> parameters less than or equal to zero.
		/// </exception>
		public RenderTarget3D(GraphicsDevice graphicsDevice, int width, int height, int depth)
			: this(graphicsDevice, width, height, depth, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents) 
		{}
	}
}
