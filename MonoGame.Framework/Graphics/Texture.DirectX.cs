// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal SharpDX.Direct3D11.Resource _texture;

        private SharpDX.Direct3D11.ShaderResourceView _resourceView;

        /// <summary>
        /// Gets the handle to a shared resource.
        /// </summary>
        /// <returns>
        /// The handle of the shared resource, or <see cref="IntPtr.Zero"/> if the texture was not
        /// created as a shared resource.
        /// </returns>
        public IntPtr GetSharedHandle()
        {
            using (var resource = _texture.QueryInterface<SharpDX.DXGI.Resource>())
                return resource.SharedHandle;
        }

        internal abstract SharpDX.Direct3D11.Resource CreateTexture();

        internal SharpDX.Direct3D11.Resource GetTexture()
        {
            if (_texture == null)
                _texture = CreateTexture();

            return _texture;
        }

        internal SharpDX.Direct3D11.ShaderResourceView GetShaderResourceView()
        {
            if (_resourceView == null)
                _resourceView = CreateShaderResourceView();

            return _resourceView;
        }

        protected virtual SharpDX.Direct3D11.ShaderResourceView
            CreateShaderResourceView()
        {
            return new SharpDX.Direct3D11.ShaderResourceView(GraphicsDevice._d3dDevice, GetTexture());
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _resourceView);
            SharpDX.Utilities.Dispose(ref _texture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _resourceView);
                SharpDX.Utilities.Dispose(ref _texture);
            }

            base.Dispose(disposing);
        }
    }
}

