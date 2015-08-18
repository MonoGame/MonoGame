// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Binding of vertex buffer and other per-vertex parameters for graphics pipeline.
	/// </summary>
	public struct VertexBufferBinding
	{
		#region Private Fields

		private VertexBuffer _vertexBuffer;
		private int _vertexOffset;
		private int _instanceFrequency;

		#endregion
		
		#region Public Properties
		
		/// <summary>
		/// Amount of instances to draw for each draw call.
		/// </summary>
		public int InstanceFrequency{get{return _instanceFrequency;}}
	
		/// <summary>
		/// An offset in vertices from the beginning of vertex sequence.
		/// </summary>
	    public int VertexOffset{get{return _vertexOffset;}}
	
	    /// <summary>
	    /// The vertex buffer associated with this binding.
	    /// </summary>
	    public VertexBuffer VertexBuffer{get{return this._vertexBuffer;}}
		
		#endregion		
		
		#region Constructors
		
		/// <summary>
		/// Constructs a new vertex binding from the vertex buffer, vertex offset and instance frequency.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer.</param>
		/// <param name="vertexOffset">An offset in vertices.</param>
		/// <param name="instanceFrequency">Amount of instances to draw for each draw call.</param>
		public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset, int instanceFrequency)
	    {
	      if (vertexBuffer == null)
	        throw new ArgumentNullException("vertexBuffer");
	      if (vertexOffset < 0 || vertexOffset >= vertexBuffer.VertexCount)
	        throw new ArgumentOutOfRangeException("vertexOffset");
	      if (instanceFrequency < 0)
	        throw new ArgumentOutOfRangeException("instanceFrequency");
	      _vertexBuffer = vertexBuffer;
	      _vertexOffset = vertexOffset;
	      _instanceFrequency = instanceFrequency;
	    }
		
		/// <summary>
		/// Constructs a new vertex binding from the vertex buffer and vertex offset.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer.</param>
		/// <param name="vertexOffset">An offset in vertices.</param>
		public VertexBufferBinding(VertexBuffer vertexBuffer, int vertexOffset)
	    {
	      if (vertexBuffer == null)
	        throw new ArgumentNullException("vertexBuffer");
	      if (vertexOffset < 0 || vertexOffset >= vertexBuffer.VertexCount)
	        throw new ArgumentOutOfRangeException("vertexOffset");
	      _vertexBuffer = vertexBuffer;
	      _vertexOffset = vertexOffset;
	      _instanceFrequency = 0;
	    }
		
		/// <summary>
		/// Constructs a new vertex binding from the vertex buffer.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer.</param>
		public VertexBufferBinding(VertexBuffer vertexBuffer)
	    {
	      if (vertexBuffer == null)
	      	throw new ArgumentNullException("vertexBuffer");
	      
	      _vertexBuffer = vertexBuffer;
	      _vertexOffset = 0;
	      _instanceFrequency = 0;
	    }
		
		#endregion
		
		#region Operators
					
		/// <summary>
		/// Implicit conversion of <see cref="VertexBuffer"/> to equivalent <see cref="VertexBufferBinding"/> representation.
		/// </summary>
		/// <param name="vertexBuffer">The vertex buffer for conversion.</param>
		/// <returns>A new instance of <see cref="VertexBufferBinding"/> created from the vertex buffer.</returns>
		public static implicit operator VertexBufferBinding(VertexBuffer vertexBuffer)
	    {
	      return new VertexBufferBinding(vertexBuffer);
	    }
		
		#endregion
	}
}
