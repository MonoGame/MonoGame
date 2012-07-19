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
using PssVertexBuffer = Sce.Pss.Core.Graphics.VertexBuffer;
#elif GLES
using OpenTK.Graphics.ES20;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexBuffer : GraphicsResource
    {
        protected bool _isDynamic;

#if DIRECTX
        internal SharpDX.Direct3D11.VertexBufferBinding _binding;
        protected SharpDX.Direct3D11.Buffer _buffer;
#elif PSS
        internal PssVertexBuffer _buffer;
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
                throw new ArgumentNullException("Graphics Device Cannot Be Null");

            this.graphicsDevice = graphicsDevice;
            this.VertexDeclaration = vertexDeclaration;
            this.VertexCount = vertexCount;
            this.BufferUsage = bufferUsage;

            _isDynamic = dynamic;

#if DIRECTX
            // TODO: To use Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            SharpDX.Direct3D11.CpuAccessFlags accessflags = SharpDX.Direct3D11.CpuAccessFlags.None;
            SharpDX.Direct3D11.ResourceUsage usage = SharpDX.Direct3D11.ResourceUsage.Default;

            if (dynamic)
            {
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Write;
                usage = SharpDX.Direct3D11.ResourceUsage.Dynamic;
            }

            _buffer = new SharpDX.Direct3D11.Buffer(    graphicsDevice._d3dDevice, 
                                                        vertexDeclaration.VertexStride * vertexCount,
                                                        usage,
                                                        SharpDX.Direct3D11.BindFlags.VertexBuffer,
                                                        accessflags,
                                                        SharpDX.Direct3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );

            _binding = new SharpDX.Direct3D11.VertexBufferBinding(_buffer, VertexDeclaration.VertexStride, 0);
#elif PSS
            VertexFormat[] vertexFormat = vertexDeclaration.GetVertexFormat();
            _buffer = new PssVertexBuffer(vertexCount, vertexFormat);
#else
            Threading.BlockOnUIThread(() =>
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
            });
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
            Threading.BlockOnUIThread(() =>
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
            });
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
            SetData<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride, SetDataOptions.Discard);
        }
        		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetData<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride, SetDataOptions.Discard);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetData<T>(0, data, 0, data.Length, VertexDeclaration.VertexStride, SetDataOptions.Discard);
        }

        protected void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if ((vertexStride > (VertexCount * VertexDeclaration.VertexStride)) || (vertexStride < VertexDeclaration.VertexStride))
                throw new ArgumentOutOfRangeException("One of the following conditions is true:\nThe vertex stride is larger than the vertex buffer.\nThe vertex stride is too small for the type of data requested.");
   
#if !PSS
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
#endif

#if DIRECTX

            if (_isDynamic)
            {
                // We assume discard by default.
                var mode = SharpDX.Direct3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = SharpDX.Direct3D11.MapMode.WriteNoOverwrite;

                SharpDX.DataStream stream;
                lock (graphicsDevice._d3dContext)
                {
                    graphicsDevice._d3dContext.MapSubresource(
                        _buffer,
                        mode,
                        SharpDX.Direct3D11.MapFlags.None,
                        out stream);

                    stream.Position = offsetInBytes;
                    stream.WriteRange(data, startIndex, elementCount);

                    graphicsDevice._d3dContext.UnmapSubresource(_buffer, 0);
                }
            }
            else
            {
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var startBytes = startIndex * elementSizeInBytes;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

                var box = new SharpDX.DataBox(dataPtr, 1, 0);

                var region = new SharpDX.Direct3D11.ResourceRegion();
                region.Top = 0;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = 1;
                region.Left = offsetInBytes;
                region.Right = offsetInBytes + (elementCount * elementSizeInBytes);

                lock (graphicsDevice._d3dContext)
                    graphicsDevice._d3dContext.UpdateSubresource(box, _buffer, 0, region);

                dataHandle.Free();
            }

#elif PSS
            _buffer.SetVertices(data, offsetInBytes, startIndex, elementCount);
#else
            Threading.BlockOnUIThread(() =>
            {
                var sizeInBytes = elementSizeInBytes * elementCount;
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferSubData<T>(BufferTarget.ArrayBuffer, new IntPtr(offsetInBytes), new IntPtr(sizeInBytes), data);
            });
#endif
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
