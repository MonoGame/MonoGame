using System;
using System.Runtime.InteropServices;


#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class Texture3D : Texture
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Depth { get; private set; }
		
		PixelInternalFormat glInternalFormat;
		PixelFormat glFormat;
		PixelType glType;
		
		public Texture3D (GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
		{
			this.GraphicsDevice = graphicsDevice;
			Width = width;
			Height = height;
			Depth = depth;

			this.glTarget = TextureTarget.Texture3D;

			GL.GenTextures (1, out this.glTexture);
			GL.BindTexture (glTarget, glTexture);

			format.GetGLFormat (out glInternalFormat, out glFormat, out glType);

			GL.TexImage3D (glTarget, 0, glInternalFormat, width, height, depth, 0, glFormat, glType, IntPtr.Zero);

			if (mipMap) {
					throw new NotImplementedException ();
			}
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

            GL.BindTexture(glTarget, glTexture);
			GL.TexSubImage3D(glTarget, level, left, top, front, right-left, bottom-top, back-front, glFormat, glType, dataPtr);
			
			dataHandle.Free ();
		}
		

	}
}

