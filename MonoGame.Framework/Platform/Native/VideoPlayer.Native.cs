// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media;

public sealed partial class VideoPlayer : IDisposable
{
    private void PlatformInitialize()
    {
        // Nothing to do... the Video does the work.
    }

    private Texture2D PlatformGetTexture()
    {
        return _currentVideo.GetTexture();
    }

    private void PlatformGetState(ref MediaState result)
    {
        result = _currentVideo.State;
    }

    private void PlatformPause()
    {
        _currentVideo.Pause();
    }

    private void PlatformResume()
    {
        _currentVideo.Resume();
    }

    private void PlatformPlay()
    {
        _currentVideo.Play();
    }

    private void PlatformStop()
    {
        _currentVideo.Stop();
    }

    private TimeSpan PlatformGetPlayPosition()
    {
        return _currentVideo.Position;
    }

    private void PlatformSetIsLooped()
    {
        _currentVideo.IsLooped = _isLooped;
    }

    private void PlatformSetIsMuted()
    {
        _currentVideo.IsMuted = _isMuted;
    }

    private void PlatformSetVolume()
    {
        _currentVideo.Volume = _volume;
    }

    private void PlatformDispose(bool disposing)
    {

    }
}
