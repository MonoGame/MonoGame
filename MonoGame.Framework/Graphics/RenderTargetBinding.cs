// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	// http://msdn.microsoft.com/en-us/library/ff434403.aspx
	public struct RenderTargetBinding
	{
        private readonly Texture _renderTarget;
        private readonly int _arraySlice;
        private DepthFormat _depthFormat;

		public Texture RenderTarget 
        {
			get { return _renderTarget; }
		}

        public int ArraySlice
        {
            get { return _arraySlice; }
        }

        internal DepthFormat DepthFormat
        {
            get { return _depthFormat; }
        }

		public RenderTargetBinding(RenderTarget2D renderTarget)
		{
			if (renderTarget == null) 
				throw new ArgumentNullException("renderTarget");

			_renderTarget = renderTarget;
            _arraySlice = (int)CubeMapFace.PositiveX;
            _depthFormat = renderTarget.DepthStencilFormat;
		}

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

        public RenderTargetBinding(RenderTarget3D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");

            _renderTarget = renderTarget;
            _arraySlice = 0;
            _depthFormat = renderTarget.DepthStencilFormat;
        }

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

        public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

#if DIRECTX

        public static implicit operator RenderTargetBinding(RenderTarget3D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

#endif
	}
}
