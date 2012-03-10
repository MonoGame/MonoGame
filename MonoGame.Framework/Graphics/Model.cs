using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
	public class Model
	{
		private GraphicsDevice graphicsDevice;
		private List<ModelBone> bones;
		private List<ModelMesh> meshes;

		// Summary:
		//     Gets a collection of ModelBone objects which describe how each mesh in the
		//     Meshes collection for this model relates to its parent mesh.
		public ModelBoneCollection Bones { get; private set; }
		//
		// Summary:
		//     Gets a collection of ModelMesh objects which compose the model. Each ModelMesh
		//     in a model may be moved independently and may be composed of multiple materials
		//     identified as ModelMeshPart objects.
		public ModelMeshCollection Meshes { get; private set; }
		//
		// Summary:
		//     Gets the root bone for this model.
		public ModelBone Root { get; set; }
		//
		// Summary:
		//     Gets or sets an object identifying this model.
		public object Tag { get; set; }

		public Model()
		{

		}

		public Model(GraphicsDevice graphicsDevice, List<ModelBone> bones, List<ModelMesh> meshes)
		{
			// TODO: Complete member initialization
			this.bones = bones;
			this.meshes = meshes;
			this.graphicsDevice = graphicsDevice;

			Bones = new ModelBoneCollection(bones);
			Meshes = new ModelMeshCollection(meshes);
		}
		
		public void BuildHierarchy()
		{
			var globalScale = Matrix.CreateScale(0.01f);
			
			foreach(var node in this.Root.Children)
			{
				BuildHierarchy(node, this.Root.Transform * globalScale, 0);
			}
		}
		
		private void BuildHierarchy(ModelBone node, Matrix parentTransform, int level)
		{
			node.ModelTransform = node.Transform * parentTransform;
			
			foreach (var child in node.Children) 
			{
				BuildHierarchy(child, node.ModelTransform, level + 1);
			}
			
			string s = string.Empty;
			
			for (int i = 0; i < level; i++) 
			{
				s += "\t";
			}
			
			Debug.WriteLine("{0}:{1}", s, node.Name);
		}
		
		float dt = 0;
		float timer = 0;
		int modelPart = 0;
		
		public void Draw(Matrix world, Matrix view, Matrix projection) 
		{       			
			// Look up combined bone matrices for the entire model.
            Matrix[] boneTransforms = new Matrix[bones.Count];
			CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
					IEffectMatrices effectMatricies = effect as IEffectMatrices;
					if (effectMatricies == null) {
						throw new InvalidOperationException();
					}
                    effectMatricies.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effectMatricies.View = view;
                    effectMatricies.Projection = projection;
                }

                mesh.Draw();
            }
		}
		
		public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			if (destinationBoneTransforms == null)
				throw new ArgumentNullException("destinationBoneTransforms");
			if (destinationBoneTransforms.Length < this.bones.Count)
				throw new ArgumentOutOfRangeException("destinationBoneTransforms");
			int count = this.bones.Count;
			for (int index1 = 0; index1 < count; ++index1)
			{
				ModelBone modelBone = (this.bones)[index1];
				if (modelBone.Parent == null)
				{
					destinationBoneTransforms[index1] = modelBone.transform;
				}
				else
				{
					int index2 = modelBone.Parent.Index;
					Matrix.Multiply(ref modelBone.transform, ref destinationBoneTransforms[index2], out destinationBoneTransforms[index1]);
				}
			}
		}
	}

	//// Summary:
	////     Represents a 3D model composed of multiple ModelMesh objects which may be
	////     moved independently.
	//public sealed class Model
	//{
	//    // Summary:
	//    //     Gets a collection of ModelBone objects which describe how each mesh in the
	//    //     Meshes collection for this model relates to its parent mesh.
	//    public ModelBoneCollection Bones { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets a collection of ModelMesh objects which compose the model. Each ModelMesh
	//    //     in a model may be moved independently and may be composed of multiple materials
	//    //     identified as ModelMeshPart objects.
	//    public ModelMeshCollection Meshes { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the root bone for this model.
	//    public ModelBone Root { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets or sets an object identifying this model.
	//    public object Tag { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

	//    // Summary:
	//    //     Copies a transform of each bone in a model relative to all parent bones of
	//    //     the bone into a given array.
	//    //
	//    // Parameters:
	//    //   destinationBoneTransforms:
	//    //     The array to receive bone transforms.
	//    public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms) { throw new NotImplementedException(); }
	//    //
	//    // Summary:
	//    //     Copies an array of transforms into each bone in the model.
	//    //
	//    // Parameters:
	//    //   sourceBoneTransforms:
	//    //     An array containing new bone transforms.
	//    public void CopyBoneTransformsFrom(Matrix[] sourceBoneTransforms) { throw new NotImplementedException(); }
	//    //
	//    // Summary:
	//    //     Copies each bone transform relative only to the parent bone of the model
	//    //     to a given array.
	//    //
	//    // Parameters:
	//    //   destinationBoneTransforms:
	//    //     The array to receive bone transforms.
	//    public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms) { throw new NotImplementedException(); }
	//    //
	//    // Summary:
	//    //     Render a model after applying the matrix transformations.
	//    //
	//    // Parameters:
	//    //   world:
	//    //     A world transformation matrix.
	//    //
	//    //   view:
	//    //     A view transformation matrix.
	//    //
	//    //   projection:
	//    //     A projection transformation matrix.
	//    public void Draw(Matrix world, Matrix view, Matrix projection) { throw new NotImplementedException(); }
	//}
}
