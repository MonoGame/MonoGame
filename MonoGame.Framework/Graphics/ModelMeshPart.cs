using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class ModelMeshPart
	{
		// Summary:
		//     Gets or sets the material Effect for this mesh part. Reference page contains
		//     code sample.
		private Effect _effect;
		public Effect Effect {
			get {
				return _effect;
			}
			set {
				if (value == _effect) {
					return;
				}
				_effect = value;
				parent.Effects.Add (value);
			}
		}
		//
		// Summary:
		//     Gets the index buffer for this mesh part.
		public IndexBuffer IndexBuffer { get; set; }
		//
		// Summary:
		//     Gets the number of vertices used during a draw call.
		public int NumVertices { get; set; }
		//
		// Summary:
		//     Gets the number of primitives to render.
		public int PrimitiveCount { get; set; }
		//
		// Summary:
		//     Gets the location in the index array at which to start reading vertices.
		public int StartIndex { get; set; }
		//
		// Summary:
		//     Gets or sets an object identifying this model mesh part.
		public object Tag { get; set; }
		//
		// Summary:
		//     Gets the vertex buffer for this mesh part.
		public VertexBuffer VertexBuffer { get; set; }
		//
		// Summary:
		//     Gets the offset (in vertices) from the top of vertex buffer.
		public int VertexOffset { get; set; }

		internal int VertexBufferIndex { get; set; }

		internal int IndexBufferIndex { get; set; }

		internal int EffectIndex { get; set; }
		
		internal ModelMesh parent;
	}

	//// Summary:
	////     Represents a batch of geometry information to submit to the graphics device
	////     during rendering. Each ModelMeshPart is a subdivision of a ModelMesh object.
	////     The ModelMesh class is split into multiple ModelMeshPart objects, typically
	////     based on material information.
	//public sealed class ModelMeshPart
	//{
	//    // Summary:
	//    //     Gets or sets the material Effect for this mesh part. Reference page contains
	//    //     code sample.
	//    public Effect Effect { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the index buffer for this mesh part.
	//    public IndexBuffer IndexBuffer { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the number of vertices used during a draw call.
	//    public int NumVertices { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the number of primitives to render.
	//    public int PrimitiveCount { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the location in the index array at which to start reading vertices.
	//    public int StartIndex { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets or sets an object identifying this model mesh part.
	//    public object Tag { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the vertex buffer for this mesh part.
	//    public VertexBuffer VertexBuffer { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the offset (in vertices) from the top of vertex buffer.
	//    public int VertexOffset { get { throw new NotImplementedException(); } }
	//}
}
