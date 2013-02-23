using System;
using System.Runtime.InteropServices;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using TextureMinFilter = OpenTK.Graphics.ES20.All;
#endif
#elif DIRECTX
using SharpDX;
using SharpDX.Direct3D11;
#elif PSM
using Sce.PlayStation.Core.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class TextureCube : Texture
	{
		protected int size;

        /// <summary>
        /// Gets the width and height of the cube map face in pixels.
        /// </summary>
        /// <value>The width and height of a cube map face in pixels.</value>
        public int Size
        {
            get
            {
                return size;
            }
        }
		
#if DIRECTX

#elif PSM
		//TODO
#else
		PixelInternalFormat glInternalFormat;
		PixelFormat glFormat;
		PixelType glType;
#endif
		
		public TextureCube (GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, size, mipMap, format, false)
		{
        }

        internal TextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");
			
            this.GraphicsDevice = graphicsDevice;
			this.size = size;
            this.format = format;
            this.levelCount = mipMap ? CalculateMipLevels(size) : 1;

#if DIRECTX
            var description = new Texture2DDescription
            {
                Width = size,
                Height = size,
                MipLevels = levelCount,
                ArraySize = 6, // A texture cube is a 2D texture array with 6 textures.
                Format = SharpDXHelper.ToFormat(format),
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.TextureCube
            };

            if (renderTarget)
            {
                description.BindFlags |= BindFlags.RenderTarget;
                if (mipMap)
                    description.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
            }

            _texture = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, description);
#elif PSM
			//TODO
#else
			this.glTarget = TextureTarget.TextureCubeMap;
#if IOS || ANDROID
			GL.GenTextures(1, ref this.glTexture);
#else
			GL.GenTextures(1, out this.glTexture);
#endif
            GraphicsExtensions.CheckGLError();
            GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
			                mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
			                (int)TextureMagFilter.Linear);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
			                (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
			                (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();


			format.GetGLFormat (out glInternalFormat, out glFormat, out glType);
			
			for (int i=0; i<6; i++) 
            {
				TextureTarget target = GetGLCubeFace((CubeMapFace)i);

				if (glFormat == (PixelFormat)All.CompressedTextureFormats) 
                {
					throw new NotImplementedException();
				} 
                else 
                {
#if IOS || ANDROID
					GL.TexImage2D (target, 0, (int)glInternalFormat, size, size, 0, glFormat, glType, IntPtr.Zero);
#else
					GL.TexImage2D (target, 0, glInternalFormat, size, size, 0, glFormat, glType, IntPtr.Zero);
#endif
                    GraphicsExtensions.CheckGLError();
                }
			}
			
			if (mipMap)
			{
#if IOS || ANDROID
				GL.GenerateMipmap(TextureTarget.TextureCubeMap);
#else
				GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)All.True);
#endif
                GraphicsExtensions.CheckGLError();
			}
#endif
        }

        /// <summary>
        /// Gets a copy of cube texture data specifying a cubemap face.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cubeMapFace">The cube map face.</param>
        /// <param name="data">The data.</param>
        public void GetData<T>(CubeMapFace cubeMapFace, T[] data) where T : struct
        {
            //FIXME Does not compile on Android or iOS
#if MONOMAC
            TextureTarget target = GetGLCubeFace(cubeMapFace);
            GL.BindTexture(target, this.glTexture);
            // 4 bytes per pixel
            if (data.Length < size * size * 4)
                throw new ArgumentException("data");

            GL.GetTexImage<T>(target, 0, PixelFormat.Bgra,
                PixelType.UnsignedByte, data);
#else
			throw new NotImplementedException();
#endif
        }

		public void SetData<T> (CubeMapFace face, T[] data) where T : struct
		{
            SetData(face, 0, null, data, 0, data.Length);
		}

        public void SetData<T>(CubeMapFace face, T[] data, int startIndex, int elementCount) where T : struct
		{
            SetData(face, 0, null, data, startIndex, elementCount);
		}
		
        public void SetData<T>(CubeMapFace face, int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		{
            if (data == null) 
                throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
			
            int xOffset, yOffset, width, height;
			if (rect.HasValue)
			{
				xOffset = rect.Value.X;
				yOffset = rect.Value.Y;
				width = rect.Value.Width;
				height = rect.Value.Height;
            }
            else
            {
                xOffset = 0;
                yOffset = 0;
                width = Math.Max(1, this.size >> level);
                height = Math.Max(1, this.size >> level);
            }

#if DIRECTX
            var box = new DataBox(dataPtr, GetPitch(width), 0);

            int subresourceIndex = (int)face * levelCount + level;

            var region = new ResourceRegion
            {
                Top = yOffset,
                Front = 0,
                Back = 1,
                Bottom = yOffset + height,
                Left = xOffset,
                Right = xOffset + width
            };

            var d3dContext = GraphicsDevice._d3dContext;
            lock (d3dContext)
                d3dContext.UpdateSubresource(box, _texture, subresourceIndex, region);
#elif PSM
			//TODO
#else
			GL.BindTexture (TextureTarget.TextureCubeMap, this.glTexture);
            GraphicsExtensions.CheckGLError();

			TextureTarget target = GetGLCubeFace(face);
			if (glFormat == (PixelFormat)All.CompressedTextureFormats) 
            {
				throw new NotImplementedException();
			} 
            else 
            {
				GL.TexSubImage2D(target, level, xOffset, yOffset, width, height, glFormat, glType, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
#endif
            dataHandle.Free ();
		}
		
#if OPENGL
		private TextureTarget GetGLCubeFace(CubeMapFace face) 
        {
			switch (face) 
            {
			case CubeMapFace.PositiveX: return TextureTarget.TextureCubeMapPositiveX;
			case CubeMapFace.NegativeX: return TextureTarget.TextureCubeMapNegativeX;
			case CubeMapFace.PositiveY: return TextureTarget.TextureCubeMapPositiveY;
			case CubeMapFace.NegativeY: return TextureTarget.TextureCubeMapNegativeY;
			case CubeMapFace.PositiveZ: return TextureTarget.TextureCubeMapPositiveZ;
			case CubeMapFace.NegativeZ: return TextureTarget.TextureCubeMapNegativeZ;
			}
			throw new ArgumentException();
		}
#endif

	}
}

