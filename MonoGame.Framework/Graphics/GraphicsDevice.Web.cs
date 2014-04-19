// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        private void PlatformSetup()
        {
            throw new NotImplementedException();
        }

        private void PlatformInitialize()
        {
            throw new NotImplementedException();
        }

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose()
        {
            throw new NotImplementedException();
        }

        public void PlatformPresent()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            throw new NotImplementedException();
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            throw new NotImplementedException();
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            throw new NotImplementedException();
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            throw new NotImplementedException();
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            throw new NotImplementedException();
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            throw new NotImplementedException();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {
            throw new NotImplementedException();
        }

        private static GraphicsProfile PlatformGetHighestSupportedGraphicsProfile(GraphicsDevice graphicsDevice)
        {
            return GraphicsProfile.HiDef;
        }
    }
}
