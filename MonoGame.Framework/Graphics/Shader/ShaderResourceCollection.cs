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
            public string bufferName;
        }

        private readonly ResourceInfo[] _readonlyResources;
        private readonly ResourceInfo[] _writeableResources;

        private ShaderStage _stage;

        public int MaxReadableBuffers { get { return _readonlyResources.Length; } }
        public int MaxWriteableBuffers { get { return _writeableResources.Length; } }

        internal ShaderResourceCollection(ShaderStage stage, int maxReadableBuffers, int maxWriteableBuffers)
		{
            _stage = stage;

            _readonlyResources = new ResourceInfo[maxReadableBuffers];
            _writeableResources = new ResourceInfo[maxWriteableBuffers];
        }

        public void SetResourceAtIndex(ShaderResource buffer, string bufferName, int index, bool writeAccess)
        {
            if (writeAccess && _stage != ShaderStage.Compute)
                throw new ArgumentException("Only a compute shader can use RWStructuredBuffer currently. Uae a regular StructuredBuffer instead and assign it the same buffer.");

            var resources = writeAccess ? _writeableResources : _readonlyResources;

            resources[index] = new ResourceInfo
            {
                resource = buffer,
                bufferName = bufferName,
            };
        }

        internal void Clear()
        {
            for (var i = 0; i < _readonlyResources.Length; i++)
                _readonlyResources[i] = new ResourceInfo();

            for (var i = 0; i < _writeableResources.Length; i++)
                _writeableResources[i] = new ResourceInfo();
        }

#if WEB
        internal void SetShaderResources(GraphicsDevice device, int shaderProgram)
#elif OPENGL
        internal void SetShaderResources(GraphicsDevice device, ShaderProgram shaderProgram)
#else
        internal void SetShaderResources(GraphicsDevice device)
#endif
        {
            for (var i = 0; i < _readonlyResources.Length; i++)
            {
                var resource = _readonlyResources[i].resource;
                if (resource != null && !resource.IsDisposed)
                {
#if OPENGL || WEB
                    resource.PlatformApply(device, shaderProgram, _readonlyResources[i].bufferName, i, false);
#else
                    resource.PlatformApply(device, _stage, i, false);
#endif
                }
            }

            for (var i = 0; i < _writeableResources.Length; i++)
            {
                var resource = _writeableResources[i].resource;
                if (resource != null && !resource.IsDisposed)
                {
#if OPENGL || WEB
                    resource.PlatformApply(device, shaderProgram, _writeableResources[i].bufferName, i, true);
#else
                    resource.PlatformApply(device, _stage, i, true);
#endif
                }
            }
        }
    }
}
