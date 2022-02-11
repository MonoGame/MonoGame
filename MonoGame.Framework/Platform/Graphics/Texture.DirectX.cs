// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        private Resource _texture;


        /// <summary>
        /// Gets the handle to a shared resource.
        /// </summary>
        /// <returns>
        /// The handle of the shared resource, or <see cref="IntPtr.Zero"/> if the texture was not
        /// created as a shared resource.
        /// </returns>
        public IntPtr GetSharedHandle()
        {
            using (var resource = GetTexture().QueryInterface<SharpDX.DXGI.Resource>())
                return resource.SharedHandle;
        }

        internal abstract Resource CreateTexture();

        internal Resource GetTexture()
        {
            if (_texture == null)
                _texture = CreateTexture();

            return _texture;
        }

        internal override ShaderResourceView CreateShaderResourceView()
        {
            return new ShaderResourceView(GraphicsDevice._d3dDevice, GetTexture());
        }

        internal override UnorderedAccessView CreateUnorderedAccessView()
        {
            return new UnorderedAccessView(GraphicsDevice._d3dDevice, GetTexture(), GetUnorderedAccessViewDescription(0));
        }

        internal abstract UnorderedAccessViewDescription GetUnorderedAccessViewDescription(int mipSlice);

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _resourceView);
            SharpDX.Utilities.Dispose(ref _unorderedAccessView);
            SharpDX.Utilities.Dispose(ref _texture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _resourceView);
                SharpDX.Utilities.Dispose(ref _unorderedAccessView);
                SharpDX.Utilities.Dispose(ref _texture);
            }

            base.Dispose(disposing);
        }
    }
}

