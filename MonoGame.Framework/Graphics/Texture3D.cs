using System;
using System.IO;
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

#if DIRECTX
        private bool renderTarget;
        private bool mipMap;
#endif
		
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

		public Texture3D(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
            : this(graphicsDevice, width, height, depth, mipMap, format, false)
		{		    
		}

		protected Texture3D (GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

			this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this.depth = depth;
            this._levelCount = 1;
		    this._format = format;

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
            this.renderTarget = renderTarget;
            this.mipMap = mipMap;

            if (mipMap)
                this._levelCount = CalculateMipLevels(width, height, depth);

            // Create texture
            GetTexture();
#endif
        }

#if DIRECTX

        internal override SharpDX.Direct3D11.Resource CreateTexture()
        {
            var description = new Texture3DDescription
            {
                Width = width,
                Height = height,
                Depth = depth,
                MipLevels = _levelCount,
                Format = SharpDXHelper.ToFormat(_format),
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
            };

            if (renderTarget)
            {
                description.BindFlags |= BindFlags.RenderTarget;
                if (mipMap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    description.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
                }
            }

            return new SharpDX.Direct3D11.Texture3D(GraphicsDevice._d3dDevice, description);
        }

#endif
        
        public void SetData<T>(T[] data) where T : struct
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
                d3dContext.UpdateSubresource(box, GetTexture(), subresourceIndex, region);
#endif
            dataHandle.Free ();
		}

        /// <summary>
        /// Gets a copy of 3D texture data, specifying a mipmap level, source box, start index, and number of elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="level">Mipmap level.</param>
        /// <param name="left">Position of the left side of the box on the x-axis.</param>
        /// <param name="top">Position of the top of the box on the y-axis.</param>
        /// <param name="right">Position of the right side of the box on the x-axis.</param>
        /// <param name="bottom">Position of the bottom of the box on the y-axis.</param>
        /// <param name="front">Position of the front of the box on the z-axis.</param>
        /// <param name="back">Position of the back of the box on the z-axis.</param>
        /// <param name="data">Array of data.</param>
        /// <param name="startIndex">Index of the first element to get.</param>
        /// <param name="elementCount">Number of elements to get.</param>
        public void GetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("data cannot be null");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");

            // Disallow negative box size
            if ((left < 0 || left >= right)
                || (top < 0 || top >= bottom)
                || (front < 0 || front >= back))
                throw new ArgumentException("Neither box size nor box position can be negative");
#if IOS

            // Reading back a texture from GPU memory is unsupported
            // in OpenGL ES 2.0 and no work around has been implemented.           
            throw new NotSupportedException("OpenGL ES 2.0 does not support texture reads.");

#elif ANDROID

            throw new NotImplementedException();

#elif PSM

            throw new NotImplementedException();

#elif DIRECTX

            // Create a temp staging resource for copying the data.
            // 
            // TODO: Like in Texture2D, we should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var desc = new Texture3DDescription
            {
                Width = width,
                Height = height,
                Depth = depth,
                MipLevels = 1,
                Format = SharpDXHelper.ToFormat(_format),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                OptionFlags = ResourceOptionFlags.None,
            };

            var d3dContext = GraphicsDevice._d3dContext;
            using (var stagingTex = new SharpDX.Direct3D11.Texture3D(GraphicsDevice._d3dDevice, desc))
            {
                lock (d3dContext)
                {
                    // Copy the data from the GPU to the staging texture.
                    d3dContext.CopySubresourceRegion(GetTexture(), level, new ResourceRegion(left, top, front, right, bottom, back), stagingTex, 0);

                    // Copy the data to the array.
                    DataStream stream;
                    var databox = d3dContext.MapSubresource(stagingTex, 0, MapMode.Read, MapFlags.None, out stream);

                    // Some drivers may add pitch to rows or slices.
                    // We need to copy each row separatly and skip trailing zeros.
                    var currentIndex = startIndex;
                    var elementSize = SharpDX.Utilities.SizeOf<T>();
                    var elementsInRow = right - left;
                    var rowsInSlice = bottom - top;
                    for (var slice = front; slice < back; slice++)
                    {
                        for (var row = top; row < bottom; row++)
                        {
                            stream.ReadRange(data, currentIndex, elementsInRow);
                            stream.Seek(databox.RowPitch - (elementSize * elementsInRow), SeekOrigin.Current);
                            currentIndex += elementsInRow;
                        }
                        stream.Seek(databox.SlicePitch - (databox.RowPitch * rowsInSlice), SeekOrigin.Current);
                    }
                    stream.Dispose();
                }
            }

#else

            throw new NotImplementedException();

#endif
        }

        /// <summary>
        /// Gets a copy of 3D texture data, specifying a start index and number of elements.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">Array of data.</param>
        /// <param name="startIndex">Index of the first element to get.</param>
        /// <param name="elementCount">Number of elements to get.</param>
        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            GetData(0, 0, 0, width, height, 0, depth, data, startIndex, elementCount);
        }

        /// <summary>
        /// Gets a copy of 3D texture data.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="data">Array of data.</param>
        public void GetData<T>(T[] data) where T : struct
        {
            GetData(data, 0, data.Length);
        }
	}
}

