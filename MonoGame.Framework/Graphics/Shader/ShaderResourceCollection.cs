// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class ShaderResourceCollection
    {
        struct ResourceInfo
        {
            public ShaderResource resource;
            public string blockName; // name of the OpenGL block
        }

        private readonly ResourceInfo[] _readonlyResources;
        private readonly ResourceInfo[] _writeableResources;

        private int _readonlyValid;
        private int _writeableValid;

        private ShaderStage _stage;

        public int MaxReadableResources { get { return _readonlyResources.Length; } }
        public int MaxWriteableResources { get { return _writeableResources.Length; } }

        internal ShaderResourceCollection(ShaderStage stage, int maxReadableResources, int maxWriteableResources)
		{
            _stage = stage;

            _readonlyResources = new ResourceInfo[maxReadableResources];
            _writeableResources = new ResourceInfo[maxWriteableResources];
        }

        public void SetResourceAtIndex(ShaderResource resource, string blockName, int index, bool writeAccess)
        {
            if (writeAccess && _stage != ShaderStage.Compute)
                throw new ArgumentException("Only a compute shader can use RWStructuredBuffer currently. Uae a regular StructuredBuffer instead and assign it the same buffer.");

            var resources = writeAccess ? _writeableResources : _readonlyResources;

            resources[index] = new ResourceInfo
            {
                resource = resource,
                blockName = blockName,
            };

            if (writeAccess)
                _writeableValid |= 1 << index;
            else
                _readonlyValid |= 1 << index;
        }

        internal void Clear()
        {
            for (var i = 0; i < _readonlyResources.Length; i++)
                _readonlyResources[i] = new ResourceInfo();

            for (var i = 0; i < _writeableResources.Length; i++)
                _writeableResources[i] = new ResourceInfo();

            _readonlyValid = 0;
            _writeableValid = 0;
        }

#if WEB
        internal void SetShaderResources(GraphicsDevice device, int shaderProgram)
#elif OPENGL
        internal void SetShaderResources(GraphicsDevice device, ShaderProgram shaderProgram)
#else
        internal void SetShaderResources(GraphicsDevice device)
#endif
        {
            if (_readonlyValid != 0) // If there are no readable resources then skip it.
            {
                int valid = _readonlyValid;

                for (var i = 0; i < _readonlyResources.Length; i++)
                {
                    var resource = _readonlyResources[i].resource;
                    if (resource != null && !resource.IsDisposed)
                    {
#if OPENGL || WEB
                        resource.PlatformApply(device, shaderProgram, _readonlyResources[i].blockName, i, false);
#else
                        resource.PlatformApply(device, _stage, i, false);
#endif
                    }

                    // Early out if this is the last one.
                    valid &= ~(1 << i);
                    if (valid == 0)
                        break;
                }
            }

            if (_writeableValid != 0) // If there are no writeable resources then skip it.
            {
                int valid = _writeableValid;

                for (var i = 0; i < _writeableResources.Length; i++)
                {
                    var resource = _writeableResources[i].resource;
                    if (resource != null && !resource.IsDisposed)
                    {
#if OPENGL || WEB
                        resource.PlatformApply(device, shaderProgram, _writeableResources[i].blockName, i, true);
#else
                        resource.PlatformApply(device, _stage, i, true);
#endif
                    }

                    // Early out if this is the last one.
                    valid &= ~(1 << i);
                    if (valid == 0)
                        break;
                }
            }
        }
    }
}
