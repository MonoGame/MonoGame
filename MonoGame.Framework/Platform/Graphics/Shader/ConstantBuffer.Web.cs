// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class ConstantBuffer
    {
        private void PlatformInitialize()
        {
            throw new NotImplementedException();
        }

        private void PlatformClear()
        {
            throw new NotImplementedException();
        }

        public unsafe void PlatformApply(GraphicsDevice device, int program)
        {
            throw new NotImplementedException();
        }
    }
}
