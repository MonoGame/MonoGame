// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsContext : IDisposable
    {
        public GraphicsContext(GraphicsDevice device)
        {
            Initialize(device);
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
            // ...          
        }
        #endregion
    }
    
}
