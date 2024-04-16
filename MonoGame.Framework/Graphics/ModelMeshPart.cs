using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a batch of geometry information to submit to the graphics device during rendering.
    /// Each <b>ModelMeshPart</b> is a subdivision of a <see cref="ModelMesh"/> object.
    /// The <see cref="ModelMesh"/> class is split into multiple <b>ModelMeshPart</b> objects,
    /// typically based on material information.
    /// </summary>
    /// <remarks>
    /// It is not necessary to use this class directly.
    /// In advanced rendering scenarios, it is possible to draw using <b>ModelMeshPart</b> properties in combination
    /// with the vertex and index buffers on <see cref="ModelMesh"/>.
    /// However, in most cases, <see cref="ModelMesh.Draw()"/> will be sufficient.
    /// </remarks>
	public sealed class ModelMeshPart
	{
        private Effect _effect;

        /// <summary>
        /// Gets or sets the material <see cref="Effect"/> for this mesh part.
        /// </summary>
        public Effect Effect 
        {
            get 
            {
                return _effect;
            }
            set 
            {
                if (value == _effect)
                    return;

                if (_effect != null)
                {
                    // First check to see any other parts are also using this effect.
                    var removeEffect = true;
                    foreach (var part in parent.MeshParts)
                    {
                        if (part != this && part._effect == _effect)
                        {
                            removeEffect = false;
                            break;
                        }
                    }

                    if (removeEffect)
                        parent.Effects.Remove(_effect);
                }

                // Set the new effect.
                _effect = value;
                
                if (_effect != null && !parent.Effects.Contains(_effect))                
                    parent.Effects.Add(_effect);
            }
        }

        /// <summary>
        /// Gets the index buffer for this mesh part.
        /// </summary>
		public IndexBuffer IndexBuffer { get; set; }

        /// <summary>
        /// Gets the number of vertices used during a draw call.
        /// </summary>
		public int NumVertices { get; set; }

        /// <summary>
        /// Gets the number of primitives to render.
        /// </summary>
		public int PrimitiveCount { get; set; }

        /// <summary>
        /// Gets the location in the index array at which to start reading vertices.
        /// </summary>
		public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets an object identifying this model mesh part.
        /// </summary>
		public object Tag { get; set; }

        /// <summary>
        /// Gets the vertex buffer for this mesh part.
        /// </summary>
		public VertexBuffer VertexBuffer { get; set; }

        /// <summary>
        /// Gets the offset (in vertices) from the top of vertex buffer.
        /// </summary>
		public int VertexOffset { get; set; }

		internal int VertexBufferIndex { get; set; }

		internal int IndexBufferIndex { get; set; }

		internal int EffectIndex { get; set; }
		
		internal ModelMesh parent;

        /// <summary>
        /// Using this constructor is strongly discouraged. Adding meshes to models at runtime is
        /// not supported and may lead to <see cref="NullReferenceException"/>s if parent is not set.
        /// </summary>
        [Obsolete("This constructor is deprecated and will be made internal in a future release.")]
        public ModelMeshPart() { }
	}
}