using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{


    public class IndexBuffer : GraphicsResource
    {
        public BufferUsage BufferUsage { get; private set; }
        public int IndexCount { get; private set; }
        public IndexElementSize IndexElementSize { get; private set; }

        internal uint ibo;

        protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage, bool dynamic)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
            this.graphicsDevice = graphicsDevice;
            this.IndexElementSize = indexElementSize;
            this.IndexCount = indexCount;
            this.BufferUsage = bufferUsage;

            var sizeInBytes = indexCount * (this.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4);

#if IPHONE
			GL.GenBuffers(1, ref ibo);
#else
            GL.GenBuffers(1, out ibo);
#endif
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          (IntPtr)sizeInBytes, IntPtr.Zero, dynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) :
            this(graphicsDevice, indexElementSize, indexCount, bufferUsage, false)
        {
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
            this(graphicsDevice,
                 (indexType == typeof(short) || indexType == typeof(ushort)) ? IndexElementSize.SixteenBits : IndexElementSize.ThirtyTwoBits,
                 indexCount, usage)
        {
        }

        internal void Apply()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
        }

        public unsafe void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, ibo);
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            IntPtr ptr = new IntPtr();
            ptr = GL.Arb.MapBuffer(BufferTargetArb.ArrayBuffer, ArbVertexBufferObject.ReadOnlyArb | ArbVertexBufferObject.ArrayBufferArb);

            if (ptr != null && ptr.ToInt32() != 0)
            {
                byte* byt = (byte*)ptr.ToPointer();
                byt = byt + offsetInBytes;

                for (int j = 0; j < elementCount; j++)
                {
                    byte[] bytes = new byte[elementSizeInByte];
                    for (int i = 0; i < elementSizeInByte; i++)
                    {
                        bytes[i] = *byt;
                        byt++;
                    }

                    IntPtr buffer = Marshal.AllocHGlobal(elementSizeInByte);
                    Marshal.Copy(bytes, 0, buffer, elementSizeInByte);
                    object retobj = Marshal.PtrToStructure(buffer, typeof(T));
                    data[j] = (T)retobj;
                    Marshal.Release(buffer);
                }

                GL.Arb.UnmapBuffer(BufferTargetArb.ArrayBuffer);
                Marshal.Release(ptr);
            }
        }

        public unsafe void GetData<T>(T[] data, int startIndex) where T : struct
        {
            this.GetData<T>(0, data, startIndex, data.Count());
        }

        public unsafe void GetData<T>(T[] data) where T : struct
        {
            this.GetData<T>(0, data, 0, data.Count());
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInByte * elementCount;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);

            dataHandle.Free();
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInByte * elementCount;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, (IntPtr)sizeInBytes, dataPtr);

            dataHandle.Free();
        }

        public void SetData<T>(T[] data) where T : struct
        {
            if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var sizeInBytes = elementSizeInByte * data.Length;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var dataPtr = dataHandle.AddrOfPinnedObject();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, (IntPtr)sizeInBytes, dataPtr);

            dataHandle.Free();
        }

        public override void Dispose()
        {
            GL.DeleteBuffers(1, ref ibo);
            base.Dispose();
        }
    }
}
