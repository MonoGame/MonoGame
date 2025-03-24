// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public partial class RenderTargetCube
{
    private unsafe void PlatformConstruct(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
    {
        Handle = MGG.RenderTarget_Create(
            GraphicsDevice.Handle,
            TextureType.Cube,
            _format,
            Size,
            Size,
            1,
            _levelCount,
            6,
            preferredDepthFormat,
            preferredMultiSampleCount,
            usage);
    }
}
