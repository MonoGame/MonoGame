﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;
using System.IO;
using System.Diagnostics;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    [ContentImporter(".fbx", ".x", DisplayName = "Open Asset Import Library - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class OpenAssetImporter : ContentImporter<NodeContent>
    {
        private Scene _scene;
        private NodeContent _rootNode;
        private List<MaterialContent> _materials;

        private Matrix4x4 _globalInverseXform = Matrix4x4.Identity;
        private Node _skeletonRoot;
        private List<string> _boneNames = new List<string>();
        private List<string> _skeletonNodes = new List<string>();
        private Dictionary<string, Matrix4x4> _objectToBone = new Dictionary<string, Matrix4x4>();

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            var identity = new ContentIdentity(filename, GetType().Name);

            using (var importer = new AssimpContext())
            {
                _scene = importer.ImportFile(filename,
                    //PostProcessSteps.FindInstances | // No effect + slow?
                    PostProcessSteps.FindInvalidData |
                    PostProcessSteps.FlipUVs |
                    PostProcessSteps.FlipWindingOrder |
                    //PostProcessSteps.MakeLeftHanded | // Appears to just mess things up
                    PostProcessSteps.JoinIdenticalVertices |
                    PostProcessSteps.ImproveCacheLocality |
                    PostProcessSteps.OptimizeMeshes |
                    //PostProcessSteps.OptimizeGraph | // Will eliminate helper nodes
                    PostProcessSteps.RemoveRedundantMaterials |
                    PostProcessSteps.Triangulate
                    );

                _globalInverseXform = _scene.RootNode.Transform;
                _globalInverseXform.Inverse();

                _rootNode = new NodeContent
                {
                    Name = _scene.RootNode.Name,
                    Identity = identity,
                    Transform = ToXna(_scene.RootNode.Transform)
                };

                _materials = ImportMaterials(identity, _scene);

                FindMeshes(_scene.RootNode, _scene.RootNode.Transform);

                if (_scene.HasAnimations)
                {
                    var skeleton = CreateSkeleton();
                    CreateAnimation(skeleton);
                }

                _scene.Clear();
            }

            return _rootNode;
        }

        private static List<MaterialContent> ImportMaterials(ContentIdentity identity, Scene scene)
        {
            var materials = new List<MaterialContent>();

            foreach (var sceneMaterial in scene.Materials)
            {
                var material = new BasicMaterialContent
                {
                    Name = sceneMaterial.Name,
                    Identity = identity,
                };

                if (sceneMaterial.HasTextureDiffuse)
                {
                    var texture = new ExternalReference<TextureContent>(sceneMaterial.TextureDiffuse.FilePath, identity);
                    texture.OpaqueData.Add("TextureCoordinate", string.Format("TextureCoordinate{0}", sceneMaterial.TextureDiffuse.UVIndex));
                    material.Textures.Add("Texture", texture);
                }

                if (sceneMaterial.HasTextureOpacity)
                {
                    var texture = new ExternalReference<TextureContent>(sceneMaterial.TextureOpacity.FilePath, identity);
                    texture.OpaqueData.Add("TextureCoordinate", string.Format("TextureCoordinate{0}", sceneMaterial.TextureOpacity.UVIndex));
                    material.Textures.Add("Transparency", texture);
                }

                if (sceneMaterial.HasColorDiffuse)
                    material.DiffuseColor = ToXna(sceneMaterial.ColorDiffuse);

                if (sceneMaterial.HasColorEmissive)
                    material.EmissiveColor = ToXna(sceneMaterial.ColorEmissive);

                if (sceneMaterial.HasOpacity)
                    material.Alpha = sceneMaterial.Opacity;

                if (sceneMaterial.HasColorSpecular)
                    material.SpecularColor = ToXna(sceneMaterial.ColorSpecular);

                if (sceneMaterial.HasShininessStrength)
                    material.SpecularPower = sceneMaterial.ShininessStrength;

                materials.Add(material);
            }

            return materials;
        }

        private MeshContent CreateMesh(Mesh sceneMesh)
        {
            var mesh = new MeshContent { Name = sceneMesh.Name };

            // Position vertices are shared at the mesh level
            foreach (var vert in sceneMesh.Vertices)
                mesh.Positions.Add(new Vector3(vert.X, vert.Y, vert.Z));

            var geom = new GeometryContent
            {
                Material = _materials[sceneMesh.MaterialIndex]
            };

            // Geometry vertices reference 1:1 with the MeshContent parent,
            // no indirection is necessary.
            //geom.Vertices.Positions.AddRange(mesh.Positions);
            geom.Vertices.AddRange(Enumerable.Range(0, sceneMesh.VertexCount));
            geom.Indices.AddRange(sceneMesh.GetIndices());

            if (sceneMesh.HasBones)
            {
                var xnaWeights = new List<BoneWeightCollection>();
                for (var i = 0; i < geom.Indices.Count; i++)
                {
                    var list = new BoneWeightCollection();
                    for (var boneIndex = 0; boneIndex < sceneMesh.BoneCount; boneIndex++)
                    {
                        var bone = sceneMesh.Bones[boneIndex];
                        foreach (var weight in bone.VertexWeights)
                        {
                            if (weight.VertexID != i)
                                continue;

                            list.Add(new BoneWeight(bone.Name, weight.Weight));
                        }
                    }
                    if (list.Count > 0)
                        xnaWeights.Add(list);
                }

                geom.Vertices.Channels.Add(VertexChannelNames.Weights(0), xnaWeights);
            }

            // Individual channels go here
            if (sceneMesh.HasNormals)
                geom.Vertices.Channels.Add(VertexChannelNames.Normal(), ToXna(sceneMesh.Normals));

            for (var i = 0; i < sceneMesh.TextureCoordinateChannelCount; i++)
                geom.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(i),
                                           ToXnaTexCoord(sceneMesh.TextureCoordinateChannels[i]));

            mesh.Geometry.Add(geom);

            return mesh;
        }

        private void FindMeshes(Node aiNode, Matrix4x4 parentXform)
        {
            var transform = parentXform * aiNode.Transform;
            foreach (var meshIndex in aiNode.MeshIndices)
            {
                var aiMesh = _scene.Meshes[meshIndex];

                // Extract bind pose.
                foreach (var bone in aiMesh.Bones)
                {
                    if (!_boneNames.Contains(bone.Name))
                        _boneNames.Add(bone.Name);

                    var boneName = bone.Name;
                    _objectToBone[boneName] = Matrix4x4.Identity;
                    //_objectToBone[boneName].Inverse();
                }

                // Extract geometry
                var mesh = CreateMesh(aiMesh);
                mesh.Name = aiNode.Name;
                mesh.Transform = ToXna(transform);

                _rootNode.Children.Add(mesh);
            }

            // Children
            foreach (var child in aiNode.Children)
                FindMeshes(child, transform);
        }

        private BoneContent CreateSkeleton()
        {
            _skeletonRoot = FindSkeletonRoot(_boneNames, _scene.RootNode);
            if (_skeletonRoot == null)
                return null;

            return WalkHierarchy(_skeletonRoot, _rootNode, Matrix4x4.Identity) as BoneContent;
        }

        private Node FindSkeletonRoot(IEnumerable<string> boneNames, Node sceneRoot)
        {
            Node rootNode = null;
            var minDepth = int.MaxValue;

            foreach (var boneName in boneNames)
            {
                var node = sceneRoot.FindNode(boneName);

                // Walk up the tree to find the depth of this node
                var depth = 0;
                var walk = node;
                while (walk != sceneRoot)
                {
                    walk = walk.Parent;
                    depth++;
                }

                if (depth < minDepth)
                {
                    rootNode = node;
                    minDepth = depth;
                }

                if (!_skeletonNodes.Contains(boneName))
                    _skeletonNodes.Add(boneName);
            }

            // We're at the base of the skeleton, now walk
            // all the way up to the scene root to get the
            // full stack.
            Node skeletonRoot = rootNode;
            while (rootNode.Parent != sceneRoot)
            {
                // The FBX path likes to put these extra preserve
                // pivot nodes in here.
                if (!rootNode.Name.Contains("$AssimpFbx$"))
                    skeletonRoot = rootNode;

                if (!_skeletonNodes.Contains(skeletonRoot.Name))
                    _skeletonNodes.Add(skeletonRoot.Name);

                rootNode = rootNode.Parent;
            }

            return skeletonRoot;
        }

        private NodeContent WalkHierarchy(Node aiNode, NodeContent xnaParent, Matrix4x4 parentXform)
        {
            var transform = Matrix4x4.Identity;
            Node walk = aiNode;
            while (walk != null && walk.Name != xnaParent.Name)
            {
                transform *= walk.Transform;
                walk = walk.Parent;
            }

            NodeContent node = null;

            if (!aiNode.Name.Contains("_$AssimpFbx$")) // Ignore pivot nodes
            {
                const string mangling = "_$AssimpFbxNull$"; // Null leaf nodes are helpers

                if (_skeletonNodes.Contains(aiNode.Name))
                    node = new BoneContent { Name = aiNode.Name };
                else if (aiNode.Name.Contains(mangling))
                    node = new NodeContent { Name = aiNode.Name.Replace(mangling, string.Empty) };

                // Only emit XNA nodes for concrete nodes
                if (node != null)
                {
                    node.Transform = ToXna(transform);
                    xnaParent.Children.Add(node);

                    // For the children, this is the new parent.
                    xnaParent = node;
                }
            }

            // Children
            foreach (var child in aiNode.Children)
                WalkHierarchy(child, xnaParent, transform);

            return node;
        }

        private static string FixupAnimationName(string name)
        {
            return name.Replace("AnimStack::", string.Empty);
        }

        private void CreateAnimation(NodeContent skeleton)
        {
            if (skeleton != null)
            {
                foreach (var animation in _scene.Animations)
                    skeleton.Animations.Add(FixupAnimationName(animation.Name), CreateAnimation(animation));
            }
        }

        private AnimationContent CreateAnimation(Assimp.Animation aiAnimation)
        {
            var animation = new AnimationContent
            {
                Name = FixupAnimationName(aiAnimation.Name),
                Duration = TimeSpan.FromSeconds(aiAnimation.DurationInTicks / aiAnimation.TicksPerSecond)
            };

            foreach (var aiChannel in aiAnimation.NodeAnimationChannels)
            {
                var channel = new AnimationChannel();

                // We can have different numbers of keyframes for each, so find the max index.
                var keyCount = Math.Max(aiChannel.PositionKeyCount, Math.Max(aiChannel.RotationKeyCount, aiChannel.ScalingKeyCount));

                // Get all unique keyframe times
                var times = aiChannel.PositionKeys.Select(k => k.Time)
                    .Union(aiChannel.RotationKeys.Select(k => k.Time))
                    .Union(aiChannel.ScalingKeys.Select(k => k.Time))
                    .Distinct().ToList();

                // The rest of this loop is almost certainly wrong. Don't trust it.
                // There's some magical combination, ordering, or transposition we have
                // to figure out to translate FBX->Assimp->XNA.
                // Possibilities: matrix offset transform, missing a base transform, an extra base transform, etc.

                var toBoneSpace = _objectToBone.ContainsKey(aiChannel.NodeName)
                    ? _objectToBone[aiChannel.NodeName] * _skeletonRoot.Transform
                    : _skeletonRoot.Transform;

                foreach (var aiKeyTime in times)
                {
                    var time = TimeSpan.FromSeconds(aiKeyTime / aiAnimation.TicksPerSecond);
                    var translation = Matrix4x4.FromTranslation(aiChannel.PositionKeys.FirstOrDefault(k => k.Time == aiKeyTime).Value);
                    var rotation = new Matrix4x4(aiChannel.RotationKeys.FirstOrDefault(k => k.Time == aiKeyTime).Value.GetMatrix());
                    var scale = Matrix4x4.FromScaling(aiChannel.ScalingKeys.FirstOrDefault(k => k.Time == aiKeyTime).Value);
                    var nodeTransform = translation * rotation * scale;

                    var xform = toBoneSpace * nodeTransform * _globalInverseXform;
                    channel.Add(new AnimationKeyframe(time, ToXna(xform)));
                }

                animation.Channels.Add(aiChannel.NodeName, channel);
            }

            return animation;
        }

        #region Conversion Helpers

        [DebuggerStepThrough]
        public static Matrix ToXna(Matrix4x4 matrix)
        {
            var result = Matrix.Identity;

            result.M11 = matrix.A1;
            result.M12 = matrix.B1;
            result.M13 = matrix.C1;
            result.M14 = matrix.D1;

            result.M21 = matrix.A2;
            result.M22 = matrix.B2;
            result.M23 = matrix.C2;
            result.M24 = matrix.D2;

            result.M31 = matrix.A3;
            result.M32 = matrix.B3;
            result.M33 = matrix.C3;
            result.M34 = matrix.D3;

            result.M41 = matrix.A4;
            result.M42 = matrix.B4;
            result.M43 = matrix.C4;
            result.M44 = matrix.D4;

            return result;
        }

        [DebuggerStepThrough]
        public static IEnumerable<Vector2> ToXna(IEnumerable<Vector2D> vectors)
        {
            foreach (var vector in vectors)
                yield return ToXna(vector);
        }

        [DebuggerStepThrough]
        public static IEnumerable<Vector3> ToXna(IEnumerable<Vector3D> vectors)
        {
            foreach (var vector in vectors)
                yield return ToXna(vector);
        }

        [DebuggerStepThrough]
        public static IEnumerable<Vector2> ToXnaTexCoord(IEnumerable<Vector3D> vectors)
        {
            foreach (var vector in vectors)
                yield return new Vector2(vector.X, vector.Y);
        }

        [DebuggerStepThrough]
        public static Vector2 ToXna(Vector2D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        [DebuggerStepThrough]
        public static Vector3 ToXna(Vector3D vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        [DebuggerStepThrough]
        public static Quaternion ToXna(Assimp.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        [DebuggerStepThrough]
        public static Vector3 ToXna(Assimp.Color4D color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        #endregion
    }
}
