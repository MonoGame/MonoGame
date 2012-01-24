using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GL11 = OpenTK.Graphics.ES11.GL;
using All11 = OpenTK.Graphics.ES11.All;

namespace Microsoft.Xna.Framework.Graphics
{
    public class VertexBuffer : GraphicsResource
    {
        internal Type _type;
        private int _vertexCount;
        private BufferUsage _bufferUsage;
        internal object _buffer = null;
        internal IntPtr _bufferPtr;
        internal int _bufferIndex = 0;
		internal int _size;		
        internal static int _bufferCount = 0;
		internal uint _bufferStore; 

		// allow for 50 buffers initially
		internal static VertexBuffer[] _allBuffers = new VertexBuffer[50];
		internal static List<Action> _delayedBufferDelegates = new List<Action>();

        public VertexBuffer(GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
        {
            this.graphicsDevice = graphics;
            this._type = type;
            this._vertexCount = vertexCount;
            this._bufferUsage = bufferUsage;
        }
        
		public int VertexCount { get; set; }
		
		internal static void CreateFrameBuffers()
		{
			foreach (var action in _delayedBufferDelegates)
				action.Invoke();
			
			_delayedBufferDelegates.Clear();
		}
		
		internal void GenerateBuffer<T>() where T : struct, IVertexType
		{
			var vd = VertexDeclaration.FromType(_type);
			
			_size = vd.VertexStride * ((T[])_buffer).Length;
			
            All11 bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? All11.StaticDraw : All11.DynamicDraw;
			
            GL11.GenBuffers(1, ref _bufferStore);
            GL11.BindBuffer(All11.ArrayBuffer, _bufferStore);
            GL11.BufferData<T>(All11.ArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
		}
		
        public void GetData<T>(T[] vertices) where T : IVertexType
        {
            if (_buffer == null)
                throw new Exception("Can't get data on an empty buffer");

            var _tbuff = (T[])_buffer;
            for (int i = 0; i < _tbuff.Length; i++)
                vertices[i] = _tbuff[i];
        }

        public void SetData<T>(T[] vertices) where T : struct, IVertexType
        {
			//the creation of the buffer should mb be moved to the constructor and then glMapBuffer and Unmap should be used to update it
			//glMapBuffer - sets data
			//glUnmapBuffer - finished setting data
			
            _buffer = vertices;
            _bufferPtr = GCHandle.Alloc(_buffer, GCHandleType.Pinned).AddrOfPinnedObject();			
			
			_bufferIndex = _bufferCount + 1;
			_allBuffers[_bufferIndex] = this;
			
			_delayedBufferDelegates.Add(GenerateBuffer<T>);
			
            _bufferCount++;
            // TODO: Kill buffers in PhoneOSGameView.DestroyFrameBuffer()
        }

		public override void Dispose ()
		{
			GL11.GenBuffers(0, ref _bufferStore);
            base.Dispose();
		}
    }
	
    public class DynamicVertexBuffer : VertexBuffer
    {
        public DynamicVertexBuffer(GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
            : base(graphics, type, vertexCount, bufferUsage)
        {
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct, IVertexType
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct, IVertexType
        {
            throw new NotImplementedException();
        }
    }
}
