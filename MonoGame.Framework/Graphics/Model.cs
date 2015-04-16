﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a 3D-Model.
    /// </summary>
	public sealed class Model
	{
		private static Matrix[] sharedDrawBoneMatrices;
		
		private GraphicsDevice graphicsDevice;

        /// <summary>
        /// A collection of <see cref="ModelBone"/> objects which describe how each mesh in the
        /// mesh collection for this model relates to its parent mesh.
        /// </summary>
        public ModelBoneCollection Bones { get; private set; }

        /// <summary>
        /// A collection of <see cref="ModelMesh"/> objects which compose the model. Each <see cref="ModelMesh"/>
        /// in a model may be moved independently and may be composed of multiple materials
        /// identified as <see cref="ModelMeshPart"/> objects.
        /// </summary>
        public ModelMeshCollection Meshes { get; private set; }

        /// <summary>
        // Root bone for this model.
        /// </summary>
        public ModelBone Root { get; set; }

        /// <summary>
        /// Custom attached object.
        /// <remarks>
        /// Skinning data is example of attached object for model.
        /// </remarks>
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Sets custom effect for all meshes in this model.
        /// </summary>
        /// <param name="effect">Specified <see cref="Effect"/>.</param>
        /// <remarks>
        /// This call is equivalent to
        /// <code>
        /// foreach (var mesh in model.Meshes)
        /// {
        ///    foreach (var part in mesh.MeshParts)
        ///    {
        ///       part.Effect = effect;
        ///    }
        /// }
        /// </code>
        /// </remarks>
        public void SetCustomEffect(Effect effect)
	    {
            foreach (var mesh in this.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

		internal Model()
		{

		}

        internal Model(GraphicsDevice graphicsDevice, List<ModelBone> bones, List<ModelMesh> meshes)
		{
			// TODO: Complete member initialization
			this.graphicsDevice = graphicsDevice;

			Bones = new ModelBoneCollection(bones);
			Meshes = new ModelMeshCollection(meshes);
		}

        internal void BuildHierarchy()
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
			
			//string s = string.Empty;
			//
			//for (int i = 0; i < level; i++) 
			//{
			//	s += "\t";
			//}
			//
			//Debug.WriteLine("{0}:{1}", s, node.Name);
		}

        /// <summary>
        /// Draws a model after applying the matrix transformations.
        /// </summary>
        /// <param name="world">A matrix of world transformation.</param>
        /// <param name="view">A matrix of view transformation.</param>
        /// <param name="projection">A matrix of projection transformation.</param>
        public void Draw(Matrix world, Matrix view, Matrix projection) 
		{       
            int boneCount = this.Bones.Count;
			
			if (sharedDrawBoneMatrices == null ||
				sharedDrawBoneMatrices.Length < boneCount)
			{
				sharedDrawBoneMatrices = new Matrix[boneCount];    
			}
			
			// Look up combined bone matrices for the entire model.            
			CopyAbsoluteBoneTransformsTo(sharedDrawBoneMatrices);

            // Draw the model.
            foreach (ModelMesh mesh in Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
					IEffectMatrices effectMatricies = effect as IEffectMatrices;
					if (effectMatricies == null) {
						throw new InvalidOperationException();
					}
                    effectMatricies.World = sharedDrawBoneMatrices[mesh.ParentBone.Index] * world;
                    effectMatricies.View = view;
                    effectMatricies.Projection = projection;
                }

                mesh.Draw();
            }
		}

        /// <summary>
        /// Copies bone transforms relative to all parent bones of the each bone from this model to a given array.
        /// </summary>
        /// <param name="destinationBoneTransforms">An array where bone transform data will be copied.</param>
        public void CopyAbsoluteBoneTransformsTo(Matrix[] destinationBoneTransforms)
		{
			if (destinationBoneTransforms == null)
				throw new ArgumentNullException("destinationBoneTransforms");
            if (destinationBoneTransforms.Length < this.Bones.Count)
				throw new ArgumentOutOfRangeException("destinationBoneTransforms");
            int count = this.Bones.Count;
			for (int index1 = 0; index1 < count; ++index1)
			{
                ModelBone modelBone = (this.Bones)[index1];
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

        /// <summary>
        /// Copies bone transforms relative to <see cref="Model.Root"/> bone from a given array to this model.
        /// </summary>
        /// <param name="sourceBoneTransforms">An array of prepared bone transform data.</param>
        public void CopyBoneTransformsFrom(Matrix[] sourceBoneTransforms)
        {
            if(sourceBoneTransforms==null)
                throw new ArgumentNullException("sourceBoneTransforms");
            if (sourceBoneTransforms.Length < Bones.Count)
                throw new ArgumentOutOfRangeException("sourceBoneTransforms");

            int count = Bones.Count;
            for (int i = 0; i < count; i++)
            {
                Bones[i].Transform = sourceBoneTransforms[i];
            }
        }

        /// <summary>
        /// Copies bone transforms relative to <see cref="Model.Root"/> bone from this model to a given array.
        /// </summary>
        /// <param name="destinationBoneTransforms">An array which will be filled with bone data.</param>
        public void CopyBoneTransformsTo(Matrix[] destinationBoneTransforms)
        {
            if (destinationBoneTransforms == null)
                throw new ArgumentNullException("destinationBoneTransforms");
            if (destinationBoneTransforms.Length < Bones.Count)
                throw new ArgumentOutOfRangeException("destinationBoneTransforms");

            int count = Bones.Count;
            for (int i = 0; i < count; i++)
            {
                destinationBoneTransforms[i] = Bones[i].Transform;
            }
        }
	}
}
