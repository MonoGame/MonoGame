using System;
using System.Runtime.InteropServices;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#endif
#elif DIRECTX
using SharpDX;
using SharpDX.Direct3D11;
#elif PSM
// TODO!
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class Texture3D : Texture
	{
        private int width;
        private int height;
        private int depth;
		
#if OPENGL
		PixelInternalFormat glInternalFormat;
		PixelFormat glFormat;
		PixelType glType;
#endif

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public int Depth
        {
            get { return depth; }
        }

		public Texture3D (GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

			this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.levelCount = 1;

#if OPENGL
			this.glTarget = TextureTarget.Texture3D;
            
            GL.GenTextures(1, out this.glTexture);
            GraphicsExtensions.CheckGLError();

			GL.BindTexture (glTarget, glTexture);
            GraphicsExtensions.CheckGLError();

			format.GetGLFormat (out glInternalFormat, out glFormat, out glType);

			GL.TexImage3D (glTarget, 0, glInternalFormat, width, height, depth, 0, glFormat, glType, IntPtr.Zero);
            GraphicsExtensions.CheckGLError();

			if (mipMap) 
                throw new NotImplementedException("Texture3D does not yet support mipmaps.");
#elif DIRECTX
            if (mipMap)
                this.levelCount = CalculateMipLevels(width, height, depth);

            var description = new Texture3DDescription
            {
                Width = width,
                Height = height,
                Depth = depth,
                MipLevels = levelCount,
                Format = SharpDXHelper.ToFormat(format),
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
            };

            _texture = new SharpDX.Direct3D11.Texture3D(graphicsDevice._d3dDevice, description);
#endif
        }
		
		public void SetData<T> (T[] data) where T : struct
		{
			SetData<T>(data, 0, data.Length);
		}
		
		public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct
		{
			SetData<T>(0, 0, 0, Width, Height, 0, Depth, data, startIndex, elementCount);
		}
		
		public void SetData<T> (int level,
		                        int left, int top, int right, int bottom, int front, int back,
		                        T[] data, int startIndex, int elementCount) where T : struct
		{
			if (data == null) 
				throw new ArgumentNullException("data");

			var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
            int width = right - left;
            int height = bottom - top;
            int depth = back - front;

#if OPENGL
            GL.BindTexture(glTarget, glTexture);
            GraphicsExtensions.CheckGLError();
			GL.TexSubImage3D(glTarget, level, left, top, front, width, height, depth, glFormat, glType, dataPtr);
            GraphicsExtensions.CheckGLError();
#elif DIRECTX
            int rowPitch = GetPitch(width);
            int slicePitch = rowPitch * height; // For 3D texture: Size of 2D image.
            var box = new DataBox(dataPtr, rowPitch, slicePitch);

            int subresourceIndex = level;

            var region = new ResourceRegion(left, top, front, right, bottom, back);

            var d3dContext = GraphicsDevice._d3dContext;
            lock (d3dContext)
                d3dContext.UpdateSubresource(box, _texture, subresourceIndex, region);
#endif
            dataHandle.Free ();
		}
	}
}

