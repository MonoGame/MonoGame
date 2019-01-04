using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class ModelMeshPart
	{
        private Effect _effect;

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

		public IndexBuffer IndexBuffer { get; set; }

		public int NumVertices { get; set; }

		public int PrimitiveCount { get; set; }

		public int StartIndex { get; set; }

		public object Tag { get; set; }

		public VertexBuffer VertexBuffer { get; set; }

		public int VertexOffset { get; set; }

		internal int VertexBufferIndex { get; set; }

		internal int IndexBufferIndex { get; set; }

		internal int EffectIndex { get; set; }
		
		internal ModelMesh parent;
        
        internal ModelMeshPart() { }
	}
}
