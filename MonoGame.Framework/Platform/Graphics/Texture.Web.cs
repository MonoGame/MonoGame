// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal int glTexture = -1;
        internal SamplerState glLastSamplerState;

        private void PlatformGraphicsDeviceResetting()
        {
            glTexture = -1;
            glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                glTexture = -1;
                glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }
    }
}

