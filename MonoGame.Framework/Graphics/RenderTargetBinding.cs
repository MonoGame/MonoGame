// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents the binding of a <see cref="RenderTarget2D"/> used in an array of bindings when setting multiple
    /// render targets on the graphics device.
    /// </summary>
	public struct RenderTargetBinding
	{
        private readonly Texture _renderTarget;
        private readonly int _arraySlice;
        private DepthFormat _depthFormat;

        /// <summary>
        /// Gets the the render target texture
        /// </summary>
		public Texture RenderTarget
        {
			get { return _renderTarget; }
		}

        /// <summary>
        ///
        /// </summary>
        public int ArraySlice
        {
            get { return _arraySlice; }
        }

        /// <summary>
        /// Gets the depth format specified for the render target
        /// </summary>
        internal DepthFormat DepthFormat
        {
            get { return _depthFormat; }
        }

        /// <summary>
        /// Creates a new <b>RenderTargetBinding</b> for the specified render target.
        /// </summary>
        /// <param name="renderTarget">The render target to create the binding for.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="renderTarget"/> parameter is null.</exception>
		public RenderTargetBinding(RenderTarget2D renderTarget)
		{
			if (renderTarget == null)
				throw new ArgumentNullException("renderTarget");

			_renderTarget = renderTarget;
            _arraySlice = (int)CubeMapFace.PositiveX;
            _depthFormat = renderTarget.DepthStencilFormat;
		}

        /// <summary>
        /// Creates a new <b>RenderTargetBinding</b> with the specified parameters.
        /// </summary>
        /// <param name="renderTarget">A cube map render target.</param>
        /// <param name="cubeMapFace">The cube map of the render target.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="renderTarget"/> parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="cubeMapFace"/> parameter is less than <see cref="CubeMapFace.PositiveX"/> or greater
        /// than <see cref="CubeMapFace.NegativeZ"/>.
        /// </exception>
        public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
                throw new ArgumentOutOfRangeException("cubeMapFace");

            _renderTarget = renderTarget;
            _arraySlice = (int)cubeMapFace;
            _depthFormat = renderTarget.DepthStencilFormat;
        }

#if DIRECTX

        /// <summary>
        /// Creates a new <b>RenderTargetBinding</b> with the specified parameters.
        /// </summary>
        /// <param name="renderTarget">The render target to create the binding for.</param>
        /// <param name="arraySlice">The index of the texture array slice to bind.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="renderTarget" /> parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="arraySlice" /> is less than zero or greater than the array size of the render target
        /// </exception>
        /// <exception cref="InvalidOperationException">Texture arrays are not supported by the current graphics device.</exception>
        public RenderTargetBinding(RenderTarget2D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (arraySlice < 0 || arraySlice >= renderTarget.ArraySize)
                throw new ArgumentOutOfRangeException("arraySlice");
            if (!renderTarget.GraphicsDevice.GraphicsCapabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            _renderTarget = renderTarget;
            _arraySlice = arraySlice;
            _depthFormat = renderTarget.DepthStencilFormat;
        }

        /// <summary>
        /// Creates a new <b>RenderTargetBinding</b> with the specified parameters.
        /// </summary>
        /// <param name="renderTarget">The 3D render target to create the binding for.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="renderTarget" /> parameter is null.</exception>
        public RenderTargetBinding(RenderTarget3D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");

            _renderTarget = renderTarget;
            _arraySlice = 0;
            _depthFormat = renderTarget.DepthStencilFormat;
        }

        /// <summary>
        /// Creates a new <b>RenderTargetBinding</b> with the specified parameters.
        /// </summary>
        /// <param name="renderTarget">The 3D render target to create the binding for.</param>
        /// <param name="arraySlice">The index of the depth slice to bind.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="renderTarget" /> parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="arraySlice" /> is less than zero or greater than or equal to the depth of the render target.
        /// </exception>
        public RenderTargetBinding(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (arraySlice < 0 || arraySlice >= renderTarget.Depth)
                throw new ArgumentOutOfRangeException("arraySlice");

            _renderTarget = renderTarget;
            _arraySlice = arraySlice;
            _depthFormat = renderTarget.DepthStencilFormat;
        }

#endif

        /// <summary>
        /// Implicitly converts a <see cref="RenderTarget2D"/> instance to a <b>RenderTargetBinding</b> instance.
        /// </summary>
        /// <param name="renderTarget">The render target to convert.</param>
        /// <returns>A new <b>RenderTargetBinding</b> instance bound to the specified render target.</returns>
        public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

#if DIRECTX

        /// <summary>
        /// Implicitly converts a <see cref="RenderTarget3D"/> instance to a <b>RenderTargetBinding</b> instance.
        /// </summary>
        /// <param name="renderTarget">The render target to convert.</param>
        /// <returns>A new <b>RenderTargetBinding</b> instance bound to the specified render target.</returns>
        public static implicit operator RenderTargetBinding(RenderTarget3D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

#endif
	}
}
