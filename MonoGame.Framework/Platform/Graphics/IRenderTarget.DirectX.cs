// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial interface IRenderTarget
    {
        /// <summary>
        /// Gets the <see cref="RenderTargetView"/> for the specified array slice.
        /// </summary>
        /// <param name="arraySlice">The array slice.</param>
        /// <returns>The <see cref="RenderTargetView"/>.</returns>
        /// <remarks>
        /// For texture cubes: The array slice is the index of the cube map face.
        /// </remarks>
        RenderTargetView GetRenderTargetView(int arraySlice);

        /// <summary>
        /// Gets the <see cref="DepthStencilView"/>.
        /// </summary>
        /// <returns>The <see cref="DepthStencilView"/>. Can be <see langword="null"/>.</returns>
        DepthStencilView GetDepthStencilView();
    }
}
