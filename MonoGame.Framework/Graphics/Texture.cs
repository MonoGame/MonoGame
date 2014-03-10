// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureUnit = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public abstract class Texture : GraphicsResource
	{
		internal SurfaceFormat _format;
		internal int _levelCount;

#if DIRECTX


        internal SharpDX.Direct3D11.Resource _texture;

	private SharpDX.Direct3D11.ShaderResourceView _resourceView;

#elif OPENGL
		internal int glTexture = -1;
		internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        internal SamplerState glLastSamplerState = null;
#endif
		
		public SurfaceFormat Format
		{
			get { return _format; }
		}
		
		public int LevelCount
		{
			get { return _levelCount; }
		}

#if DIRECTX
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
#endif

        internal static int CalculateMipLevels(int width, int height = 0, int depth = 0)
        {
            int levels = 1;
            int size = Math.Max(Math.Max(width, height), depth);
            while (size > 1)
            {
                size = size / 2;
                levels++;
            }
            return levels;
        }

        internal int GetPitch(int width)
        {
            Debug.Assert(width > 0, "The width is negative!");

            int pitch;

            switch (_format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbEtc1:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                    Debug.Assert(MathHelper.IsPowerOfTwo(width), "This format must be power of two!");
                    pitch = ((width + 3) / 4) * _format.Size();
                    break;

                default:
                    pitch = width * _format.Size();
                    break;
            };

            return pitch;
        }

#if DIRECTX

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
                _resourceView = new SharpDX.Direct3D11.ShaderResourceView(GraphicsDevice._d3dDevice, GetTexture());

            return _resourceView;
        }

#endif

        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }

        private void PlatformGraphicsDeviceResetting()
        {
#if OPENGL
            this.glTexture = -1;
            this.glLastSamplerState = null;
#endif

#if DIRECTX

            SharpDX.Utilities.Dispose(ref _resourceView);
            SharpDX.Utilities.Dispose(ref _texture);

#endif
        }

        protected override void Dispose(bool disposing)
		{
            if (!IsDisposed)
            {
                PlatformDispose(disposing);
            }
            base.Dispose(disposing);
		}

        private void PlatformDispose(bool disposing)
        {
#if DIRECTX
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _resourceView);
                SharpDX.Utilities.Dispose(ref _texture);
            }
#endif
#if OPENGL
            GraphicsDevice.AddDisposeAction(() =>
            {
                GL.DeleteTextures(1, ref glTexture);
                GraphicsExtensions.CheckGLError();
                glTexture = -1;
            });

            glLastSamplerState = null;
#endif
        }
		
	}
}

