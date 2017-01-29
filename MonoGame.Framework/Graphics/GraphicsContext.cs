// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsContext : IDisposable
    {
        private bool _disposed = false;

        private GraphicsDevice _device;
        
        private Color _blendFactor = Color.White;
        private bool _blendFactorDirty;
        
        private BlendState _blendState;    
        private BlendState _actualBlendState;
        internal bool _blendStateDirty;
        
        private BlendState _blendStateAdditive;
        private BlendState _blendStateAlphaBlend;
        private BlendState _blendStateNonPremultiplied;
        private BlendState _blendStateOpaque;

        private void Initialize(GraphicsDevice device)
        {
            _device = device;
                        
            _blendStateAdditive = BlendState.Additive.Clone();
            _blendStateAlphaBlend = BlendState.AlphaBlend.Clone();
            _blendStateNonPremultiplied = BlendState.NonPremultiplied.Clone();
            _blendStateOpaque = BlendState.Opaque.Clone();
        }
        
        ~GraphicsContext()
        {
            Dispose(false);
        }

        internal void SetDefaultRenderStates()
        {
            _blendStateDirty = true;
            BlendState = BlendState.Opaque;
        }
        
        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// When only changing BlendFactor, use this rather than <see cref="Graphics.BlendState.BlendFactor"/> to
        /// only update BlendFactor so the whole BlendState does not have to be updated.
        /// </remarks>
        public Color BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                if (_blendFactor == value)
                    return;
                _blendFactor = value;
                _blendFactorDirty = true;
            }
        }
        
        public BlendState BlendState
        {
			get { return _blendState; }
			set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                // Don't set the same state twice!
                if (_blendState == value)
                    return;

				_blendState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newBlendState = _blendState;
                if (ReferenceEquals(_blendState, BlendState.Additive))
                    newBlendState = _blendStateAdditive;
                else if (ReferenceEquals(_blendState, BlendState.AlphaBlend))
                    newBlendState = _blendStateAlphaBlend;
                else if (ReferenceEquals(_blendState, BlendState.NonPremultiplied))
                    newBlendState = _blendStateNonPremultiplied;
                else if (ReferenceEquals(_blendState, BlendState.Opaque))
                    newBlendState = _blendStateOpaque;

                // Blend state is now bound to a device... no one should
                // be changing the state of the blend state object now!
                newBlendState.BindToGraphicsDevice(_device);

                _actualBlendState = newBlendState;

                BlendFactor = _actualBlendState.BlendFactor;

                _blendStateDirty = true;
            }
		}

        internal void ApplyBlend(bool force = false)
        {
            if (force || _blendFactorDirty || _blendStateDirty)
            {
                PlatformApplyBlend(force);
                _blendFactorDirty = false;
                _blendStateDirty = false;
            }
        }

        #region Implement IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                PlatformDispose(disposing);
                _device = null;
                _disposed = true;
            }
        }

        #endregion
    }    
}
