using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#endif
#elif PSM
using Sce.PlayStation.Core.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexBuffer : GraphicsResource
    {
        internal bool _isDynamic;

#if DIRECTX
        private SharpDX.Direct3D11.VertexBufferBinding _binding;
        private SharpDX.Direct3D11.Buffer _buffer;

        internal SharpDX.Direct3D11.VertexBufferBinding Binding
        {
            get
            {
                GenerateIfRequired();
                return _binding;
            }
        }

#elif PSM
        internal Array _vertexArray;
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
                throw new ArgumentNullException("Graphics Device Cannot Be null");

            this.GraphicsDevice = graphicsDevice;
            this.VertexDeclaration = vertexDeclaration;
            this.VertexCount = vertexCount;
            this.BufferUsage = bufferUsage;

            // Make sure the graphics device is assigned in the vertex declaration.
            if (vertexDeclaration.GraphicsDevice != graphicsDevice)
                vertexDeclaration.GraphicsDevice = graphicsDevice;

            _isDynamic = dynamic;

#if DIRECTX

            GenerateIfRequired();

#elif PSM
            //Do nothing, we cannot create the storage array yet
#else
            Threading.BlockOnUIThread(GenerateIfRequired);
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

        /// <summary>
        /// The GraphicsDevice is resetting, so GPU resources must be recreated.
        /// </summary>
        internal protected override void GraphicsDeviceResetting()
        {
#if OPENGL
            vbo = 0;
#endif

#if DIRECTX

            _binding = new SharpDX.Direct3D11.VertexBufferBinding();
            SharpDX.Utilities.Dispose(ref _buffer);

#endif
        }

#if OPENGL
        /// <summary>
        /// If the VBO does not exist, create it.
        /// </summary>
        void GenerateIfRequired()
        {
            if (vbo == 0)
            {
                //GLExt.Oes.GenVertexArrays(1, out this.vao);
                //GLExt.Oes.BindVertexArray(this.vao);
#if IOS || ANDROID
                GL.GenBuffers(1, ref this.vbo);
#else
                GL.GenBuffers(1, out this.vbo);
#endif
                GraphicsExtensions.CheckGLError();
                GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo);
                GraphicsExtensions.CheckGLError();
                GL.BufferData(BufferTarget.ArrayBuffer,
                              new IntPtr(VertexDeclaration.VertexStride * VertexCount), IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }
        }
#endif

#if DIRECTX

        void GenerateIfRequired()
        {
            if (_buffer != null)
                return;

            // TODO: To use Immutable resources we would need to delay creation of 
            // the Buffer until SetData() and recreate them if set more than once.

            var accessflags = SharpDX.Direct3D11.CpuAccessFlags.None;
            var usage = SharpDX.Direct3D11.ResourceUsage.Default;

            if (_isDynamic)
            {
                accessflags |= SharpDX.Direct3D11.CpuAccessFlags.Write;
                usage = SharpDX.Direct3D11.ResourceUsage.Dynamic;
            }

            _buffer = new SharpDX.Direct3D11.Buffer(GraphicsDevice._d3dDevice,
                                                        VertexDeclaration.VertexStride * VertexCount,
                                                        usage,
                                                        SharpDX.Direct3D11.BindFlags.VertexBuffer,
                                                        accessflags,
                                                        SharpDX.Direct3D11.ResourceOptionFlags.None,
                                                        0  // StructureSizeInBytes
                                                        );

            _binding = new SharpDX.Direct3D11.VertexBufferBinding(_buffer, VertexDeclaration.VertexStride, 0);
        }

#endif

        public void GetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
#if GLES
            // Buffers are write-only on OpenGL ES 1.1 and 2.0.  See the GL_OES_mapbuffer extension for more information.
            // http://www.khronos.org/registry/gles/extensions/OES/OES_mapbuffer.txt
            throw new NotSupportedException("Vertex buffers are write-only on OpenGL ES platforms");
#else
            if (data == null)
                throw new ArgumentNullException("data", "This method does not accept null for this parameter.");
            if (data.Length < (startIndex + elementCount))
                throw new ArgumentOutOfRangeException("elementCount", "This parameter must be a valid index within the array.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
			if ((elementCount * vertexStride) > (VertexCount * VertexDeclaration.VertexStride))
                throw new InvalidOperationException("The array is not the correct size for the amount of data requested.");

#if DIRECTX

            GenerateIfRequired();

            if (_isDynamic)
            {
                throw new NotImplementedException();
            }
            else
            {
                var deviceContext = GraphicsDevice._d3dContext;
                
                // Copy the buffer to a staging resource
                var stagingDesc = _buffer.Description;
                stagingDesc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
                stagingDesc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read | SharpDX.Direct3D11.CpuAccessFlags.Write;
                stagingDesc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
                stagingDesc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
                var stagingBuffer = new SharpDX.Direct3D11.Buffer(GraphicsDevice._d3dDevice, stagingDesc);
                
                lock (GraphicsDevice._d3dContext)
                    deviceContext.CopyResource(_buffer, stagingBuffer);

                int TsizeInBytes = SharpDX.Utilities.SizeOf<T>();
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var startBytes = startIndex * vertexStride;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                SharpDX.DataPointer DataPointer = new SharpDX.DataPointer(dataPtr, data.Length * TsizeInBytes);

                lock (GraphicsDevice._d3dContext)
                {
                    // Map the staging resource to a CPU accessible memory
                    var box = deviceContext.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                    if (vertexStride == TsizeInBytes)
                    {
                        SharpDX.Utilities.CopyMemory(dataPtr, box.DataPointer + offsetInBytes, vertexStride * elementCount);
                    }
                    else
                    {
                        for (int i = 0; i < data.Length; i++)
                            SharpDX.Utilities.CopyMemory(dataPtr + i * TsizeInBytes, box.DataPointer + i * vertexStride + offsetInBytes, TsizeInBytes);
                    }

                    // Make sure that we unmap the resource in case of an exception
                    deviceContext.UnmapSubresource(stagingBuffer, 0);
                }
                stagingBuffer.Dispose();
            }
#elif PSM
            throw new NotImplementedException();
#else

            if (Threading.IsOnUIThread())
            {
                GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride);
            }
            else
            {
                Threading.BlockOnUIThread (() => GetBufferData(offsetInBytes, data, startIndex, elementCount, vertexStride));
            }

#endif
#endif
        }

#if OPENGL && !GLES

        private void GetBufferData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            GL.BindBuffer (BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            IntPtr ptr = GL.MapBuffer (BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
            GraphicsExtensions.CheckGLError();
            // Pointer to the start of data to read in the index buffer
            ptr = new IntPtr (ptr.ToInt64 () + offsetInBytes);
            if (data is byte[]) {
                byte[] buffer = data as byte[];
                // If data is already a byte[] we can skip the temporary buffer
                // Copy from the vertex buffer to the destination array
                Marshal.Copy (ptr, buffer, 0, buffer.Length);
            } else {
                // Temporary buffer to store the copied section of data
                byte[] buffer = new byte[elementCount * vertexStride - offsetInBytes];
                // Copy from the vertex buffer to the temporary buffer
                Marshal.Copy(ptr, buffer, 0, buffer.Length);
                
                var dataHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject ().ToInt64 () + startIndex * elementSizeInByte);
                
                // Copy from the temporary buffer to the destination array
                
                int dataSize = Marshal.SizeOf(typeof(T));
                if (dataSize == vertexStride)
                    Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
                else
                {
                    // If the user is asking for a specific element within the vertex buffer, copy them one by one...
                    for (int i = 0; i < elementCount; i++)
                    {
                        Marshal.Copy(buffer, i * vertexStride, dataPtr, dataSize);
                        dataPtr = (IntPtr)(dataPtr.ToInt64() + dataSize);
                    }
                }
                
                dataHandle.Free ();
                
                //Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            }
        
#endif
        
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
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, VertexDeclaration.VertexStride, SetDataOptions.None);
        }
        		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(0, data, startIndex, elementCount, VertexDeclaration.VertexStride, SetDataOptions.None);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetDataInternal<T>(0, data, 0, data.Length, VertexDeclaration.VertexStride, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data is null");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");

            var bufferSize = VertexCount * VertexDeclaration.VertexStride;
            if ((vertexStride > bufferSize) || (vertexStride < VertexDeclaration.VertexStride))
                throw new ArgumentOutOfRangeException("One of the following conditions is true:\nThe vertex stride is larger than the vertex buffer.\nThe vertex stride is too small for the type of data requested.");
   
#if !PSM
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
#endif

#if DIRECTX

            GenerateIfRequired();

            if (_isDynamic)
            {
                // We assume discard by default.
                var mode = SharpDX.Direct3D11.MapMode.WriteDiscard;
                if ((options & SetDataOptions.NoOverwrite) == SetDataOptions.NoOverwrite)
                    mode = SharpDX.Direct3D11.MapMode.WriteNoOverwrite;

                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                {
                    var dataBox = d3dContext.MapSubresource(_buffer, 0, mode, SharpDX.Direct3D11.MapFlags.None);
                    SharpDX.Utilities.Write(IntPtr.Add(dataBox.DataPointer, offsetInBytes), data, startIndex,
                                            elementCount);
                    d3dContext.UnmapSubresource(_buffer, 0);
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

                lock (GraphicsDevice._d3dContext)
                    GraphicsDevice._d3dContext.UpdateSubresource(box, _buffer, 0, region);

                dataHandle.Free();
            }

#elif PSM
            if (_vertexArray == null)
                _vertexArray = new T[VertexCount];
            Array.Copy(data, offsetInBytes / vertexStride, _vertexArray, startIndex, elementCount);
#else

            if (Threading.IsOnUIThread())
            {
                SetBufferData(bufferSize, elementSizeInBytes, offsetInBytes, data, startIndex, elementCount, vertexStride, options);
            }
            else
            {
                Threading.BlockOnUIThread(() => SetBufferData(bufferSize, elementSizeInBytes, offsetInBytes, data, startIndex, elementCount, vertexStride, options));
            }
#endif
        }

#if OPENGL

        private void SetBufferData<T>(int bufferSize, int elementSizeInBytes, int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            GenerateIfRequired();
            
            var sizeInBytes = elementSizeInBytes * elementCount;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GraphicsExtensions.CheckGLError();
            
            if (options == SetDataOptions.Discard)
            {
                // By assigning NULL data to the buffer this gives a hint
                // to the device to discard the previous content.
                GL.BufferData(  BufferTarget.ArrayBuffer,
                              (IntPtr)bufferSize, 
                              IntPtr.Zero,
                              _isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
                GraphicsExtensions.CheckGLError();
            }

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInBytes);

            GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
            GraphicsExtensions.CheckGLError();

            dataHandle.Free();
        }
        
#endif
        
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
#if DIRECTX
                if (disposing)
                {
                    if (_buffer != null)
                    {
                        _buffer.Dispose();
                        _buffer = null;
                    }
                }
#elif PSM
                //Do nothing
                _vertexArray = null;
#else
                GraphicsDevice.AddDisposeAction(() =>
                    {
                        GL.DeleteBuffers(1, ref vbo);
                        GraphicsExtensions.CheckGLError();
                    });
#endif
            }
            base.Dispose(disposing);
		}
    }
}
