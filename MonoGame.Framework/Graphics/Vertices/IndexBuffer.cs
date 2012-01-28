using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#else
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class IndexBuffer : GraphicsResource
    {
        internal Type _type;
        internal int _count;
		private object _buffer;
		internal IntPtr _bufferPtr;
		internal IntPtr _sizePtr;
        private readonly BufferUsage _bufferUsage;
		internal int _bufferIndex;
		internal int _size;
		internal uint _bufferStore;

        // TODO: Remove this IB limit!
        internal static IndexBuffer[] _allBuffers = new IndexBuffer[50];
        internal static int _bufferCount;

		internal static List<Action> _delayedBufferDelegates = new List<Action>(); 
		
		internal static void CreateFrameBuffers()
		{
			foreach (var action in _delayedBufferDelegates)
				action.Invoke();
			
			_delayedBufferDelegates.Clear();
		}		

		public IndexElementSize IndexElementSize { get; internal set; }
		
		public int IndexCount { get; internal set; }

        public IndexBuffer (GraphicsDevice graphics, Type type, int count, BufferUsage bufferUsage)
        {
			if (type != typeof(int) && type != typeof(short) && type != typeof(byte) && type != typeof(ushort))
				throw new NotSupportedException ("The only types that are supported are: int, short, byte, ushort");

            graphicsDevice = graphics;
            _type = type;
			if (type == typeof(short))
				IndexElementSize = IndexElementSize.SixteenBits;
			else
				IndexElementSize = IndexElementSize.ThirtyTwoBits;
			IndexCount = count;
            _bufferUsage = bufferUsage;
        }

		public IndexBuffer (GraphicsDevice graphics, IndexElementSize size, int count, BufferUsage bufferUsage)
		{
            graphicsDevice = graphics;
			_type = (size == IndexElementSize.SixteenBits) ? typeof(short) : typeof(int);
			IndexCount = count;
			IndexElementSize = size;
			_bufferUsage = bufferUsage;
		}

		internal void GenerateBuffer<T>() where T : struct
		{
			#if MONOMAC		
			
			BufferUsageHint bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
			GL.GenBuffers (1, out _bufferStore);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, _bufferStore);
			GL.BufferData<T> (BufferTarget.ElementArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
			
			#else
						
            var bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? All11.StaticDraw : All11.DynamicDraw;
            GL11.GenBuffers(1, ref _bufferStore);
            GL11.BindBuffer(All11.ElementArrayBuffer, _bufferStore);
            GL11.BufferData<T>(All11.ElementArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			

			#endif
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
			#if MONOMAC		
			GL.GenBuffers (0, out _bufferStore);
			#else					
			GL11.GenBuffers(0, ref _bufferStore);
			#endif
						
            base.Dispose();
        }		
    }
}
