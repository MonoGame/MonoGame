// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content
{
    internal class ModelReader : ContentTypeReader<Model>
	{
		public ModelReader ()
		{
		}
		
		static int ReadBoneReference(ContentReader reader, uint boneCount)
        {
            uint boneId;

            // Read the bone ID, which may be encoded as either an 8 or 32 bit value.
            if (boneCount < 255)
            {
                boneId = reader.ReadByte();
            }
            else
            {
                boneId = reader.ReadUInt32();
            }

            // Print out the bone ID.
            if (boneId != 0)
            {
                return (int)(boneId - 1);
            }

            return -1;
        }
		
		protected internal override Model Read(ContentReader reader, Model existingInstance)
		{
            // Read the bone names and transforms.
            uint boneCount = reader.ReadUInt32();

            List<ModelBone> bones = new List<ModelBone>((int)boneCount);

            for (uint i = 0; i < boneCount; i++)
            {
                string name = reader.ReadObject<string>();
				var matrix = reader.ReadMatrix();
                var bone = new ModelBone { Transform = matrix, Index = (int)i, Name = name };
                bones.Add(bone);
            }
			
            // Read the bone hierarchy.
            for (int i = 0; i < boneCount; i++)
            {
                var bone = bones[i];

                // Read the parent bone reference.
                var parentIndex = ReadBoneReference(reader, boneCount);

                if (parentIndex != -1)
                {
                    bone.Parent = bones[parentIndex];
                }

                // Read the child bone references.
                uint childCount = reader.ReadUInt32();

                if (childCount != 0)
                {
                    for (uint j = 0; j < childCount; j++)
                    {
                        var childIndex = ReadBoneReference(reader, boneCount);
                        if (childIndex != -1)
                        {
                            bone.AddChild(bones[childIndex]);
                        }
                    }
                }
            }

            List<ModelMesh> meshes = new List<ModelMesh>();

            //// Read the mesh data.
            int meshCount = reader.ReadInt32();
            for (int i = 0; i < meshCount; i++)
            {
                string name = reader.ReadObject<string>();
                var parentBoneIndex = ReadBoneReference(reader, boneCount);
				var boundingSphere = reader.ReadBoundingSphere();

                // Tag
                var meshTag = reader.ReadObject<object>();

                // Read the mesh part data.
                int partCount = reader.ReadInt32();

                List<ModelMeshPart> parts = new List<ModelMeshPart>(partCount);

                for (uint j = 0; j < partCount; j++)
                {
                    ModelMeshPart part;
                    if (existingInstance != null)
                        part = existingInstance.Meshes[i].MeshParts[(int)j];
                    else
                        part = new ModelMeshPart();

					part.VertexOffset = reader.ReadInt32();
                    part.NumVertices = reader.ReadInt32();
                    part.StartIndex = reader.ReadInt32();
                    part.PrimitiveCount = reader.ReadInt32();

                    // tag
                    part.Tag = reader.ReadObject<object>();
					
					parts.Add(part);
					
					int jj = (int)j;
					reader.ReadSharedResource<VertexBuffer>(delegate (VertexBuffer v)
					{
						parts[jj].VertexBuffer = v;
					});
					reader.ReadSharedResource<IndexBuffer>(delegate (IndexBuffer v)
					{
						parts[jj].IndexBuffer = v;
					});
					reader.ReadSharedResource<Effect>(delegate (Effect v)
					{
						parts[jj].Effect = v;
					});

					
                }

                if (existingInstance != null)
                    continue;

				ModelMesh mesh = new ModelMesh(reader.GetGraphicsDevice(), parts);

                // Tag reassignment
                mesh.Tag = meshTag;

				mesh.Name = name;
				mesh.ParentBone = bones[parentBoneIndex];
				mesh.ParentBone.AddMesh(mesh);
				mesh.BoundingSphere = boundingSphere;
				meshes.Add(mesh);
            }

            if (existingInstance != null)
            {
                // Read past remaining data and return existing instance
                ReadBoneReference(reader, boneCount);
                reader.ReadObject<object>();
                return existingInstance;
            }

            // Read the final pieces of model data.
            var rootBoneIndex = ReadBoneReference(reader, boneCount);

            Model model = new Model(reader.GetGraphicsDevice(), bones, meshes);

            model.Root = bones[rootBoneIndex];
		
			model.BuildHierarchy();
			
			// Tag?
            model.Tag = reader.ReadObject<object>();
			
			return model;
		}
	}
}

