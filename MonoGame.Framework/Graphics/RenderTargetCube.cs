#region License
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
#endregion License

using System;
#if DIRECTX
using SharpDX.DXGI;
using SharpDX.Direct3D11;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a texture cube that can be used as a render target.
    /// </summary>
    public class RenderTargetCube : TextureCube, IRenderTarget
    {
#if DIRECTX
        private RenderTargetView[] _renderTargetViews;
        private DepthStencilView _depthStencilView;
#endif

        /// <summary>
        /// Gets the depth-stencil buffer format of this render target.
        /// </summary>
        /// <value>The format of the depth-stencil buffer.</value>
        public DepthFormat DepthStencilFormat { get; private set; }

        /// <summary>
        /// Gets the number of multisample locations.
        /// </summary>
        /// <value>The number of multisample locations.</value>
        public int MultiSampleCount { get; private set; }

        /// <summary>
        /// Gets the usage mode of this render target.
        /// </summary>
        /// <value>The usage mode of the render target.</value>
        public RenderTargetUsage RenderTargetUsage { get; private set; }

        /// <inheritdoc/>
        int IRenderTarget.Width
        {
            get { return size; }
        }

        /// <inheritdoc/>
        int IRenderTarget.Height
        {
            get { return size; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetCube"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="size">The width and height of a texture cube face in pixels.</param>
        /// <param name="mipMap"><see langword="true"/> to generate a full mipmap chain; otherwise <see langword="false"/>.</param>
        /// <param name="preferredFormat">The preferred format of the surface.</param>
        /// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
        public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat)
            : this(graphicsDevice, size, mipMap, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.DiscardContents)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTargetCube"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="size">The width and height of a texture cube face in pixels.</param>
        /// <param name="mipMap"><see langword="true"/> to generate a full mipmap chain; otherwise <see langword="false"/>.</param>
        /// <param name="preferredFormat">The preferred format of the surface.</param>
        /// <param name="preferredDepthFormat">The preferred format of the depth-stencil buffer.</param>
        /// <param name="preferredMultiSampleCount">The preferred number of multisample locations.</param>
        /// <param name="usage">The usage mode of the render target.</param>
        public RenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
            : base(graphicsDevice, size, mipMap, preferredFormat, true)
        {
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = preferredMultiSampleCount;
            RenderTargetUsage = usage;

#if DIRECTX
            // Create one render target view per cube map face.
            _renderTargetViews = new RenderTargetView[6];
            for (int i = 0; i < _renderTargetViews.Length; i++)
            {
                var renderTargetViewDescription = new RenderTargetViewDescription
                {
                    Dimension = RenderTargetViewDimension.Texture2DArray,
                    Format = SharpDXHelper.ToFormat(preferredFormat),
                    Texture2DArray =
                    {
                        ArraySize = 1,
                        FirstArraySlice = i,
                        MipSlice = 0
                    }
                };

                _renderTargetViews[i] = new RenderTargetView(graphicsDevice._d3dDevice, GetTexture(), renderTargetViewDescription);
            }

            // If we don't need a depth buffer then we're done.
            if (preferredDepthFormat == DepthFormat.None)
                return;

            var sampleDescription = new SampleDescription(1, 0);
            if (preferredMultiSampleCount > 1)
            {
                sampleDescription.Count = preferredMultiSampleCount;
                sampleDescription.Quality = (int)StandardMultisampleQualityLevels.StandardMultisamplePattern;
            }

            var depthStencilDescription = new Texture2DDescription
            {
                Format = SharpDXHelper.ToFormat(preferredDepthFormat),
                ArraySize = 1,
                MipLevels = 1,
                Width = size,
                Height = size,
                SampleDescription = sampleDescription,
                BindFlags = BindFlags.DepthStencil,
            };

            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, depthStencilDescription))
            {
                var depthStencilViewDescription = new DepthStencilViewDescription
                {
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Format = SharpDXHelper.ToFormat(preferredDepthFormat),
                };
                _depthStencilView = new DepthStencilView(graphicsDevice._d3dDevice, depthBuffer, depthStencilViewDescription);
            }
#else
            throw new NotImplementedException();
#endif            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if DIRECTX
                if (_renderTargetViews != null)
                {
                    for (var i = 0; i < _renderTargetViews.Length; i++)
                        _renderTargetViews[i].Dispose();

                    _renderTargetViews = null;
                    SharpDX.Utilities.Dispose(ref _depthStencilView);
                }
#endif
            }

            base.Dispose(disposing);
        }

#if DIRECTX
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public RenderTargetView GetRenderTargetView(int arraySlice)
        {
            return _renderTargetViews[arraySlice];
        }

        /// <inheritdoc/>
        [CLSCompliant(false)]
        public DepthStencilView GetDepthStencilView()
        {
            return _depthStencilView;
        }
#endif
    }
}
