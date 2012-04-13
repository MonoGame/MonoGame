// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content
{
	public class ModelReader : ContentTypeReader<Model>
	{
		List<VertexBuffer> vertexBuffers = new List<VertexBuffer>();
        List<IndexBuffer> indexBuffers = new List<IndexBuffer>();
		List<Effect> effects = new List<Effect>();
		List<GraphicsResource> sharedResources = new List<GraphicsResource>();

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
                Debug.WriteLine("bone #{0}", boneId - 1);
                return (int)(boneId - 1);
            }
            else
            {
                Debug.WriteLine("null");
            }

            return -1;
        }
		
		protected internal override Model Read(ContentReader reader, Model existingInstance)
		{
			List<ModelBone> bones = new List<ModelBone>();

            // Read the bone names and transforms.
            uint boneCount = reader.ReadUInt32();
            Debug.WriteLine("Bone count: {0}", boneCount);

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

                Debug.WriteLine("Bone {0} hierarchy:", i);

                // Read the parent bone reference.
                Debug.WriteLine("Parent: ");
                var parentIndex = ReadBoneReference(reader, boneCount);

                if (parentIndex != -1)
                {
                    bone.Parent = bones[parentIndex];
                }

                // Read the child bone references.
                uint childCount = reader.ReadUInt32();

                if (childCount != 0)
                {
                    Debug.WriteLine("Children:");

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
            Debug.WriteLine("Mesh count: {0}", meshCount);

            for (int i = 0; i < meshCount; i++)
            {

                Debug.WriteLine("Mesh {0}", i);
                string name = reader.ReadObject<string>();
                var parentBoneIndex = ReadBoneReference(reader, boneCount);
				var boundingSphere = reader.ReadBoundingSphere();

                // Tag
                reader.ReadObject<object>();

                // Read the mesh part data.
                int partCount = reader.ReadInt32();
                Debug.WriteLine("Mesh part count: {0}", partCount);

                List<ModelMeshPart> parts = new List<ModelMeshPart>();

                for (uint j = 0; j < partCount; j++)
                {
                    ModelMeshPart part = new ModelMeshPart();

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
				ModelMesh mesh = new ModelMesh(reader.GraphicsDevice, parts);
				mesh.Name = name;
				mesh.ParentBone = bones[parentBoneIndex];
				mesh.ParentBone.AddMesh(mesh);
				mesh.BoundingSphere = boundingSphere;
				meshes.Add(mesh);
            }

            // Read the final pieces of model data.
            var rootBoneIndex = ReadBoneReference(reader, boneCount);

            Model model = new Model(reader.GraphicsDevice, bones, meshes);

            model.Root = bones[rootBoneIndex];
		
			model.BuildHierarchy();
			
			// Tag?
            model.Tag = reader.ReadObject<object>();
			
			return model;
		}
	}
}

