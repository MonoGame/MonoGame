// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Model - MonoGame")]
    public class ModelProcessor : ContentProcessor<NodeContent, ModelContent>
    {
        private ContentIdentity _identity;

        #region Fields for default values

        private bool _colorKeyEnabled = true;
        private bool _generateMipmaps = true;
        private bool _premultiplyTextureAlpha = true;
        private bool _premultiplyVertexColors = true;
        private float _scale = 1.0f;
        private TextureProcessorOutputFormat _textureFormat = TextureProcessorOutputFormat.DxtCompressed;

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

		[DefaultValue(typeof(TextureProcessorOutputFormat), "DxtCompressed")]
        public virtual TextureProcessorOutputFormat TextureFormat
        {
            get { return _textureFormat; }
            set { _textureFormat = value; }
        }

        #endregion

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            _identity = input.Identity;

            // Perform the processor transforms.
            if (RotationX != 0.0f || RotationY != 0.0f || RotationZ != 0.0f || Scale != 1.0f)
            {
                var rotX = Matrix.CreateRotationX(MathHelper.ToRadians(RotationX));
                var rotY = Matrix.CreateRotationY(MathHelper.ToRadians(RotationY));
                var rotZ = Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ));
                var scale = Matrix.CreateScale(Scale);
                MeshHelper.TransformScene(input, rotZ * rotX * rotY * scale);
            }

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

            var boneList = new List<ModelBoneContent>();
            var meshList = new List<ModelMeshContent>();
            var rootNode = ProcessNode(input, null, boneList, meshList, context);

            return new ModelContent(rootNode, boneList, meshList);
        }

        private ModelBoneContent ProcessNode(NodeContent node, ModelBoneContent parent, List<ModelBoneContent> boneList, List<ModelMeshContent> meshList, ContentProcessorContext context)
        {
            var result = new ModelBoneContent(node.Name, boneList.Count, node.Transform, parent);
            boneList.Add(result);

            if (node is MeshContent)
                meshList.Add(ProcessMesh(node as MeshContent, result, context));

            var children = new List<ModelBoneContent>();
            foreach (var child in node.Children)
                children.Add(ProcessNode(child, result, boneList, meshList, context));
            result.Children = new ModelBoneContentCollection(children);

            return result;
        }

        private ModelMeshContent ProcessMesh(MeshContent mesh, ModelBoneContent parent, ContentProcessorContext context)
        {
            var parts = new List<ModelMeshPartContent>();
            var vertexBuffer = new VertexBufferContent();
            var indexBuffer = new IndexCollection();
			
			if (GenerateTangentFrames)
            {
                context.Logger.LogMessage("Generating tangent frames.");
                foreach (GeometryContent geom in mesh.Geometry)
                {
                    if (!geom.Vertices.Channels.Contains(VertexChannelNames.Normal(0)))
                    {
                        MeshHelper.CalculateNormals(geom, true);
                    }

                    if(!geom.Vertices.Channels.Contains(VertexChannelNames.Tangent(0)) ||
                        !geom.Vertices.Channels.Contains(VertexChannelNames.Binormal(0)))
                    {
                        MeshHelper.CalculateTangentFrames(geom, VertexChannelNames.TextureCoordinate(0), VertexChannelNames.Tangent(0),
                            VertexChannelNames.Binormal(0));
                    }
                }
            }

            var startVertex = 0;
            foreach (var geometry in mesh.Geometry)
            {
                var vertices = geometry.Vertices;
                var vertexCount = vertices.VertexCount;
                ModelMeshPartContent partContent;
                if (vertexCount == 0)
                    partContent = new ModelMeshPartContent();
                else
                {
                    var geomBuffer = geometry.Vertices.CreateVertexBuffer();
                    vertexBuffer.Write(vertexBuffer.VertexData.Length, 1, geomBuffer.VertexData);

                    var startIndex = indexBuffer.Count;
                    indexBuffer.AddRange(geometry.Indices);

                    partContent = new ModelMeshPartContent(vertexBuffer, indexBuffer, startVertex, vertexCount, startIndex, geometry.Indices.Count / 3);

                    // Geoms are supposed to all have the same decl, so just steal one of these
                    vertexBuffer.VertexDeclaration = geomBuffer.VertexDeclaration;

                    startVertex += vertexCount;
                }

                partContent.Material = geometry.Material;
                parts.Add(partContent);
            }

            var bounds = new BoundingSphere();
            if (mesh.Positions.Count > 0)
                bounds = BoundingSphere.CreateFromPoints(mesh.Positions);

            return new ModelMeshContent(mesh.Name, mesh, parent, bounds, parts);
        }

        protected virtual MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            var parameters = new OpaqueDataDictionary();
            parameters.Add("ColorKeyColor", ColorKeyColor);
            parameters.Add("ColorKeyEnabled", ColorKeyEnabled);
            parameters.Add("GenerateMipmaps", GenerateMipmaps);
            parameters.Add("PremultiplyTextureAlpha", PremultiplyTextureAlpha);
            parameters.Add("ResizeTexturesToPowerOfTwo", ResizeTexturesToPowerOfTwo);
            parameters.Add("TextureFormat", TextureFormat);
            parameters.Add("DefaultEffect", DefaultEffect);

            return context.Convert<MaterialContent, MaterialContent>(material, "MaterialProcessor", parameters);
        }

        protected virtual void ProcessGeometryUsingMaterial(MaterialContent material,
                                                            IEnumerable<GeometryContent> geometryCollection,
                                                            ContentProcessorContext context)
        {
            // If we don't get a material then assign a default one.
            if (material == null)
                material = MaterialProcessor.CreateDefaultMaterial(DefaultEffect);

            // Test requirements from the assigned material.
            int textureChannels;
            bool vertexWeights = false;
            if (material is DualTextureMaterialContent)
            {
                textureChannels = 2;
            }
            else if (material is SkinnedMaterialContent)
            {
                textureChannels = 1;
                vertexWeights = true;
            }
            else if (material is EnvironmentMapMaterialContent)
            {
                textureChannels = 1;
            }
            else if (material is AlphaTestMaterialContent)
            {
                textureChannels = 1;
            }
            else
            {
                // Just check for a "Texture" which should cover custom Effects
                // and BasicEffect which can have an optional texture.
                textureChannels = material.Textures.ContainsKey("Texture") ? 1 : 0;                
            }

            // By default we must set the vertex color property
            // to match XNA behavior.
            material.OpaqueData["VertexColorEnabled"] = false;

            // If we run into a geometry that requires vertex
            // color we need a seperate material for it.
            var colorMaterial = material.Clone();
            colorMaterial.OpaqueData["VertexColorEnabled"] = true;    

            foreach (var geometry in geometryCollection)
            {
                // Process the geometry.
                for (var i = 0; i < geometry.Vertices.Channels.Count; i++)
                    ProcessVertexChannel(geometry, i, context);

                // Verify we have the right number of texture coords.
                for (var i = 0; i < textureChannels; i++)
                {
                    if (!geometry.Vertices.Channels.Contains(VertexChannelNames.TextureCoordinate(i)))
                        throw new InvalidContentException(
                            string.Format("The mesh \"{0}\", using {1}, contains geometry that is missing texture coordinates for channel {2}.", 
                            geometry.Parent.Name,
                            MaterialProcessor.GetDefaultEffect(material),
                            i),
                            _identity);
                }

                // Do we need to enable vertex color?
                if (geometry.Vertices.Channels.Contains(VertexChannelNames.Color(0)))
                    geometry.Material = colorMaterial;
                else
                    geometry.Material = material;

                // Do we need vertex weights?
                if (vertexWeights)
                {
                    var weightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, 0);
                    if (!geometry.Vertices.Channels.Contains(weightsName))
                        throw new InvalidContentException(
                            string.Format("The skinned mesh \"{0}\" contains geometry without any vertex weights.", geometry.Parent.Name),
                            _identity);                    
                }
            }
        }

        protected virtual void ProcessVertexChannel(GeometryContent geometry,
                                                    int vertexChannelIndex,
                                                    ContentProcessorContext context)
        {
            var channel = geometry.Vertices.Channels[vertexChannelIndex];

            // TODO: According to docs, channels with VertexElementUsage.Color -> Color

            // Channels[VertexChannelNames.Weights] -> { Byte4 boneIndices, Color boneWeights }
            if (channel.Name.StartsWith(VertexChannelNames.Weights()))
                ProcessWeightsChannel(geometry, vertexChannelIndex, _identity);
        }

        private static void ProcessWeightsChannel(GeometryContent geometry, int vertexChannelIndex, ContentIdentity identity)
        {
            // NOTE: Portions of this code is from the XNA CPU Skinning 
            // sample under Ms-PL, (c) Microsoft Corporation.

            // create a map of Name->Index of the bones
            var skeleton = MeshHelper.FindSkeleton(geometry.Parent);
            if (skeleton == null)
            {
                throw new InvalidContentException(
                    "Skeleton not found. Meshes that contain a Weights vertex channel cannot be processed without access to the skeleton data.",
                    identity);                     
            }

            var boneIndices = new Dictionary<string, int>();
            var flattenedBones = MeshHelper.FlattenSkeleton(skeleton);
            for (var i = 0; i < flattenedBones.Count; i++)
                boneIndices.Add(flattenedBones[i].Name, i);

            var vertexChannel = geometry.Vertices.Channels[vertexChannelIndex];
            var inputWeights = vertexChannel as VertexChannel<BoneWeightCollection>;
            if (inputWeights == null)
            {
                throw new InvalidContentException(
                    string.Format(
                        "Vertex channel \"{0}\" is the wrong type. It has element type {1}. Type {2} is expected.",
                        vertexChannel.Name,
                        vertexChannel.ElementType.FullName,
                        "Microsoft.Xna.Framework.Content.Pipeline.Graphics.BoneWeightCollection"),
                    identity);                          
            }
            var outputIndices = new Byte4[inputWeights.Count];
            var outputWeights = new Vector4[inputWeights.Count];
            for (var i = 0; i < inputWeights.Count; i++)
                ConvertWeights(inputWeights[i], boneIndices, outputIndices, outputWeights, i);

            // create our new channel names
            var usageIndex = VertexChannelNames.DecodeUsageIndex(inputWeights.Name);
            var indicesName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, usageIndex);
            var weightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, usageIndex);

            // add in the index and weight channels
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 1, indicesName, outputIndices);
            geometry.Vertices.Channels.Insert(vertexChannelIndex + 2, weightsName, outputWeights);

            // remove the original weights channel
            geometry.Vertices.Channels.RemoveAt(vertexChannelIndex);
        }

        // From the XNA CPU Skinning Sample under Ms-PL, (c) Microsoft Corporation
        private static void ConvertWeights(BoneWeightCollection weights, Dictionary<string, int> boneIndices, Byte4[] outIndices, Vector4[] outWeights, int vertexIndex)
        {
            // we only handle 4 weights per bone
            const int maxWeights = 4;

            // create some temp arrays to hold our values
            var tempIndices = new int[maxWeights];
            var tempWeights = new float[maxWeights];

            // cull out any extra bones
            weights.NormalizeWeights(maxWeights);

            // get our indices and weights
            for (var i = 0; i < weights.Count; i++)
            {
                var weight = weights[i];

                if (!boneIndices.ContainsKey(weight.BoneName))
                {
                    var errorMessage = string.Format("Bone '{0}' was not found in the skeleton! Skeleton bones are: '{1}'.", weight.BoneName, string.Join("', '", boneIndices.Keys));
                    throw new Exception(errorMessage);
                }

                tempIndices[i] = boneIndices[weight.BoneName];
                tempWeights[i] = weight.Weight;
            }

            // zero out any remaining spaces
            for (var i = weights.Count; i < maxWeights; i++)
            {
                tempIndices[i] = 0;
                tempWeights[i] = 0;
            }

            // output the values
            outIndices[vertexIndex] = new Byte4(tempIndices[0], tempIndices[1], tempIndices[2], tempIndices[3]);
            outWeights[vertexIndex] = new Vector4(tempWeights[0], tempWeights[1], tempWeights[2], tempWeights[3]);
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
