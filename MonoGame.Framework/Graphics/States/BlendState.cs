// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class BlendState : GraphicsResource
	{
        private readonly TargetBlendState[] _targetBlendState;

	    private Color _blendFactor;

	    private int _multiSampleMask;

	    private bool _independentBlendEnable;

        [Conditional("DEBUG")]
        private void AssertIfBound()
        {
            Debug.Assert(GraphicsDevice == null, "You cannot modify the blend state after it has been bound to the graphics device!");
        }

        /// <summary>
        /// Returns the target specific blend state.
        /// </summary>
        /// <param name="index">The 0 to 3 target blend state index.</param>
        /// <returns>A target blend state.</returns>
        public TargetBlendState this[int index]
        {
            get { return _targetBlendState[index]; }
        }

	    public BlendFunction AlphaBlendFunction
	    {
	        get { return _targetBlendState[0].AlphaBlendFunction; } 
            set
            {
                AssertIfBound();
                _targetBlendState[0].AlphaBlendFunction = value;
            }
	    }

		public Blend AlphaDestinationBlend
        {
            get { return _targetBlendState[0].AlphaDestinationBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].AlphaDestinationBlend = value;
            }
        }

		public Blend AlphaSourceBlend
        {
            get { return _targetBlendState[0].AlphaSourceBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].AlphaSourceBlend = value;
            }
        }

		public BlendFunction ColorBlendFunction
        {
            get { return _targetBlendState[0].ColorBlendFunction; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorBlendFunction = value;
            }
        }

		public Blend ColorDestinationBlend
        {
            get { return _targetBlendState[0].ColorDestinationBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorDestinationBlend = value;
            }
        }

		public Blend ColorSourceBlend
        {
            get { return _targetBlendState[0].ColorSourceBlend; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorSourceBlend = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels
        {
            get { return _targetBlendState[0].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[0].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels1
        {
            get { return _targetBlendState[1].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[1].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels2
        {
            get { return _targetBlendState[2].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[2].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels3
        {
            get { return _targetBlendState[3].ColorWriteChannels; }
            set
            {
                AssertIfBound();
                _targetBlendState[3].ColorWriteChannels = value;
            }
        }

	    public Color BlendFactor
	    {
	        get { return _blendFactor; }
            set
            {
                AssertIfBound();
                _blendFactor = value;
            }
	    }

        public int MultiSampleMask
        {
            get { return _multiSampleMask; }
            set
            {
                AssertIfBound();
                _multiSampleMask = value;
            }
        }

        /// <summary>
        /// Enables use of the per-target blend states.
        /// </summary>
        public bool IndependentBlendEnable
        {
            get { return _independentBlendEnable; }
            set
            {
                AssertIfBound();
                _independentBlendEnable = value;
            }
        }

        public static BlendState Additive
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.Additive);
                return GraphicsDeviceContext.Current.Additive.Value;
            }
        }

        public static BlendState AlphaBlend
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.AlphaBlend);
                return GraphicsDeviceContext.Current.AlphaBlend.Value;
            }
        }

        public static BlendState NonPremultiplied
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.NonPremultiplied);
                return GraphicsDeviceContext.Current.NonPremultiplied.Value; 
            }
        }

        public static BlendState Opaque
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.Opaque);
                return GraphicsDeviceContext.Current.Opaque.Value; 
            }
        }

        public BlendState() 
        {
            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = new TargetBlendState();
            _targetBlendState[1] = new TargetBlendState();
            _targetBlendState[2] = new TargetBlendState();
            _targetBlendState[3] = new TargetBlendState();

			_blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;
            _independentBlendEnable = false;
        }
	}
}

