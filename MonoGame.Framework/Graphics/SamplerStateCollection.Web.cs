// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
    {
        private void PlatformSetSamplerState(int index)
        {
            throw new NotImplementedException();
        }

        private void PlatformClear()
        {
            throw new NotImplementedException();
        }

        private void PlatformDirty()
        {
            throw new NotImplementedException();
        }

        internal void PlatformSetSamplers(GraphicsDevice device)
        {
            throw new NotImplementedException();
        }
	}
}
