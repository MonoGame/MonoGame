// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsContext : IDisposable
    {
        internal DeviceContext _d3dContext;

        public GraphicsContext(GraphicsDevice device, DeviceContext deviceContext)
        {
            Initialize(device);
            this._d3dContext = deviceContext;
        }
        
        internal void PlatformApplyBlend(bool force)
        {   
            var state = _actualBlendState.GetDxState(_device);
            var factor = GetBlendFactor();
            _d3dContext.OutputMerger.SetBlendState(state, factor);
        }



#if WINDOWS_UAP
        private SharpDX.Mathematics.Interop.RawColor4 GetBlendFactor()
        {
			return new SharpDX.Mathematics.Interop.RawColor4(
					BlendFactor.R / 255.0f,
					BlendFactor.G / 255.0f,
					BlendFactor.B / 255.0f,
					BlendFactor.A / 255.0f);
        }
#else
        private Color4 GetBlendFactor()
        {
			return BlendFactor.ToColor4();
        }
#endif

        #region Implement IDisposable
        private void PlatformDispose(bool disposing)
        {            
            if (disposing)
            {
                // Release managed objects
                
                _blendState = null;
                _actualBlendState = null;
                _blendStateAdditive.Dispose();
                _blendStateAlphaBlend.Dispose();
                _blendStateNonPremultiplied.Dispose();
                _blendStateOpaque.Dispose();
            }

            // Release native objects
            SharpDX.Utilities.Dispose(ref _d3dContext);            
        }
        #endregion
    }
    
}
