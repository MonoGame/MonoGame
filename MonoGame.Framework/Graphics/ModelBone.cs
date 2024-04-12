using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents bone data for a model.
    /// </summary>
    public sealed class ModelBone
	{
		private List<ModelBone> children = new List<ModelBone>();
		
		private List<ModelMesh> meshes = new List<ModelMesh>();

        /// <summary>
        /// List of the meshes for this bone.
        /// </summary>
		public List<ModelMesh> Meshes {
			get {
				return this.meshes;
			}
			private set {
				meshes = value;
			}
		}

        /// <summary>
        /// Gets a collection of bones that are children of this bone.
        /// </summary>
        public ModelBoneCollection Children { get; private set; }

        /// <summary>
        /// Gets the index of this bone in the <see cref="Model.Bones">Model.Bones</see> collection.
        /// </summary>
		public int Index { get; set; }

        /// <summary>
        /// Gets the name of this bone.
        /// </summary>
		public string Name { get; set; }

        /// <summary>
        /// Gets the parent of this bone.
        /// </summary>
		public ModelBone Parent { get; set; }

		internal Matrix transform;
        /// <summary>
        /// Gets or sets the matrix used to transform this bone relative to its parent bone.
        /// </summary>
		public Matrix Transform 
		{ 
			get { return this.transform; } 
			set { this.transform = value; }
		}
		
		/// <summary>
		/// Transform of this node from the root of the model not from the parent
		/// </summary>
		public Matrix ModelTransform {
			get;
			set;
		}

        /// <summary>
        /// Creates a new instance of <see cref="ModelBone"/>.
        /// </summary>
		public ModelBone ()	
		{
			Children = new ModelBoneCollection(new List<ModelBone>());
		}

        /// <summary>
        /// Add a <see cref="ModelMesh"/> to the mesh collection.
        /// </summary>
        /// <param name="mesh"><see cref="ModelMesh"/> to be added</param>
		public void AddMesh(ModelMesh mesh)
		{
			meshes.Add(mesh);
		}

        /// <summary>
        /// Adds a child bone to this bone.
        /// </summary>
        /// <param name="modelBone"><see cref="ModelBone"/> to be added.</param>
		public void AddChild(ModelBone modelBone)
		{
			children.Add(modelBone);
			Children = new ModelBoneCollection(children);
		}
	}

	//// Summary:
	////     Represents bone data for a model. Reference page contains links to related
	////     conceptual articles.
	//public sealed class ModelBone
	//{
	//    // Summary:
	//    //     Gets a collection of bones that are children of this bone.
	//    public ModelBoneCollection Children { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the index of this bone in the Bones collection.
	//    public int Index { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the name of this bone.
	//    public string Name { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets the parent of this bone.
	//    public ModelBone Parent { get { throw new NotImplementedException(); } }
	//    //
	//    // Summary:
	//    //     Gets or sets the matrix used to transform this bone relative to its parent
	//    //     bone.
	//    public Matrix Transform { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
	//}
}
