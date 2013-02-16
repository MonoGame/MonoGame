using System;
using System.Runtime.InteropServices;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#endif
#elif DIRECTX
// TODO!
#elif PSM
// TODO!
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class Texture3D : Texture
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Depth { get; private set; }
		
#if OPENGL
		PixelInternalFormat glInternalFormat;
		PixelFormat glFormat;
		PixelType glType;
#elif DIRECTX
#endif

		public Texture3D (GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format)
		{
			this.GraphicsDevice = graphicsDevice;
			Width = width;
			Height = height;
			Depth = depth;

#if OPENGL
			this.glTarget = TextureTarget.Texture3D;
            
            GL.GenTextures(1, out this.glTexture);
            GraphicsExtensions.CheckGLError();

			GL.BindTexture (glTarget, glTexture);
            GraphicsExtensions.CheckGLError();

			format.GetGLFormat (out glInternalFormat, out glFormat, out glType);

			GL.TexImage3D (glTarget, 0, glInternalFormat, width, height, depth, 0, glFormat, glType, IntPtr.Zero);
            GraphicsExtensions.CheckGLError();

			if (mipMap) {
					throw new NotImplementedException ();
			}
#elif DIRECTX

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

#if OPENGL
            GL.BindTexture(glTarget, glTexture);
            GraphicsExtensions.CheckGLError();
			GL.TexSubImage3D(glTarget, level, left, top, front, right-left, bottom-top, back-front, glFormat, glType, dataPtr);
            GraphicsExtensions.CheckGLError();
#elif DIRECTX

#endif
            dataHandle.Free ();
		}
		

	}
}

