// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class ShaderResourceCollection
    {
        // cached lists of different sizes to avoid garbage when setting UAV's for the pixel shader stage
        static UnorderedAccessView[][] uavListsOfAllSizes = new UnorderedAccessView[GraphicsDevice.MaxUavSlotsPerStage+1][];
        static int[][] intListsOfAllSizes = new int[GraphicsDevice.MaxUavSlotsPerStage+1][];

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

                    // we can't use the same method we used the compute stage, where we set resources to slots one by one
                    // with the output merger, we have to set all UAV's at once. setting a single one would clear the previous ones
                    // UAV's have to be set after the render targets
                    int numRTs = 0;
                    for (var i = 0; i < activeRenderTargets.Length; i++)
                    {
                        if (activeRenderTargets[i] != null)
                            numRTs = i + 1;
                    }

                    // grab a UAV list of the right size from the cashe to avoid garbage
                    var maxUAVs = _writeableResources.Length - numRTs;
                    var unorderedAccessViews = uavListsOfAllSizes[maxUAVs] ??= new UnorderedAccessView[maxUAVs];
                    var uavInitialCounts = intListsOfAllSizes[maxUAVs] ??= new int[maxUAVs];
                    
                    for (var i = 0; i < maxUAVs; i++)
                    {
                        int slot = numRTs + i;
                        var resourceInfo = _writeableResources[slot];
                        var resource = resourceInfo.resource;
                        if (resource != null && !resource.IsDisposed)
                        {
                            unorderedAccessViews[i] = resource.GetUnorderedAccessView();
                            uavInitialCounts[i] = resource.CounterBufferResetValue;
                        }
                        else
                        {
                            unorderedAccessViews[i] = null;
                            uavInitialCounts[i] = 0;
                        }
                    }

                    outputMerger.SetUnorderedAccessViews(numRTs, unorderedAccessViews, uavInitialCounts);
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
