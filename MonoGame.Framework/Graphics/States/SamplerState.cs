// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Contains sampler state, which determines how to sample texture data.
    /// </summary>
    public partial class SamplerState : GraphicsResource
    {
        static SamplerState()
        {
            AnisotropicClamp = new SamplerState("SamplerState.AnisotropicClamp", TextureFilter.Anisotropic, TextureAddressMode.Clamp);
            AnisotropicWrap = new SamplerState("SamplerState.AnisotropicWrap", TextureFilter.Anisotropic, TextureAddressMode.Wrap);
            LinearClamp = new SamplerState("SamplerState.LinearClamp", TextureFilter.Linear, TextureAddressMode.Clamp);
            LinearWrap = new SamplerState("SamplerState.LinearWrap", TextureFilter.Linear, TextureAddressMode.Wrap);
            PointClamp = new SamplerState("SamplerState.PointClamp", TextureFilter.Point, TextureAddressMode.Clamp);
            PointWrap = new SamplerState("SamplerState.PointWrap", TextureFilter.Point, TextureAddressMode.Wrap);
        }

        /// <summary>
        /// Contains default state for anisotropic filtering and texture coordinate clamping.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        /// <code>
        /// Filter = TextureFilter.Anisotropic,
        /// AddressU = TextureAddressMode.Clamp,
        /// AddressV = TextureAddressMode.Clamp,
        /// AddressW = TextureAddressMode.Clamp,
        /// </code>
        /// </remarks>
        public static readonly SamplerState AnisotropicClamp;
        /// <summary>
        /// Contains default state for anisotropic filtering and texture coordinate wrapping.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        /// <code>
        /// Filter = TextureFilter.Anisotropic,
        /// AddressU = TextureAddressMode.Wrap,
        /// AddressV = TextureAddressMode.Wrap,
        /// AddressW = TextureAddressMode.Wrap,
        /// </code>
        /// </remarks>
        public static readonly SamplerState AnisotropicWrap;
        /// <summary>
        /// Contains default state for linear filtering and texture coordinate clamping.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        /// <code>
        /// Filter = TextureFilter.Linear,
        /// AddressU = TextureAddressMode.Clamp,
        /// AddressV = TextureAddressMode.Clamp,
        /// AddressW = TextureAddressMode.Clamp,
        /// </code>
        /// </remarks>
        public static readonly SamplerState LinearClamp;
        /// <summary>
        /// Contains default state for linear filtering and texture coordinate wrapping.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        /// <code>
        /// Filter = TextureFilter.Linear,
        /// AddressU = TextureAddressMode.Wrap,
        /// AddressV = TextureAddressMode.Wrap,
        /// AddressW = TextureAddressMode.Wrap,
        /// </code>
        /// </remarks>
        public static readonly SamplerState LinearWrap;
        /// <summary>
        /// Contains default state for point filtering and texture coordinate clamping.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        /// <code>
        /// Filter = TextureFilter.Point,
        /// AddressU = TextureAddressMode.Clamp,
        /// AddressV = TextureAddressMode.Clamp,
        /// AddressW = TextureAddressMode.Clamp,
        /// </code>
        /// </remarks>
        public static readonly SamplerState PointClamp;
        /// <summary>
        /// Contains default state for point filtering and texture coordinate wrapping.
        /// </summary>
        /// <remarks>
        /// This built-in state object has the following settings:
        /// <code>
        /// Filter = TextureFilter.Point,
        /// AddressU = TextureAddressMode.Wrap,
        /// AddressV = TextureAddressMode.Wrap,
        /// AddressW = TextureAddressMode.Wrap,
        /// </code>
        /// </remarks>
        public static readonly SamplerState PointWrap;

        private readonly bool _defaultStateObject;

        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;
        private Color _borderColor;
        private TextureFilter _filter;
        private int _maxAnisotropy;
        private int _maxMipLevel;
        private float _mipMapLevelOfDetailBias;
        private TextureFilterMode _filterMode;
        private CompareFunction _comparisonFunction;

        /// <summary>
        /// Gets or sets the texture-address mode for the u-coordinate.
        /// </summary>
        public TextureAddressMode AddressU
        {
            get { return _addressU; }
            set
            {
                ThrowIfBound();
                _addressU = value;
            }
        }

        /// <summary>
        /// Gets or sets the texture-address mode for the v-coordinate.
        /// </summary>
        public TextureAddressMode AddressV
        {
            get { return _addressV; }
            set
            {
                ThrowIfBound();
                _addressV = value;
            }
        }

        /// <summary>
        /// Gets or sets the texture-address mode for the w-coordinate.
        /// </summary>
        public TextureAddressMode AddressW
        {
            get { return _addressW; }
            set
            {
                ThrowIfBound();
                _addressW = value;
            }
        }

        /// <summary>
        /// Gets or sets the color to use for texels outside of range,
        /// when <see cref="TextureAddressMode.Border"/> is used.
        /// </summary>
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                ThrowIfBound();
                _borderColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of filtering during sampling.
        /// </summary>
        public TextureFilter Filter
        {
            get { return _filter; }
            set
            {
                ThrowIfBound();
                _filter = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum anisotropy.
        /// The default value is 4.
        /// </summary>
        /// <remarks>
        /// Use anisotropic filtering to reduce blur and aliasing effects
        /// when texturing a surface that will be viewed at an extreme viewing angle.
        /// </remarks>
        public int MaxAnisotropy
        {
            get { return _maxAnisotropy; }
            set
            {
                ThrowIfBound();
                _maxAnisotropy = value;
            }
        }

        /// <summary>
        /// Gets or sets the level of detail (LOD) index of the largest map to use.
        /// </summary>
        /// <value>
        /// The maximum LOD, which ranges from 0 to n-1, where n is the index of the largest map.
        /// </value>
        public int MaxMipLevel
        {
            get { return _maxMipLevel; }
            set
            {
                ThrowIfBound();
                _maxMipLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the mipmap LOD bias. The default value is 0. <para/>
        /// A negative value indicates a larger mipmap level; a positive value indicates a smaller mipmap level.
        /// </summary>
        /// <remarks>
        /// Mipmap LOD bias offsets the mipmap level from which a texture is sampled
        /// (the result is computed using trilinear texturing between the nearest two levels).
        /// </remarks>
        public float MipMapLevelOfDetailBias
        {
            get { return _mipMapLevelOfDetailBias; }
            set
            {
                ThrowIfBound();
                _mipMapLevelOfDetailBias = value;
            }
        }

        /// <summary>
        /// When using comparison sampling, also set <see cref="FilterMode"/> to <see cref="TextureFilterMode.Comparison"/>.
        /// </summary>
        public CompareFunction ComparisonFunction
        {
            get { return _comparisonFunction; }
            set
            {
                ThrowIfBound();
                _comparisonFunction = value;
            }
        }

        /// <summary>
        /// Gets or sets filtering mode for texture samplers.
        /// </summary>
        public TextureFilterMode FilterMode
        {
            get { return _filterMode; }
            set
            {
                ThrowIfBound();
                _filterMode = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");
            if (GraphicsDevice != null && GraphicsDevice != device)
                throw new InvalidOperationException("This sampler state is already bound to a different graphics device.");
            GraphicsDevice = device;
        }

        internal void ThrowIfBound()
        {
            if (_defaultStateObject)
                throw new InvalidOperationException("You cannot modify a default sampler state object.");
            if (GraphicsDevice != null)
                throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SamplerState"/> class
        /// with default values equivalent to <see cref="LinearWrap"/>.
        /// </summary>
        public SamplerState()
        {
            Filter = TextureFilter.Linear;
            AddressU = TextureAddressMode.Wrap;
            AddressV = TextureAddressMode.Wrap;
            AddressW = TextureAddressMode.Wrap;
            BorderColor = Color.White;
            MaxAnisotropy = 4;
            MaxMipLevel = 0;
            MipMapLevelOfDetailBias = 0.0f;
            ComparisonFunction = CompareFunction.Never;
            FilterMode = TextureFilterMode.Default;
        }

        private SamplerState(string name, TextureFilter filter, TextureAddressMode addressMode)
            : this()
        {
            Name = name;
            _filter = filter;
            _addressU = addressMode;
            _addressV = addressMode;
            _addressW = addressMode;
            _defaultStateObject = true;
        }

        private SamplerState(SamplerState cloneSource)
        {
            Name = cloneSource.Name;
            _filter = cloneSource._filter;
            _addressU = cloneSource._addressU;
            _addressV = cloneSource._addressV;
            _addressW = cloneSource._addressW;
            _borderColor = cloneSource._borderColor;
            _maxAnisotropy = cloneSource._maxAnisotropy;
            _maxMipLevel = cloneSource._maxMipLevel;
            _mipMapLevelOfDetailBias = cloneSource._mipMapLevelOfDetailBias;
            _comparisonFunction = cloneSource._comparisonFunction;
            _filterMode = cloneSource._filterMode;
        }

        internal SamplerState Clone()
        {
            return new SamplerState(this);
        }

        partial void PlatformDispose();

        /// <summary/>
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
