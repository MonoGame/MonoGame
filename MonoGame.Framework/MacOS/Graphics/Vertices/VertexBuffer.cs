using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	public class VertexBuffer : IDisposable
	{
		private GraphicsDevice Graphics;
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
		internal static List<Action> _delayedBufferDelegates = new List<Action> ();

		public VertexBuffer (GraphicsDevice Graphics,Type type,int vertexCount,BufferUsage bufferUsage)
		{
			this.Graphics = Graphics;
			this._type = type;
			this._vertexCount = vertexCount;
			this._bufferUsage = bufferUsage;
		}
		
		public VertexBuffer (GraphicsDevice Graphics,VertexDeclaration vertexDecs,int vertexCount,BufferUsage bufferUsage)
			: this (Graphics, vertexDecs.GetType(), vertexCount, bufferUsage)
		{
		}
		
		public int VertexCount { get; set; }

		internal static void CreateFrameBuffers ()
		{
			foreach (var action in _delayedBufferDelegates)
				action.Invoke ();

			_delayedBufferDelegates.Clear ();
		}

		//internal void GenerateBuffer<T>() where T : struct, IVertexType
		internal void GenerateBuffer<T> () where T : struct
		{
			var vd = VertexDeclaration.FromType (_type);

			_size = vd.VertexStride * ((T[])_buffer).Length;

			BufferUsageHint bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;

			GL.GenBuffers (1, out _bufferStore);
			GL.BindBuffer (BufferTarget.ArrayBuffer, _bufferStore);
			GL.BufferData<T> (BufferTarget.ArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
		}

		public unsafe void GetData<T> (T[] vertices) where T : IVertexType
		{
			if (_buffer == null)
				throw new Exception ("Can't get data on an empty buffer");

			var _tbuff = (T[])_buffer;
			for (int i = 0; i < _tbuff.Length; i++)
				vertices [i] = _tbuff [i];
		}

		//public unsafe void SetData<T>(T[] vertices) where T : struct, IVertexType
		public unsafe void SetData<T> (T[] vertices) where T : struct
		{
			//the creation of the buffer should mb be moved to the constructor and then glMapBuffer and Unmap should be used to update it
			//glMapBuffer - sets data
			//glUnmapBuffer - finished setting data

			_buffer = vertices;
			_bufferPtr = GCHandle.Alloc (_buffer, GCHandleType.Pinned).AddrOfPinnedObject ();			

			_bufferIndex = _bufferCount + 1;
			_allBuffers [_bufferIndex] = this;

			_delayedBufferDelegates.Add (GenerateBuffer<T>);

			_bufferCount++;
			// TODO: Kill buffers in PhoneOSGameView.DestroyFrameBuffer()
		}

		public void Dispose ()
		{
			GL.GenBuffers (0, out _bufferStore);
		}

		public bool IsContentLost { 
			get {
				return Graphics.IsContentLost;
			}
		}
	}

	public class DynamicVertexBuffer : VertexBuffer
	{
		public DynamicVertexBuffer (GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
		: base(graphics, type, vertexCount, bufferUsage)
		{
		}

		public DynamicVertexBuffer (GraphicsDevice graphics, VertexDeclaration vertexDecs, int vertexCount, BufferUsage bufferUsage)
			: base (graphics,vertexDecs.GetType(), vertexCount,bufferUsage)
		{

		}
	}
}
