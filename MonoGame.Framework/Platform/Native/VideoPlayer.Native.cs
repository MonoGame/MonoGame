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

    }

    private Texture2D PlatformGetTexture()
    {
        throw new NotImplementedException();
    }

    private void PlatformGetState(ref MediaState result)
    {
    }

    private void PlatformPause()
    {

    }

    private void PlatformResume()
    {

    }

    private void PlatformPlay()
    {

    }

    private void PlatformStop()
    {

    }

    private TimeSpan PlatformGetPlayPosition()
    {
        return TimeSpan.Zero;
    }

    private void PlatformSetIsLooped()
    {

    }

    private void PlatformSetIsMuted()
    {

    }

    private TimeSpan PlatformSetVolume()
    {
        return TimeSpan.Zero;
    }

    private void PlatformDispose(bool disposing)
    {

    }
}
