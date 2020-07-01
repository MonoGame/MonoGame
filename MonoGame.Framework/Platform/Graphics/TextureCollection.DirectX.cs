﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        void PlatformInit()
        {
        }

        internal void ClearTargets(GraphicsDevice device, RenderTargetBinding[] targets)
        {
            if (_applyToVertexStage && !device.GraphicsCapabilities.SupportsVertexTextures)
                return;

            if (_applyToVertexStage)
                ClearTargets(targets, device._d3dContext.VertexShader);
            else
                ClearTargets(targets, device._d3dContext.PixelShader);
        }

        private void ClearTargets(RenderTargetBinding[] targets, SharpDX.Direct3D11.CommonShaderStage shaderStage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // We assume 4 targets to avoid a loop within a loop below.
            var target0 = targets[0].RenderTarget;
            var target1 = targets[1].RenderTarget;
            var target2 = targets[2].RenderTarget;
            var target3 = targets[3].RenderTarget;

            // Make one pass across all the texture slots.
            for (var i = 0; i < _textures.Length; i++)
            {
                if (_textures[i] == null)
                    continue;

                if (_textures[i] != target0 &&
                    _textures[i] != target1 &&
                    _textures[i] != target2 &&
                    _textures[i] != target3)
                    continue;

                // Immediately clear the texture from the device.
                _dirty &= ~(1 << i);
                _textures[i] = null;
                shaderStage.SetShaderResource(i, null);
            }
        }

        void PlatformClear()
        {
        }

        void PlatformSetTextures(GraphicsDevice device)
        {
            // Skip out if nothing has changed.
            if (_dirty == 0)
                return;

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.
            SharpDX.Direct3D11.CommonShaderStage shaderStage;
            if (_applyToVertexStage)
                shaderStage = device._d3dContext.VertexShader;
            else
                shaderStage = device._d3dContext.PixelShader;

            for (var i = 0; i < _textures.Length; i++)
            {
                var mask = 1 << i;
                if ((_dirty & mask) == 0)
                    continue;

                var tex = _textures[i];

                if (_textures[i] == null || _textures[i].IsDisposed)
                    shaderStage.SetShaderResource(i, null);
                else
                {
                    shaderStage.SetShaderResource(i, _textures[i].GetShaderResourceView());
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
}
