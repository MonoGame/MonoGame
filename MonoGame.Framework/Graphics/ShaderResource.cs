// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum ShaderAccess
    {
        Read,
        ReadWrite,
    }

    internal enum ShaderResourceType
    {
        Structured = 0,
        RWStructured = 1,
        RWTexture = 2,
    }

    public abstract class ShaderResource : GraphicsResource
    {
        protected internal ShaderAccess _shaderAccess;

        internal static bool IsResourceTypeWriteable(ShaderResourceType type)
        {
            return type == ShaderResourceType.RWStructured || type == ShaderResourceType.RWTexture;
        }

#if OPENGL || WEB
        internal abstract void PlatformApply(GraphicsDevice device, ShaderProgram program, string paramName, int bindingSlot, bool writeAcess);
#else
        internal abstract void PlatformApply(GraphicsDevice device, ShaderStage stage, int bindingSlot, bool writeAcess);
#endif
    }
}
