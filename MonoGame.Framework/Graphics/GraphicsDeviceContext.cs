using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class GraphicsDeviceContextScope
        : IDisposable
    {
        internal GraphicsDeviceContextScope(GraphicsDeviceContext context)
        {
            Debug.Assert(GraphicsDeviceContext.Current == null);
            GraphicsDeviceContext.Current = context;
        }

        public void Dispose()
        {
            Debug.Assert(GraphicsDeviceContext.Current != null);
            GraphicsDeviceContext.Current = null;
        }
    }

    /// <summary>
    /// This class tightly coupled and manages various Default GraphicsResource for support multiple <see cref="GraphicsDevice">GraphicsDevices</see>
    /// To provide necessary backward-compatibility with XNA or past versions of MonoGame framework.
    /// </summary>
    internal class GraphicsDeviceContext
    {
        [ThreadStatic] private static GraphicsDeviceContext _currentDeviceContext;

        private GraphicsDevice _graphicsDevice;

        public GraphicsDeviceContext(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            SetupBlendStateFactory();
            SetupDepthStencilStateFactory();
            SetupRasterizerStateFactory();
            SetupSamplerStateFactory();
        }

        public static GraphicsDeviceContext Current
        {
            get { return _currentDeviceContext; }
            internal set { _currentDeviceContext = value; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
        }

        public void Reset()
        {
            ResetEffectCache();
            ResetBlendStates();
            ResetDepthStencilStates();
            ResetRasterizerStates();
            ResetSamplerStates();
        }

        #region BlendState

        public ObjectFactoryWithReset<BlendState> Additive
        {
            get { return _additive; }
        }

        public ObjectFactoryWithReset<BlendState> AlphaBlend
        {
            get { return _alphaBlend; }
        }

        public ObjectFactoryWithReset<BlendState> NonPremultiplied
        {
            get { return _nonPremultiplied; }
        }

        public ObjectFactoryWithReset<BlendState> Opaque
        {
            get { return _opaque; }
        }

        private ObjectFactoryWithReset<BlendState> _additive;
        private ObjectFactoryWithReset<BlendState> _alphaBlend;
        private ObjectFactoryWithReset<BlendState> _nonPremultiplied;
        private ObjectFactoryWithReset<BlendState> _opaque;

        private void SetupBlendStateFactory()
        {
            _additive = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState
            {
                Name = "BlendState.Additive",
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One
            });

            _alphaBlend = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState()
            {
                Name = "BlendState.AlphaBlend",
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha
            });

            _nonPremultiplied = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState()
            {
                Name = "BlendState.NonPremultiplied",
                ColorSourceBlend = Blend.SourceAlpha,
                AlphaSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                AlphaDestinationBlend = Blend.InverseSourceAlpha
            });

            _opaque = new Utilities.ObjectFactoryWithReset<BlendState>(() => new BlendState()
            {
                Name = "BlendState.Opaque",
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.Zero
            });
        }

        private void ResetBlendStates()
        {
            _additive.Reset();
            _alphaBlend.Reset();
            _nonPremultiplied.Reset();
            _opaque.Reset();
        }

        #endregion

        #region DepthStencilState

        public ObjectFactoryWithReset<DepthStencilState> Default
        {
            get { return _default; }
        }

        public ObjectFactoryWithReset<DepthStencilState> DepthRead
        {
            get { return _depthRead; }
        }

        public ObjectFactoryWithReset<DepthStencilState> None
        {
            get { return _none; }
        }

        private ObjectFactoryWithReset<DepthStencilState> _default;
        private ObjectFactoryWithReset<DepthStencilState> _depthRead;
        private ObjectFactoryWithReset<DepthStencilState> _none;

        private void SetupDepthStencilStateFactory()
        {
            _default = new ObjectFactoryWithReset<DepthStencilState>(() => new DepthStencilState
            {
                Name = "DepthStencilState.Default",
                DepthBufferEnable = true,
                DepthBufferWriteEnable = true
            });

            _depthRead = new ObjectFactoryWithReset<DepthStencilState>(() => new DepthStencilState
            {
                Name = "DepthStencilState.DepthRead",
                DepthBufferEnable = true,
                DepthBufferWriteEnable = false
            });

            _none = new ObjectFactoryWithReset<DepthStencilState>(() => new DepthStencilState
            {
                Name = "DepthStencilState.None",
                DepthBufferEnable = false,
                DepthBufferWriteEnable = false
            });
        }

        private void ResetDepthStencilStates()
        {
            _default.Reset();
            _depthRead.Reset();
            _none.Reset();
        }

        #endregion

        #region RasterizerState

        public ObjectFactoryWithReset<RasterizerState> CullClockwise
        {
            get { return _cullClockwise; }
        }

        public ObjectFactoryWithReset<RasterizerState> CullCounterClockwise
        {
            get { return _cullCounterClockwise; }
        }

        public ObjectFactoryWithReset<RasterizerState> CullNone
        {
            get { return _cullNone; }
        }

        private ObjectFactoryWithReset<RasterizerState> _cullClockwise;
        private ObjectFactoryWithReset<RasterizerState> _cullCounterClockwise;
        private ObjectFactoryWithReset<RasterizerState> _cullNone;

        private void SetupRasterizerStateFactory()
        {
            _cullClockwise = new ObjectFactoryWithReset<RasterizerState>(() => new RasterizerState
            {
                Name = "RasterizerState.CullClockwise",
                CullMode = CullMode.CullClockwiseFace
            });

            _cullCounterClockwise = new ObjectFactoryWithReset<RasterizerState>(() => new RasterizerState
            {
                Name = "RasterizerState.CullCounterClockwise",
                CullMode = CullMode.CullCounterClockwiseFace
            });

            _cullNone = new ObjectFactoryWithReset<RasterizerState>(() => new RasterizerState
            {
                Name = "RasterizerState.CullNone",
                CullMode = CullMode.None
            });
        }

        private void ResetRasterizerStates()
        {
            _cullClockwise.Reset();
            _cullCounterClockwise.Reset();
            _cullNone.Reset();
        }

        #endregion

        #region SampleState

        public ObjectFactoryWithReset<SamplerState> AnisotropicClamp
        {
            get { return _anisotropicClamp; }
        }

        public ObjectFactoryWithReset<SamplerState> AnisotropicWrap
        {
            get { return _anisotropicWrap; }
        }

        public ObjectFactoryWithReset<SamplerState> LinearClamp
        {
            get { return _linearClamp; }
        }

        public ObjectFactoryWithReset<SamplerState> LinearWrap
        {
            get { return _linearWrap; }
        }

        public ObjectFactoryWithReset<SamplerState> PointClamp
        {
            get { return _pointClamp; }
        }

        public ObjectFactoryWithReset<SamplerState> PointWrap
        {
            get { return _pointWrap; }
        }

        private ObjectFactoryWithReset<SamplerState> _anisotropicClamp;
        private ObjectFactoryWithReset<SamplerState> _anisotropicWrap;
        private ObjectFactoryWithReset<SamplerState> _linearClamp;
        private ObjectFactoryWithReset<SamplerState> _linearWrap;
        private ObjectFactoryWithReset<SamplerState> _pointClamp;
        private ObjectFactoryWithReset<SamplerState> _pointWrap;

        private void SetupSamplerStateFactory()
        {
            _anisotropicClamp = new ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.AnisotropicClamp",
                Filter = TextureFilter.Anisotropic,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            });

            _anisotropicWrap = new ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.AnisotropicWrap",
                Filter = TextureFilter.Anisotropic,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
            });

            _linearClamp = new ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.LinearClamp",
                Filter = TextureFilter.Linear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            });

            _linearWrap = new ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.LinearWrap",
                Filter = TextureFilter.Linear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
            });

            _pointClamp = new ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.PointClamp",
                Filter = TextureFilter.Point,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
            });

            _pointWrap = new ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.PointWrap",
                Filter = TextureFilter.Point,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
            });
        }

        private void ResetSamplerStates()
        {
            _anisotropicClamp.Reset();
            _anisotropicWrap.Reset();
            _linearClamp.Reset();
            _linearWrap.Reset();
            _pointClamp.Reset();
            _pointWrap.Reset();
        }

        #endregion

        #region Effect Cache

        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        private readonly Dictionary<int, GraphicsResource> _effectCache = new Dictionary<int, GraphicsResource>();

        public GraphicsResource GetOrAddEffect(int effectKey, Func<GraphicsResource> valueFactory)
        {
            GraphicsResource returnValue;
            if (_effectCache.TryGetValue(effectKey, out returnValue) == false)
            {
                returnValue = valueFactory();
                _effectCache.Add(effectKey, returnValue);
            }

            return returnValue;
        }

        internal void ResetEffectCache()
        {
            // Dispose all the cached effects.
            foreach (var effect in _effectCache)
                effect.Value.Dispose();

            // Clear the cache.
            _effectCache.Clear();
        }

        #endregion // Effect Cache
    }
}