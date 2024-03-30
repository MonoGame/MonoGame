// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Video : IDisposable
    {
        private void PlatformInitialize()
        {
            throw new NotImplementedException("Video is not implemented on this platform.");
        }

        private void PlatformDispose(bool disposing)
        {
            throw new NotImplementedException("Video is not implemented on this platform.");
        }
    }
}
