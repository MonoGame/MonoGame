// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

public sealed partial class SamplerStateCollection
{
    private int _dirty;

    private void PlatformSetSamplerState(int index)
    {
        _dirty |= 1 << index;
    }

    private void PlatformClear()
    {
        _dirty = int.MaxValue;
    }

    private void PlatformDirty()
    {
        _dirty = int.MaxValue;
    }

    internal unsafe void PlatformSetSamplers(GraphicsDevice device)
    {
        if (_stage == ShaderStage.Vertex && !device.GraphicsCapabilities.SupportsVertexTextures)
            return;

        // Skip out if nothing has changed.
        if (_dirty == 0)
            return;

        var handle = device.Handle;

        for (var i = 0; i < _actualSamplers.Length; i++)
        {
            var mask = 1 << i;
            if ((_dirty & mask) == 0)
                continue;

            var sampler = _actualSamplers[i];
            if (sampler != null)
            {
                var state = sampler.GetHandle(device);
                MGG.GraphicsDevice_SetSamplerState(handle, _stage, i, state);
            }

            _dirty &= ~mask;
            if (_dirty == 0)
                break;
        }

        _dirty = 0;
    }
}
