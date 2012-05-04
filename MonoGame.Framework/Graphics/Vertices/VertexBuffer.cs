using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSS
using Sce.Pss.Core.Graphics;
#elif GLES
using OpenTK.Graphics.ES20;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexBuffer : GraphicsResource
    {
#if DIRECTX
        internal SharpDX.Direct3D11.VertexBufferBinding _binding;
        private SharpDX.Direct3D11.Buffer _buffer;
#elif PSS
        private PssVertexBuffer _buffer;
#else
		//internal uint vao;
		internal uint vbo;
#endif
	
		public int VertexCount { get; private set; }
		public VertexDeclaration VertexDeclaration { get; private set; }
		public BufferUsage BufferUsage { get; private set; }
		
		protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
		{
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
			this.graphicsDevice = graphicsDevice;
            this.VertexDeclaration = vertexDeclaration;
            this.VertexCount = vertexCount;
            this.BufferUsage = bufferUsage;

#if DIRECTX
            // TODO: To use Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            _buffer = new SharpDX.Direct3D11.Buffer(    graphicsDevice._d3dDevice,
                                                        vertexDeclaration.VertexStride * vertexCount,
                                                        dynamic ? SharpDX.Direct3D11.ResourceUsage.Dynamic : SharpDX.Direct3D11.ResourceUsage.Default,
                                                        SharpDX.Direct3D11.BindFlags.VertexBuffer,
                                                        SharpDX.Direct3D11.CpuAccessFlags.None,
                                                        0, // OptionFlags                                                          
                                                        0  // StructureSizeInBytes
                                                        );

            _binding = new SharpDX.Direct3D11.VertexBufferBinding(_buffer, VertexDeclaration.VertexStride, 0);
#elif PSS
            VertexFormat[] vertexFormat = new VertexFormat[vertexDeclaration._elements.Length];
            for (int i = 0; i < vertexFormat.Length; i++)
                vertexFormat[i] = PSSHelper.ToVertexFormat(vertexDeclaration._elements[i].VertexElementFormat);
            _buffer = new PssVertexBuffer(vertexCount, 0, vertexFormat);
#else
            Threading.Begin();
            try
            {
                //GLExt.Oes.GenVertexArrays(1, out this.vao);
                //GLExt.Oes.BindVertexArray(this.vao);
#if IPHONE || ANDROID
                GL.GenBuffers(1, ref this.vbo);
#else
			    GL.GenBuffers(1, out this.vbo);
#endif
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
                GL.BufferData(BufferTarget.ArrayBuffer,
                              new IntPtr(vertexDeclaration.VertexStride * vertexCount), IntPtr.Zero,
                              dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
            }
            finally
            {
                Threading.End();
            }
#endif
		}

        public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
        {
        }
		
		public VertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, false)
		{
        }

        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This VertexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
            if ((vertexStride > (VertexCount * VertexDeclaration.VertexStride)) || (vertexStride < VertexDeclaration.VertexStride))
                throw new ArgumentOutOfRangeException("One of the following conditions is true:\nThe vertex stride is larger than the vertex buffer.\nThe vertex stride is too small for the type of data requested.");

#if DIRECTX
            throw new NotImplementedException();
#elif PSS
            throw new NotImplementedException();
#else
            Threading.Begin();
            try
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
#if IPHONE || ANDROID
                // I think the access parameter takes zero for read only or read/write.
                // The glMapBufferOES extension spec and gl2ext.h both only mention GL_WRITE_ONLY
                IntPtr ptr = GL.Oes.MapBuffer(All.ArrayBuffer, (All)0);
#else
                IntPtr ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
#endif
                // Pointer to the start of data to read in the index buffer
                ptr = new IntPtr(ptr.ToInt64() + offsetInBytes);
                if (data is byte[])
                {
                    byte[] buffer = data as byte[];
                    // If data is already a byte[] we can skip the temporary buffer
                    // Copy from the vertex buffer to the destination array
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
                }
                else
                {
                    // Temporary buffer to store the copied section of data
                    byte[] buffer = new byte[elementCount * vertexStride];
                    // Copy from the vertex buffer to the temporary buffer
                    Marshal.Copy(ptr, buffer, 0, buffer.Length);
                    // Copy from the temporary buffer to the destination array
                    Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
                }
#if IPHONE || ANDROID
                GL.Oes.UnmapBuffer(All.ArrayBuffer);
#else
                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
#endif
            }
            finally
            {
                Threading.End();
            }
#endif
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            this.GetData<T>(0, data, startIndex, elementCount, elementSizeInByte);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            this.GetData<T>(0, data, 0, data.Count(), elementSizeInByte);
        }
		
		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if ((vertexStride > (VertexCount * VertexDeclaration.VertexStride)) || (vertexStride < VertexDeclaration.VertexStride))
                throw new ArgumentOutOfRangeException("One of the following conditions is true:\nThe vertex stride is larger than the vertex buffer.\nThe vertex stride is too small for the type of data requested.");

            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInBytes * elementCount;

#if DIRECTX

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var startBytes = startIndex * elementSizeInBytes;
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

            var box = new SharpDX.DataBox(dataPtr, elementSizeInBytes, 0);

            var region = new SharpDX.Direct3D11.ResourceRegion();
            region.Top = 0;
            region.Front = 0;
            region.Back = 1;
            region.Bottom = 1;
            region.Left = offsetInBytes;
            region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

            // TODO: We need to deal with threaded contexts here!
            graphicsDevice._d3dContext.UpdateSubresource(box, _buffer, 0, region);

            dataHandle.Free();

#elif PSS
#warning This is almost 100% certainly wrong
            _buffer.SetVertices(data, startIndex, offsetInBytes / elementSizeInByte, vertexStride);
#else
            Threading.Begin();
            try
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferSubData<T>(BufferTarget.ArrayBuffer, new IntPtr(offsetInBytes), new IntPtr(sizeInBytes), data);
            }
            finally
            {
                Threading.End();
            }
#endif
		}
		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetData<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetData<T>(0, data, 0, data.Length, VertexDeclaration.VertexStride);
        }
		
		public override void Dispose()
        {
#if DIRECTX || PSS
            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }
#else
			GL.DeleteBuffers(1, ref vbo);
#endif
            base.Dispose();
		}
    }
}
