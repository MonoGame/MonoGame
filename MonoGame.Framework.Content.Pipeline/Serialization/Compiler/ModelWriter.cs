// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class ModelWriter : BuiltInContentWriter<ModelContent>
    {
        protected internal override void Write(ContentWriter output, ModelContent value)
        {
            WriteBones(output, value.Bones);

            output.Write(value.Meshes.Count);
            foreach (var mesh in value.Meshes)
            {
                output.Write(mesh.Name);
                WriteBoneReference(output, mesh.ParentBone, value.Bones);
                output.Write(mesh.BoundingSphere);
                output.WriteObject(mesh.Tag);

                output.Write(mesh.MeshParts.Count);
                foreach (var part in mesh.MeshParts)
                {
                    output.Write(part.VertexOffset);
                    output.Write(part.NumVertices);
                    output.Write(part.StartIndex);
                    output.Write(part.PrimitiveCount);
                    output.WriteObject(part.Tag);

                    output.WriteSharedResource(part.VertexBuffer);
                    output.WriteSharedResource(part.IndexBuffer);
                    output.WriteSharedResource(part.Material);
                }
            }

            WriteBoneReference(output, value.Root, value.Bones);
            output.WriteObject(value.Tag);
        }

        private void WriteBones(ContentWriter output, ModelBoneContentCollection bones)
        {
            if (bones == null)
            {
                output.Write(0u);
                return;
            }

            output.Write((uint)bones.Count);

            // Bone properties
            foreach (var bone in bones)
            {
                output.Write(bone.Name);
                output.Write(bone.Transform);
            }

            // Hierarchy
            foreach (var bone in bones)
            {
                WriteBoneReference(output, bone.Parent, bones);

                foreach (var child in bone.Children)
                    WriteBoneReference(output, child, bones);
            }
        }

        private void WriteBoneReference(ContentWriter output, ModelBoneContent bone, ModelBoneContentCollection bones)
        {
            var boneCount = bones != null ? bones.Count : 0;
            var boneId = bone != null
                             ? bone.Index + 1
                             : 0;

            if (boneCount < 255)
                output.Write((byte)boneId);
            else
                output.Write((uint)boneId);
        }
    }
}
