// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class StructuredBuffer : BufferResource
    {
        public new int ElementCount { get { return base.ElementCount; } }

        public StructuredBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage, ShaderAccess shaderAccess) :
            base(graphicsDevice,
                vertexCount,
                ReflectionHelpers.ManagedSizeOf(type),
                bufferUsage,
                Options.BufferStructured | Options.GPURead | (shaderAccess == ShaderAccess.ReadWrite ? Options.GPUWrite : Options.None))
        {
        }
    }
}
