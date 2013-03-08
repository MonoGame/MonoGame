// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Model - MonoGame")]
    public class ModelProcessor : ContentProcessor<NodeContent, ModelContent>
    {
        private readonly List<ModelMeshContent> _meshes = new List<ModelMeshContent>();

        private ContentIdentity _identity;
        private ContentBuildLogger _logger;

        #region Fields for default values

        private bool _colorKeyEnabled = true;
        private bool _generateMipmaps = true;
        private bool _premultiplyTextureAlpha = true;
        private bool _premultiplyVertexColors = true;
        private float _scale = 1.0f;
        private TextureProcessorOutputFormat _textureFormat = TextureProcessorOutputFormat.DXTCompressed;

        #endregion

        public ModelProcessor() { }

        #region Properties

        public virtual Color ColorKeyColor { get; set; }

        [DefaultValue(true)]
        public virtual bool ColorKeyEnabled
        {
            get { return _colorKeyEnabled; }
            set { _colorKeyEnabled = value; }
        }

        public virtual MaterialProcessorDefaultEffect DefaultEffect { get; set; }

        [DefaultValue(true)]
        public virtual bool GenerateMipmaps
        {
            get { return _generateMipmaps; }
            set { _generateMipmaps = value; }
        }

        public virtual bool GenerateTangentFrames { get; set; }

        [DefaultValue(true)]
        public virtual bool PremultiplyTextureAlpha
        {
            get { return _premultiplyTextureAlpha; }
            set { _premultiplyTextureAlpha = value; }
        }

        [DefaultValue(true)]
        public virtual bool PremultiplyVertexColors
        {
            get { return _premultiplyVertexColors; }
            set { _premultiplyVertexColors = value; }
        }

        public virtual bool ResizeTexturesToPowerOfTwo { get; set; }

        public virtual float RotationX { get; set; }

        public virtual float RotationY { get; set; }

        public virtual float RotationZ { get; set; }

        [DefaultValue(1.0f)]
        public virtual float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public virtual bool SwapWindingOrder { get; set; }

        [DefaultValue(typeof(TextureProcessorOutputFormat), "DXTCompressed")]
        public virtual TextureProcessorOutputFormat TextureFormat
        {
            get { return _textureFormat; }
            set { _textureFormat = value; }
        }

        #endregion

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            _identity = input.Identity;
            _logger = context.Logger;

            // Gather all the nodes in tree traversal order.
            var nodes = input.AsEnumerable().SelectDeep(n => n.Children).ToList();

            var meshes = nodes.FindAll(n => n is MeshContent).Cast<MeshContent>().ToList();
            var geometries = meshes.SelectMany(m => m.Geometry).ToList();
            var distinctMaterials = geometries.Select(g => g.Material).Distinct().ToList();

            // Loop through all distinct materials, passing them through the conversion method
            // only once, and then processing all geometries using that material.
            foreach (var inputMaterial in distinctMaterials)
            {
                var geomsWithMaterial = geometries.Where(g => g.Material == inputMaterial).ToList();
                var material = ConvertMaterial(inputMaterial, context);

                ProcessGeometryUsingMaterial(material, geomsWithMaterial, context);
            }

            // Hierarchy
            var bones = nodes.OfType<BoneContent>().ToList();
            var modelBones = new List<ModelBoneContent>();
            for (var i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];

                // Find the parent
                var parentIndex = bones.IndexOf(bone.Parent as BoneContent);
                ModelBoneContent parent = null;
                if (parentIndex > -1)
                    parent = modelBones[parentIndex];

                modelBones.Add(new ModelBoneContent(bone.Name, i, bone.Transform, parent));
            }

            foreach (var bone in modelBones)
                bone.Children = new ModelBoneContentCollection(modelBones.FindAll(b => b.Parent == bone));

            return new ModelContent(modelBones[0], modelBones, _meshes);
        }

        protected virtual MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            // Do nothing for now
            return material;
        }

        protected virtual void ProcessGeometryUsingMaterial(MaterialContent material,
                                                            IEnumerable<GeometryContent> geometryCollection,
                                                            ContentProcessorContext context)
        {
            if (material == null)
                material = new BasicMaterialContent();

            foreach (var geometry in geometryCollection)
            {
                ProcessBasicMaterial(material as BasicMaterialContent, geometry);

                var vertexBuffer = geometry.Vertices.CreateVertexBuffer();
                var primitiveCount = geometry.Vertices.PositionIndices.Count;
                var parts = new List<ModelMeshPartContent>
                    {
                        new ModelMeshPartContent(vertexBuffer, geometry.Indices, 0, primitiveCount, 0,
                                                 primitiveCount / 3)
                    };

                var parent = geometry.Parent;
                var bounds = BoundingSphere.CreateFromPoints(geometry.Vertices.Positions);
                _meshes.Add(new ModelMeshContent(parent.Name, geometry.Parent, null, bounds, parts));
            }
        }

        protected virtual void ProcessVertexChannel(GeometryContent content,
                                                    int vertexChannelIndex,
                                                    ContentProcessorContext context)
        {
            // Channels with VertexElementUsage.Color -> Color
            // Channels[VertexChannelNames.Weights] -> { Byte4 boneIndices, Color boneWeights }

            throw new NotImplementedException();
        }

        private void ProcessBasicMaterial(BasicMaterialContent basicMaterial, GeometryContent geometry)
        {
            if (basicMaterial == null)
                return;

            // If the basic material specifies a texture, geometry must have coordinates.
            if (!geometry.Vertices.Channels.Contains(VertexChannelNames.TextureCoordinate(0)))
                throw new InvalidContentException(
                    "Geometry references material with texture, but no texture coordinates were found.",
                    _identity);

            // Enable vertex color if the geometry has the channel to support it.
            if (geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
                basicMaterial.VertexColorEnabled = true;
        }
    }

    internal static class ModelEnumerableExtensions
    {
        /// <summary>
        /// Returns each element of a tree structure in hierarchical order.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The enumeration to traverse.</param>
        /// <param name="selector">A function which returns the children of the element.</param>
        /// <returns>An IEnumerable whose elements are in tree structure heriarchical order.</returns>
        public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var stack = new Stack<T>(source.Reverse());
            while (stack.Count > 0)
            {
                // Return the next item on the stack.
                var item = stack.Pop();
                yield return item;

                // Get the children from this item.
                var children = selector(item);

                // If we have no children then skip it.
                if (children == null)
                    continue;

                // We're using a stack, so we need to push the
                // children on in reverse to get the correct order.
                foreach (var child in children.Reverse())
                    stack.Push(child);
            }
        }

        /// <summary>
        /// Returns an enumerable from a single element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}
