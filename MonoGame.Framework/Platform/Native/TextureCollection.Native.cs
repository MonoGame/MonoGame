// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public sealed partial class TextureCollection
{
    private void PlatformInit()
    {
    }

    private void PlatformClear()
    {
    }

    private unsafe void PlatformSetTextures(GraphicsDevice device)
    {
        // Skip out if nothing has changed.
        if (_dirty == 0)
            return;

        for (var i = 0; i < _textures.Length; i++)
        {
            var mask = 1 << i;
            if ((_dirty & mask) == 0)
                continue;

            var tex = _textures[i];

            if (_textures[i] == null || _textures[i].IsDisposed)
                MGG.GraphicsDevice_SetTexture(device.Handle, _stage, i, null);
            else
            {
                MGG.GraphicsDevice_SetTexture(device.Handle, _stage, i, tex.Handle);
                unchecked
                {
                    _graphicsDevice._graphicsMetrics._textureCount++;
                }
            }

            _dirty &= ~mask;
            if (_dirty == 0)
                break;
        }

        _dirty = 0;
    }
}
