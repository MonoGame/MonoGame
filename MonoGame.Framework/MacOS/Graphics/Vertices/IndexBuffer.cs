using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public class IndexBuffer
    {
        private GraphicsDevice _graphics;
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
		
        public IndexBuffer(GraphicsDevice Graphics, Type type, int count, BufferUsage bufferUsage)
        {
			if (type != typeof(uint) && type != typeof(ushort) && type != typeof(byte))
				throw new NotSupportedException("The only types that are supported are: uint, ushort and byte");
			
            this._graphics = Graphics;
            this._type = type;
            this._count = count;
            this._bufferUsage = bufferUsage;
        }
        
		internal void GenerateBuffer<T>() where T : struct
		{
            BufferUsageHint bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
            GL.GenBuffers(1, out _bufferStore);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _bufferStore);
            GL.BufferData<T>(BufferTarget.ElementArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
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
		
		public void Dispose ()
		{
			GL.GenBuffers(0, out _bufferStore);
		}		
    }

	
	
    public class DynamicIndexBuffer : IndexBuffer
    {
        public DynamicIndexBuffer(GraphicsDevice Graphics, Type type, int count, BufferUsage bufferUsage) : base(Graphics, type, count, bufferUsage)
        {
        }
    }

}
