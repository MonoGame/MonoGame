// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class ShaderResourceCollection
    {
        internal void PlatformApplyAllResourcesToDevice(GraphicsDevice device)
        {
            var shaderStageDX = device.GetDXShaderStage(_stage);

            // set readable resources
            if (_readonlyDirty != 0) // If there are no readable resources then skip it.
            {
                for (var i = 0; i < _readonlyResources.Length; i++)
                {
                    var mask = 1 << i;
                    if ((_readonlyDirty & mask) == 0)
                        continue;

                    var resourceBinding = _readonlyResources[i];
                    var resource = resourceBinding.resource;
                    if (resource == null || resource.IsDisposed)
                        shaderStageDX.SetShaderResource(i, null);
                    else
                        shaderStageDX.SetShaderResource(i, resource.GetShaderResourceView());

                    // Early out if this is the last one.
                    _readonlyDirty &= ~mask;
                    if (_readonlyDirty == 0)
                        break;
                }

                _readonlyDirty = 0;
            }

            // set writeable resources (only compute and pixel shader supported with DX 11)
            if (_writeableResources != null && _writeableDirty != 0) // If there are no writeable resources then skip it.
            {
                if (_stage == ShaderStage.Compute)
                {
                    var computeStage = (SharpDX.Direct3D11.ComputeShaderStage)shaderStageDX; 

                    for (var i = 0; i < _writeableResources.Length; i++)
                    {
                        var mask = 1 << i;
                        if ((_writeableDirty & mask) == 0)
                            continue;

                        var resourceInfo = _writeableResources[i];
                        var resource = resourceInfo.resource;
                        if (resource == null || resource.IsDisposed)
                            computeStage.SetUnorderedAccessView(i, null);
                        else
                            computeStage.SetUnorderedAccessView(i, resource.GetUnorderedAccessView(), resource.CounterBufferResetValue);

                        // Early out if this is the last one.
                        _writeableDirty &= ~mask;
                        if (_writeableDirty == 0)
                            break;
                    }
                }
                else if (_stage == ShaderStage.Pixel)
                {
                    var outputMerger = device._d3dContext.OutputMerger;
                    var activeRenderTargets = device._currentRenderTargets;

                    for (var i = 0; i < _writeableResources.Length; i++)
                    {
                        var mask = 1 << i;
                        if ((_writeableDirty & mask) == 0)
                            continue;

                        // don't overwrite render targets bound to this slot
                        if (activeRenderTargets[i] != null)
                            continue;

                        var resourceInfo = _writeableResources[i];
                        var resource = resourceInfo.resource;
                        if (resource == null || resource.IsDisposed)
                            outputMerger.SetUnorderedAccessView(i, null);
                        else
                            outputMerger.SetUnorderedAccessView(i, resource.GetUnorderedAccessView(), resource.CounterBufferResetValue);

                        // Early out if this is the last one.
                        _writeableDirty &= ~mask;
                        if (_writeableDirty == 0)
                            break;
                    }
                }

                _writeableDirty = 0;
            }
        }

        internal void ClearTargets(RenderTargetBinding[] targets, SharpDX.Direct3D11.CommonShaderStage shaderStage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Make one pass across all the resource slots.
            for (var i = 0; i < _readonlyResources.Length; i++)
            {
                if (_readonlyResources[i].resource == null)
                    continue;

                for (int k = 0; k < targets.Length; k++)
                {
                    if (_readonlyResources[i].resource == targets[k].RenderTarget)
                    {
                        // Immediately clear the texture from the device.
                        _readonlyDirty &= ~(1 << i);
                        _readonlyResources[i].resource = null;
                        shaderStage.SetShaderResource(i, null);
                        break;
                    }
                }
            }
        }
    }
}
