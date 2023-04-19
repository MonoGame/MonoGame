// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum ShaderAccess
    {
        None,
        Read,
        ReadWrite,
    }

    internal enum ShaderResourceType
    {
        StructuredBuffer = 0,
        RWStructuredBuffer = 1,
        ByteBuffer = 2,
        RWByteBuffer = 3,
        RWTexture = 4,
    }

    public abstract partial class ShaderResource : GraphicsResource
    {
        public ShaderAccess ShaderAccess { get; private set; }

        internal int CounterBufferResetValue = -1;

        public ShaderResource(ShaderAccess shaderAccess)
        {
            ShaderAccess = shaderAccess;
        }
/*
#if OPENGL || WEB
        internal abstract void PlatformApply(GraphicsDevice device, ShaderProgram program, ref ResourceBinding resourceBinding, bool writeAcess);
#else
        internal abstract void PlatformApply(GraphicsDevice device, ShaderStage stage, int bindingSlot, bool writeAcess);
#endif
*/
        internal static bool IsResourceTypeWriteable(ShaderResourceType type)
        {
            return type == ShaderResourceType.RWStructuredBuffer ||
                   type == ShaderResourceType.RWTexture ||
                   type == ShaderResourceType.RWByteBuffer;
        }
    }
}
