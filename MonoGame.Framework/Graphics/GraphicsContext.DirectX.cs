// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
        
        #region Implement IDisposable
        private void PlatformDispose(bool disposing)
        {            
            if (disposing)
            {
                // Release managed objects
                // ...
            }

            // Release native objects
            SharpDX.Utilities.Dispose(ref _d3dContext);            
        }
        #endregion
    }
    
}
