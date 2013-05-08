// #region License
// /*
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
// */
// #endregion License
// 
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
		protected SurfaceFormat format;
		protected int levelCount;

#if DIRECTX

        protected SharpDX.Direct3D11.Resource _texture;

        private SharpDX.Direct3D11.ShaderResourceView _resourceView;

#elif OPENGL
		internal int glTexture = -1;
		internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        internal SamplerState glLastSamplerState = null;
#endif
		
		public SurfaceFormat Format
		{
			get { return format; }
		}
		
		public int LevelCount
		{
			get { return levelCount; }
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

            switch (format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbEtc1:
                    Debug.Assert(MathHelper.IsPowerOfTwo(width), "This format must be power of two!");
                    pitch = ((width + 3) / 4) * 8;
                    break;

                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                    Debug.Assert(MathHelper.IsPowerOfTwo(width), "This format must be power of two!");
                    pitch = ((width + 3) / 4) * 16;
                    break;

                case SurfaceFormat.Alpha8:
                    pitch = width;
                    break;

                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.NormalizedByte2:
                case SurfaceFormat.HalfSingle:
                    pitch = width * 2;
                    break;

                case SurfaceFormat.Color:
                case SurfaceFormat.Single:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Bgr32:
                case SurfaceFormat.Bgra32:
                    pitch = width * 4;
                    break;

                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Vector2:
                    pitch = width * 8;
                    break;

                case SurfaceFormat.Vector4:
                    pitch = width * 16;
                    break;

                default:
                    throw new NotImplementedException( "Unexpected format!" );
            };

            return pitch;
        }

#if DIRECTX

        internal SharpDX.Direct3D11.ShaderResourceView GetShaderResourceView()
        {
            if (_resourceView == null)
                _resourceView = new SharpDX.Direct3D11.ShaderResourceView(GraphicsDevice._d3dDevice, _texture);

            return _resourceView;
        }

#endif

        internal protected override void GraphicsDeviceResetting()
        {
#if OPENGL
            this.glTexture = -1;
            this.glLastSamplerState = null;
#endif
        }

        protected override void Dispose(bool disposing)
		{
            if (!IsDisposed)
            {
#if DIRECTX
                if (disposing)
                {
                    if (_resourceView != null)
                    {
                        _resourceView.Dispose();
                        _resourceView = null;
                    }

                    if (_texture != null)
                    {
                        _texture.Dispose();
                        _texture = null;
                    }
                }
#elif OPENGL
                GraphicsDevice.AddDisposeAction(() =>
                    {
                        GL.DeleteTextures(1, ref glTexture);
                        GraphicsExtensions.CheckGLError();
                        glTexture = -1;
                    });

                glLastSamplerState = null;
#endif
            }
            base.Dispose(disposing);
		}
		
	}
}

