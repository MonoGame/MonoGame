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
    using MonoGame.Web;

    public partial class GraphicsDevice
    {
        private void PlatformSetup()
        {
            JSAPIAccess.Instance.GraphicsDevicePlatformSetup();
        }

        private void PlatformInitialize()
        {
        }

        public void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: This is just a test at the moment.
            JSAPIAccess.Instance.GraphicsDevicePlatformClear();
        }

        private void PlatformDispose()
        {
        }

        public void PlatformPresent()
        {
        }

        private void PlatformSetViewport(ref Viewport value)
        {
        }

        private void PlatformApplyDefaultRenderTarget()
        {
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            return null;
        }

        internal void PlatformApplyState(bool applyShaders)
        {
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct, IVertexType
        {
        }

        private static GraphicsProfile PlatformGetHighestSupportedGraphicsProfile(GraphicsDevice graphicsDevice)
        {
            return GraphicsProfile.HiDef;
        }
    }
}
