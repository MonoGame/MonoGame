using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#else
using GL11 = OpenTK.Graphics.ES11.GL;
using All11 = OpenTK.Graphics.ES11.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class VertexBuffer : GraphicsResource
    {
        private readonly BufferUsage _bufferUsage;
        private readonly VertexDeclaration _vertexDeclaration;

        internal Type _type;
        internal object _buffer;
        internal IntPtr _bufferPtr;
        internal int _bufferIndex;
		internal int _size;		
		internal uint _bufferStore;

		// TODO: Remove this VB limit!
		internal uint vbo;
        internal static int _bufferCount;
        internal static VertexBuffer[] _allBuffers = new VertexBuffer[50];
		internal static List<Action> _delayedBufferDelegates = new List<Action>();

        public VertexBuffer(GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
        {
            graphicsDevice = graphics;
            _type = type;
			VertexCount = vertexCount;
            _bufferUsage = bufferUsage;
        }
		
		public VertexBuffer (GraphicsDevice graphics, VertexDeclaration vertexDecs, int vertexCount, BufferUsage bufferUsage)
			: this (graphics, vertexDecs.GetType(), vertexCount, bufferUsage)
		{
			_vertexDeclaration = vertexDecs;
		}
		
		public int VertexCount { get; private set; }

		public VertexDeclaration VertexDeclaration 
        {
			get 
            {
				return _vertexDeclaration;
			}
		}

		internal static void CreateFrameBuffers ()
		{
			foreach (var action in _delayedBufferDelegates)
				action.Invoke ();

			_delayedBufferDelegates.Clear ();
		}
		
		internal void GenerateBuffer<T>() where T : struct
		{
			var vd = VertexDeclaration.FromType(_type);
			
			_size = vd.VertexStride * ((T[])_buffer).Length;
			
			#if MONOMAC

			BufferUsageHint bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;

			GL.GenBuffers (1, out _bufferStore);
			GL.BindBuffer (BufferTarget.ArrayBuffer, _bufferStore);
			GL.BufferData (BufferTarget.ArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
			
			#else
			
            var bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? All11.StaticDraw : All11.DynamicDraw;
			
            GL11.GenBuffers(1, ref _bufferStore);
            GL11.BindBuffer(All11.ArrayBuffer, _bufferStore);
            GL11.BufferData(All11.ArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			

			#endif
		}

        public void GetData<T>(T[] vertices) where T : struct
        {
            if (_buffer == null)
                throw new Exception("Can't get data on an empty buffer");

            var tbuff = (T[])_buffer;
            for (var i = 0; i < tbuff.Length; i++)
                vertices[i] = tbuff[i];
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
   
			var elementSizeInBytes = Marshal.SizeOf(typeof(T));

			//Threading.BlockOnUIThread(() =>
            //{
                var sizeInBytes = elementSizeInBytes * elementCount;
#if MONOMAC
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferSubData<T>(BufferTarget.ArrayBuffer, new IntPtr(offsetInBytes), new IntPtr(sizeInBytes), data);
#else
				GL11.BindBuffer(All11.ArrayBuffer, vbo);
                GL11.BufferSubData<T>(All11.ArrayBuffer, new IntPtr(offsetInBytes), new IntPtr(sizeInBytes), data);
#endif

            //});
        }

		public override void Dispose ()
		{
			#if MONOMAC
			GL.GenBuffers (0, out _bufferStore);
			#else
			GL11.GenBuffers(0, ref _bufferStore);			
			#endif
			
            base.Dispose();
		}
	}
}
