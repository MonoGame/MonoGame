﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#if IPHONE
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsageHint = OpenTK.Graphics.ES20.All;
#else
using BufferUsageHint = OpenTK.Graphics.ES20.BufferUsage;
#endif

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
		internal static int _bufferCount;
		internal static VertexBuffer[] _allBuffers = new VertexBuffer[50];
		internal static List<Action> _delayedBufferDelegates = new List<Action> ();

		public VertexBuffer (GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
		{
			graphicsDevice = graphics;
			_type = type;
			VertexCount = vertexCount;
			_bufferUsage = bufferUsage;
		}
		
		public VertexBuffer (GraphicsDevice Graphics, VertexDeclaration vertexDecs, int vertexCount, BufferUsage bufferUsage)
			: this (Graphics, vertexDecs.GetType(), vertexCount, bufferUsage)
		{
			_vertexDeclaration = vertexDecs;
		}
		
		public int VertexCount { get; set; }

		public VertexDeclaration VertexDeclaration {
			get {
				return _vertexDeclaration;
			}
		}

		internal static void CreateFrameBuffers ()
		{
			foreach (var action in _delayedBufferDelegates)
				action.Invoke ();

			_delayedBufferDelegates.Clear ();
		}

		internal void GenerateBuffer<T> () where T : struct
		{
			var vd = VertexDeclaration.FromType (_type);

			_size = vd.VertexStride * ((T[])_buffer).Length;

			BufferUsageHint bufferUsage = (_bufferUsage == BufferUsage.WriteOnly) ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
			
#if IPHONE
			GL.GenBuffers (1, ref _bufferStore);
#else			
			GL.GenBuffers (1, out _bufferStore);
#endif
			GL.BindBuffer (BufferTarget.ArrayBuffer, _bufferStore);
			GL.BufferData (BufferTarget.ArrayBuffer, (IntPtr)_size, (T[])_buffer, bufferUsage);			
		}

		public void GetData<T> (T[] vertices) where T : struct
		{
			if (_buffer == null)
				throw new Exception ("Can't get data on an empty buffer");

			var _tbuff = (T[])_buffer;
			for (int i = 0; i < _tbuff.Length; i++)
				vertices [i] = _tbuff [i];
		}

        public void SetData<T>(T[] vertices) where T : struct
        {
            // TODO: This is fundimentally broken in that it is not 
            // assured that the incoming vertex array will exist unmodified
            // long enough for the delayed buffer creation to occur. 
            //
            // We either need to remove the concept of delayed buffer 
            // creation or copy the data here for safe keeping.

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

        public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();           
        }

		public void SetData<T> (
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride
			) where T : struct
		{
			throw new NotImplementedException();
		}

		public override void Dispose ()
		{
#if IPHONE
			GL.GenBuffers(0, ref _bufferStore);
#else			
			GL.GenBuffers (0, out _bufferStore);
#endif
            base.Dispose();
		}
	}

	public class DynamicVertexBuffer : VertexBuffer
	{
		public DynamicVertexBuffer (GraphicsDevice graphics, Type type, int vertexCount, BufferUsage bufferUsage)
		: base(graphics, type, vertexCount, bufferUsage)
		{
		}

		public DynamicVertexBuffer (GraphicsDevice graphics, VertexDeclaration vertexDecs, int vertexCount, BufferUsage bufferUsage)
			: base (graphics,vertexDecs, vertexCount,bufferUsage)
		{

		}
		
		public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
		{
		    throw new NotImplementedException();
		}
		
		public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
		{
		    throw new NotImplementedException();
		}
	}
}
