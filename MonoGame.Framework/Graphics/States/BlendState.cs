// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class BlendState : GraphicsResource
	{
        private readonly TargetBlendState[] _targetBlendState;

        private readonly bool _defaultStateObject;

	    private Color _blendFactor;

	    private int _multiSampleMask;

	    private bool _independentBlendEnable;

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");
            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This blend state is already bound to a different graphics device.");
            GraphicsDevice = device;
        }

        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default blend state object.");
            if (GraphicsDevice != null)
                throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");
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
                ThrowIfBound();
                _targetBlendState[0].AlphaBlendFunction = value;
            }
	    }

		public Blend AlphaDestinationBlend
        {
            get { return _targetBlendState[0].AlphaDestinationBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaDestinationBlend = value;
            }
        }

		public Blend AlphaSourceBlend
        {
            get { return _targetBlendState[0].AlphaSourceBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaSourceBlend = value;
            }
        }

		public BlendFunction ColorBlendFunction
        {
            get { return _targetBlendState[0].ColorBlendFunction; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorBlendFunction = value;
            }
        }

		public Blend ColorDestinationBlend
        {
            get { return _targetBlendState[0].ColorDestinationBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorDestinationBlend = value;
            }
        }

		public Blend ColorSourceBlend
        {
            get { return _targetBlendState[0].ColorSourceBlend; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorSourceBlend = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels
        {
            get { return _targetBlendState[0].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels1
        {
            get { return _targetBlendState[1].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[1].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels2
        {
            get { return _targetBlendState[2].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[2].ColorWriteChannels = value;
            }
        }

		public ColorWriteChannels ColorWriteChannels3
        {
            get { return _targetBlendState[3].ColorWriteChannels; }
            set
            {
                ThrowIfBound();
                _targetBlendState[3].ColorWriteChannels = value;
            }
        }

        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor"/> is set to this value when this <see cref="BlendState"/>
        /// is bound to a GraphicsDevice.
        /// </remarks>
	    public Color BlendFactor
	    {
	        get { return _blendFactor; }
            set
            {
                ThrowIfBound();
                _blendFactor = value;
            }
	    }

        public int MultiSampleMask
        {
            get { return _multiSampleMask; }
            set
            {
                ThrowIfBound();
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
                ThrowIfBound();
                _independentBlendEnable = value;
            }
        }


        public static readonly BlendState Additive;
        public static readonly BlendState AlphaBlend;
        public static readonly BlendState NonPremultiplied;
        public static readonly BlendState Opaque;

        public BlendState()
        {
            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = new TargetBlendState(this);
            _targetBlendState[1] = new TargetBlendState(this);
            _targetBlendState[2] = new TargetBlendState(this);
            _targetBlendState[3] = new TargetBlendState(this);

			_blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;
            _independentBlendEnable = false;
        }

        private BlendState(string name, Blend sourceBlend, Blend destinationBlend)
            : this()
        {
            Name = name;
            ColorSourceBlend = sourceBlend;
            AlphaSourceBlend = sourceBlend;
            ColorDestinationBlend = destinationBlend;
            AlphaDestinationBlend = destinationBlend;
            _defaultStateObject = true;
        }

        private BlendState(BlendState cloneSource)
        {
            Name = cloneSource.Name;

            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = cloneSource[0].Clone(this);
            _targetBlendState[1] = cloneSource[1].Clone(this);
            _targetBlendState[2] = cloneSource[2].Clone(this);
            _targetBlendState[3] = cloneSource[3].Clone(this);

            _blendFactor = cloneSource._blendFactor;
            _multiSampleMask = cloneSource._multiSampleMask;
            _independentBlendEnable = cloneSource._independentBlendEnable;
        }

        static BlendState()
        {
            Additive = new BlendState("BlendState.Additive", Blend.SourceAlpha, Blend.One);
            AlphaBlend = new BlendState("BlendState.AlphaBlend", Blend.One, Blend.InverseSourceAlpha);
            NonPremultiplied = new BlendState("BlendState.NonPremultiplied", Blend.SourceAlpha, Blend.InverseSourceAlpha);
            Opaque = new BlendState("BlendState.Opaque", Blend.One, Blend.Zero);
		}

	    internal BlendState Clone()
	    {
	        return new BlendState(this);
	    }

        partial void PlatformDispose();

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                for (int i = 0; i < _targetBlendState.Length; ++i)
                    _targetBlendState[i] = null;

                PlatformDispose();
            }
            base.Dispose(disposing);
        }
    }
}

