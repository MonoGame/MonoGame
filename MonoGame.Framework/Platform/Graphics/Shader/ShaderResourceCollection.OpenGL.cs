// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class ShaderResourceCollection
    {
#if WEB
        internal void PlatformApplyAllResourcesToDevice(GraphicsDevice device, int shaderProgram)
#elif OPENGL
        internal void PlatformApplyAllResourcesToDevice(GraphicsDevice device, ShaderProgram shaderProgram)
#endif
        {
            if (_readonlyDirty != 0)
                ApplyResources(device, shaderProgram, _readonlyResources, ref _readonlyDirty, false);
            if (_writeableResources != null && _writeableDirty != 0)
                ApplyResources(device, shaderProgram, _writeableResources, ref _writeableDirty, true);
        }

        private void ApplyResources(GraphicsDevice device, ShaderProgram shaderProgram, ResourceBinding[] resources, ref int dirty, bool writeAccess)
        {
            for (var i = 0; i < resources.Length; i++)
            {
                var mask = 1 << i;
                if ((dirty & mask) == 0)
                    continue;
            
                var resourceBinding = resources[i];
            
                // The SampleStateCollection is responsible for binding textures accessed via samplers
                if (!resourceBinding.useSampler)
                {
                    var resource = resourceBinding.resource;
                    if (resource != null && !resource.IsDisposed)
                        resource.PlatformApply(device, shaderProgram, ref resourceBinding, writeAccess);
                }
            
                // Early out if this is the last one.
                dirty &= ~(1 << i);
                if (dirty == 0)
                    break;
            }
            
            dirty = 0;
        }
    }
}
