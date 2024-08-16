// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private void PlatformInitialize()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformGetState(ref MediaState result)
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformPause()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformResume()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformPlay()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformStop()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformSetIsMuted()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformSetVolume()
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }

        private void PlatformDispose(bool disposing)
        {
            throw new NotImplementedException("VideoPlayer is not implemented on this platform.");
        }
    }
}
