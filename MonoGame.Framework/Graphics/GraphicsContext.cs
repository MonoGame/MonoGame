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
        
        private void Initialize(GraphicsDevice device)
        {
            _device = device;
        }
        
        ~GraphicsContext()
        {
            Dispose(false);
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
