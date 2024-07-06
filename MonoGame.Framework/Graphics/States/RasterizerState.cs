// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Contains rasterizer state, which determines how to convert vector data (shapes) into raster data (pixels).
    /// </summary>
	public partial class RasterizerState : GraphicsResource
	{
        private readonly bool _defaultStateObject;

        private CullMode _cullMode;
        private float _depthBias;
        private FillMode _fillMode;
        private bool _multiSampleAntiAlias;
        private bool _scissorTestEnable;
        private float _slopeScaleDepthBias;
        private bool _depthClipEnable;

        /// <summary>
        /// Gets or Sets the conditions for culling or removing triangles.
        /// The default value is <see cref="CullMode.CullCounterClockwiseFace"/>
        /// </summary>
        public CullMode CullMode
	    {
	        get { return _cullMode; }
            set
            {
                ThrowIfBound();
                _cullMode = value;
            }
	    }

        /// <summary>
        /// Gets or Sets the depth bias for polygons, which is the amount of bias to apply to the depth of a primitive
        /// to alleviate depth testing problems for primitives of similar depth.
        /// The default value is <c>0.0f</c>.
        /// </summary>
        /// <remarks>
        /// A polygon with a larger z-bias value appears in front of a polygon with a smaller value.  For example, a
        /// a polygon with a value of <c>1.0f</c> appears drawn in front of a polygon with a value of <c>0.0f</c>
        /// </remarks>
	    public float DepthBias
	    {
	        get { return _depthBias; }
	        set
	        {
                ThrowIfBound();
                _depthBias = value;
	        }
	    }

        /// <summary>
        /// Gets or Sets the fille mode, which defines how a triangle is filled during rendering.
        /// The default is <see cref="FillMode.Solid"/>.
        /// </summary>
	    public FillMode FillMode
	    {
	        get { return _fillMode; }
	        set
	        {
                ThrowIfBound();
                _fillMode = value;
	        }
	    }

        /// <summary>
        /// Gets or Sets a value that indicates whether multisample antialiasing is enabled.
        /// </summary>
        /// <remarks>
        /// When multisample antialiasing is enabled, full-scene antialiasing is performed by calculating sampling
        /// locations at different sample positions for each multiple sample.  When disabled, each multiple sample is
        /// written with the same sample value (sampled at a pixel center) which allows non-antialiased rendering to a
        /// mutlisample buffer.  This property has no effect when rendering to a buffer that does not support multisampling.
        /// </remarks>
	    public bool MultiSampleAntiAlias
	    {
	        get { return _multiSampleAntiAlias; }
	        set
	        {
                ThrowIfBound();
                _multiSampleAntiAlias = value;
	        }
	    }

        /// <summary>
        /// Gets or Sets a value that indicates whether scissor testing is enabled.
        /// </summary>
        /// <remarks>
        /// Scissor testing can improve drawing performance by only drawing triangles (or parts of triangles) that are
        /// contained within a <see cref="GraphicsDevice.ScissorRectangle">GraphicsDevice.ScissorRectangle</see>
        /// </remarks>
	    public bool ScissorTestEnable
	    {
	        get { return _scissorTestEnable; }
	        set
	        {
                ThrowIfBound();
                _scissorTestEnable = value;
	        }
	    }

        /// <summary>
        /// Gets or Sets a bias value that takes into account the slope of a polygon.  This bias value is applied to
        /// coplanar primitives to reduce aliasing and other rendering artifacts caused by z-fighting.
        /// </summary>
        /// <remarks>
        /// An application can help ensure that coplanar polygons are rendered property by adding a bias to the z-values
        /// that the system uses when rendering sets of coplanar polygons.  The following formula shows how to calculate
        /// the bias to be applied to coplanar primitives
        ///
        /// <code>bias = (m x SlopeScaleDepthBias) + DepthBias</code>
        ///
        /// Where <i>m</i> is the maximum depth slope of the triangle being rendered, defined as
        ///
        /// <code>m = max( abs(delta z / delta x), abs(delta z / delta y) )</code>
        /// </remarks>
	    public float SlopeScaleDepthBias
	    {
	        get { return _slopeScaleDepthBias; }
	        set
	        {
                ThrowIfBound();
                _slopeScaleDepthBias = value;
	        }
	    }

        /// <summary>
        /// Gets or Sets a value that indicates whether depth clipping is enabled.
        /// </summary>
        public bool DepthClipEnable
        {
            get { return _depthClipEnable; }
            set
            {
                ThrowIfBound();
                _depthClipEnable = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");
            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This rasterizer state is already bound to a different graphics device.");
            GraphicsDevice = device;
        }

        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default rasterizer state object.");
            if (GraphicsDevice != null)
                throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");
        }

        /// <summary>
        /// A built-in state object with settings for culling primitives with clockwise winding order.
        /// </summary>
        /// <remarks>
        /// This build-in state object has the following settings
        ///
        /// <code>
        /// CullMode = CullMode.CullClockwiseFace
        /// </code>
        /// </remarks>
	    public static readonly RasterizerState CullClockwise;

        /// <summary>
        /// A built-in state object with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        /// <remarks>
        /// This build-in state object has the following settings
        ///
        /// <code>
        /// CullMode = CullMode.CullCounterClockwiseFace
        /// </code>
        /// </remarks>
        public static readonly RasterizerState CullCounterClockwise;

        /// <summary>
        /// A built-in state object with settings for not culling any primitives.
        /// </summary>
        /// <remarks>
        /// This build-in state object has the following settings
        ///
        /// <code>
        /// CullMode = CullMode.None
        /// </code>
        /// </remarks>
        public static readonly RasterizerState CullNone;

        /// <summary>
        /// Creates a new instance of <b>RasterizerState</b>.
        /// </summary>
        public RasterizerState()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
            DepthClipEnable = true;
		}

	    private RasterizerState(string name, CullMode cullMode)
            : this()
	    {
	        Name = name;
	        _cullMode = cullMode;
	        _defaultStateObject = true;
	    }

	    private RasterizerState(RasterizerState cloneSource)
	    {
	        Name = cloneSource.Name;
	        _cullMode = cloneSource._cullMode;
	        _fillMode = cloneSource._fillMode;
	        _depthBias = cloneSource._depthBias;
	        _multiSampleAntiAlias = cloneSource._multiSampleAntiAlias;
	        _scissorTestEnable = cloneSource._scissorTestEnable;
	        _slopeScaleDepthBias = cloneSource._slopeScaleDepthBias;
	        _depthClipEnable = cloneSource._depthClipEnable;
	    }

		static RasterizerState ()
		{
		    CullClockwise = new RasterizerState("RasterizerState.CullClockwise", CullMode.CullClockwiseFace);
		    CullCounterClockwise = new RasterizerState("RasterizerState.CullCounterClockwise", CullMode.CullCounterClockwiseFace);
		    CullNone = new RasterizerState("RasterizerState.CullNone", CullMode.None);
		}

	    internal RasterizerState Clone()
	    {
	        return new RasterizerState(this);
	    }

        partial void PlatformDispose();

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                PlatformDispose();
            }
            base.Dispose(disposing);
        }
    }
}
