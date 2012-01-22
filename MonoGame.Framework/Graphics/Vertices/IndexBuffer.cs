using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    public class IndexBuffer : GraphicsResource
    {
        internal Type _type;
        internal int _count;
		private object _buffer;
		internal IntPtr _bufferPtr;
		internal IntPtr _sizePtr;
        private BufferUsage _bufferUsage;
		internal static IndexBuffer[] _allBuffers = new IndexBuffer[50];
		internal static int _bufferCount = 0;
		internal int _bufferIndex;
		internal int _size;
		internal uint _bufferStore;
		internal static List<Action> _delayedBufferDelegates = new List<Action>(); 
		
		internal static void CreateFrameBuffers()
		{
			foreach (var action in _delayedBufferDelegates)
				action.Invoke();
			
			_delayedBufferDelegates.Clear();
		}		
		
        public IndexBuffer(GraphicsDevice graphics, Type type, int count, BufferUsage bufferUsage)
        {
			if (type != typeof(uint) && type != typeof(ushort) && type != typeof(byte))
				throw new NotSupportedException("The only types that are supported are: uint, ushort and byte");
			
            this.graphicsDevice = graphics;
            this._type = type;
            this._count = count;
            this._bufferUsage = bufferUsage;
        }
        
		internal void GenerateBuffer<T>() where T : struct
		{
            All11 bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? All11.StaticDraw : All11.DynamicDraw;
            GL11.GenBuffers(1, ref _bufferStore);
            GL11.BindBuffer(All11.ElementArrayBuffer, _bufferStore);
            GL11.BufferData<T>(All11.ElementArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
		}
		
		public void SetData<T>(T[] indicesData) where T : struct
        {
			_bufferIndex = _bufferCount + 1;
            _buffer = indicesData;
			_size = indicesData.Length * Marshal.SizeOf(_type);
            _bufferPtr = GCHandle.Alloc(_buffer, GCHandleType.Pinned).AddrOfPinnedObject();
			_delayedBufferDelegates.Add(GenerateBuffer<T>);

			_allBuffers[_bufferIndex] = this;			
        }
		
		public override void Dispose ()
		{
			GL11.GenBuffers(0, ref _bufferStore);
            base.Dispose();
        }		
    }

	
	
    public class DynamicIndexBuffer : IndexBuffer
    {
        public DynamicIndexBuffer(GraphicsDevice graphics, Type type, int count, BufferUsage bufferUsage) 
            : base(graphics, type, count, bufferUsage)
        {
        }
    }

}
