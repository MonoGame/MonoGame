using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{

	// Summary:
	//     Represents a mesh that is part of a Model.
	public sealed class ModelMesh
	{
		private GraphicsDevice graphicsDevice;
		private System.Collections.Generic.List<ModelMeshPart> parts;

		public ModelMesh(GraphicsDevice graphicsDevice, System.Collections.Generic.List<ModelMeshPart> parts)
		{
			// TODO: Complete member initialization
			this.parts = parts;
			this.graphicsDevice = graphicsDevice;
			
			MeshParts = new ModelMeshPartCollection(parts);
			
			for (int i = 0; i < parts.Count; i++) {
				parts[i].parent = this;
			}
			
			Effects = new ModelEffectCollection();
		}
		
		/*internal void BuildEffectList()
		{
			List<Effect> effects = new List<Effect>();
			foreach (ModelMeshPart item in parts) 
			{
				if (!effects.Contains(item.Effect))
				{
					if (item.Effect != null)
						effects.Add(item.Effect);
				}
			}
			Effects = new ModelEffectCollection(effects);
		}*/
		
		// Summary:
		//     Gets the BoundingSphere that contains this mesh.
		public BoundingSphere BoundingSphere { get; set; }
		//
		// Summary:
		//     Gets a collection of effects associated with this mesh.
		public ModelEffectCollection Effects { get; set; }
		//
		// Summary:
		//     Gets the ModelMeshPart objects that make up this mesh. Each part of a mesh
		//     is composed of a set of primitives that share the same material.
		public ModelMeshPartCollection MeshParts { get; set; }
		//
		// Summary:
		//     Gets the name of this mesh.
		public string Name { get; set; }
		//
		// Summary:
		//     Gets the parent bone for this mesh. The parent bone of a mesh contains a
		//     transformation matrix that describes how the mesh is located relative to
		//     any parent meshes in a model.
		public ModelBone ParentBone { get; set; }
		//
		// Summary:
		//     Gets or sets an object identifying this mesh.
		public object Tag { get; set; }
				
		// Summary:
		//     Draws all of the ModelMeshPart objects in this mesh, using their current
		//     Effect settings.
		public void Draw()
		{	
			for(int i = 0; i < MeshParts.Count; i++)
			{
				var part = MeshParts[i];
				var effect = part.Effect;
				
				if (part.PrimitiveCount > 0)
				{
                    this.graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    this.graphicsDevice.Indices = part.IndexBuffer;
                    
                    for (int j = 0; j < effect.CurrentTechnique.Passes.Count; j++)
                    {
						effect.CurrentTechnique.Passes[j].Apply ();
						this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
					}
				}
			}
		}
	}


	//// Summary:
	////     Represents a mesh that is part of a Model.
	//public sealed class ModelMesh
	//{
	//    // Summary:
	//    //     Gets the BoundingSphere that contains this mesh.
	//    public BoundingSphere BoundingSphere { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets a collection of effects associated with this mesh.
	//    public ModelEffectCollection Effects { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the ModelMeshPart objects that make up this mesh. Each part of a mesh
	//    //     is composed of a set of primitives that share the same material.
	//    public ModelMeshPartCollection MeshParts { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the name of this mesh.
	//    public string Name { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the parent bone for this mesh. The parent bone of a mesh contains a
	//    //     transformation matrix that describes how the mesh is located relative to
	//    //     any parent meshes in a model.
	//    public ModelBone ParentBone { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets or sets an object identifying this mesh.
	//    public object Tag { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

	//    // Summary:
	//    //     Draws all of the ModelMeshPart objects in this mesh, using their current
	//    //     Effect settings.
	//    public void Draw() { throw new NotImplementedException(); }
	//}
}
