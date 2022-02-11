// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal struct ResourceBinding
    {
        public ShaderResource resource;
#if OPENGL
        public int bindingSlot;
        public int bindingSlotForCounter; // in OpenGL structured buffers with append/consume/counter functionality are emulated using a separate counter buffer
        public bool useSampler; 
#endif
    }

    public sealed partial class ShaderResourceCollection
    {
        private readonly ResourceBinding[] _readonlyResources;
        private readonly ResourceBinding[] _writeableResources;

        private int _readonlyDirty;
        private int _writeableDirty;

        private ShaderStage _stage;

        public int MaxReadableResources { get { return _readonlyResources.Length; } }
        public int MaxWriteableResources { get { return _writeableResources == null ? 0 : _writeableResources.Length; } }

        internal ShaderResourceCollection(ShaderStage stage, int maxReadableResources, int maxWriteableResources)
		{
            _stage = stage;

            _readonlyResources = new ResourceBinding[maxReadableResources];
            _writeableResources = maxWriteableResources > 0 ? new ResourceBinding[maxWriteableResources] : null;
        }

        public ShaderResource this[int index]
        {
            get { return _readonlyResources[index].resource; }
            set { SetResourceForBindingSlot(value, index, false, true, -1); }
        }

        public void SetResourceForBindingSlot(ShaderResource resource, int bindingSlot, bool writeAccess, bool useSampler = false, int bindingSlotForCounter = -1)
        {
            if (writeAccess && _stage != ShaderStage.Compute)
                throw new ArgumentException("Only a compute shader can use RWStructuredBuffer currently. Uae a regular StructuredBuffer instead and assign it the same buffer.");

            var resources = writeAccess ? _writeableResources : _readonlyResources;

#if OPENGL
            // DX uses u-registers in shaders for writeable buffers and textures, and t-registers for readonly buffers and textures.
            // OpenGL doesn't separate register types like this. If a shader resource is assigned to register u0, and another resource is assigned to register t0,
            // things are fine in DX, but in GL we have a binding slot conflict. To resolve this u-registers have been shifted by 16, if set explicitly (see ShaderConductor shiftAllUABuffersBindings option in MGFXC).
            // Unshift those binding slots now, to avoid an array index overflow.
            resources[bindingSlot % GraphicsDevice.UavRegisterShiftMGFXC] = new ResourceBinding
            {
                resource = resource,
                bindingSlot = bindingSlot,
                bindingSlotForCounter = bindingSlotForCounter,
                useSampler = useSampler,
            };
#else
            resources[bindingSlot] = new ResourceBinding
            {
                resource = resource,
            };
#endif

            if (writeAccess)
                _writeableDirty |= 1 << bindingSlot;
            else
                _readonlyDirty |= 1 << bindingSlot;
        }

        internal void Clear()
        {
            for (var i = 0; i < _readonlyResources.Length; i++)
                _readonlyResources[i].resource = null;

            if (_writeableResources != null)
            {
                for (var i = 0; i < _writeableResources.Length; i++)
                    _writeableResources[i].resource = null;

            }

            _readonlyDirty = ~0;
            _writeableDirty = ~0;
        }

        internal void DirtyReadonly()
        {
            _readonlyDirty = ~0;
        }
    }
}
