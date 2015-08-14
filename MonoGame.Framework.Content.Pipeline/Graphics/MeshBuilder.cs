using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public sealed class MeshBuilder
    {
        private readonly MeshContent _meshContent;
        private GeometryContent _currentGeometryContent;

        /// <summary>
        /// Gets or sets the current value for position merging of the mesh.
        /// </summary>
        public bool MergeDuplicatePositions { get; set; }

        /// <summary>
        /// Gets or sets the tolerance for <see cref="MergeDuplicatePositions"/>.
        /// </summary>
        public float MergePositionTolerance { get; set; }

        /// <summary>
        /// Gets or sets the name of the current  <see cref="MeshContent"/> object being processed.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Reverses the triangle winding order of the specified mesh.
        /// </summary>
        public bool SwapWindingOrder { get; set; }


        private MeshBuilder(string name)
        {
            Name = name;
            _meshContent = new MeshContent();
        }

        /// <summary>
        /// Adds a vertex into the index collection.
        /// </summary>
        /// <param name="indexIntoVertexCollection">Index of the inserted vertex, in the collection. This corresponds to the value returned by <see cref="CreatePosition"/>.</param>
        public void AddTriangleVertex(int indexIntoVertexCollection)
        {
            _currentGeometryContent.Vertices.Add(indexIntoVertexCollection);
        }


        public int CreateVertexChannel<T>(string usage)
        {
            _currentGeometryContent.Vertices.Channels.Add<T>(usage, null);
            return _currentGeometryContent.Vertices.Channels.Count - 1;
        }

        /// <summary>
        /// Inserts the specified vertex position into the vertex channel.
        /// </summary>
        /// <param name="x">Value of the x component of the vector.</param>
        /// <param name="y">Value of the y component of the vector.</param>
        /// <param name="z">Value of the z component of the vector.</param>
        /// <returns>Index of the inserted vertex.</returns>
        public int CreatePosition(float x, float y, float z)
        {
            return CreatePosition(new Vector3(x, y, z));
        }

        /// <summary>
        /// Inserts the specified vertex position into the vertex channel at the specified index.
        /// </summary>
        /// <param name="pos">Value of the vertex being inserted.</param>
        /// <returns>Index of the vertex being inserted.</returns>
        public int CreatePosition(Vector3 pos)
        {
            _meshContent.Positions.Add(pos);
            return _meshContent.Positions.Count - 1;
        }

        /// <summary>
        /// Ends the creation of a mesh.
        /// </summary>
        /// <returns>Resultant mesh.</returns>
        public MeshContent FinishMesh()
        {
            if (MergeDuplicatePositions)
                MeshHelper.MergeDuplicatePositions(_meshContent, MergePositionTolerance);

            MeshHelper.MergeDuplicateVertices(_meshContent);
            MeshHelper.CalculateNormals(_meshContent, false);

            if (SwapWindingOrder)
                MeshHelper.SwapWindingOrder(_meshContent);

            return _meshContent;

            //TODO: The related MeshBuilder object can no longer be used after this function is called.
        }

        /// <summary>
        /// Specifies the material used by the current mesh.
        /// </summary>
        /// <remarks>Sets the material used by the triangle being defined next. This material, in conjunction with SetOpaqueData, determines the GeometryContent object containing the next triangle. MeshBuilder maintains the material value for all future triangles. Therefore, if multiple triangles share the same material, you need only one call to SetMaterial.</remarks>
        /// <param name="material">The material to be used by the mesh.</param>
        public void SetMaterial(MaterialContent material)
        {
            _currentGeometryContent = new GeometryContent();
            _meshContent.Geometry.Add(_currentGeometryContent);
            _currentGeometryContent.Material = material;
        }

        /// <summary>
        /// Initializes the opaque data for a specific mesh object.
        /// </summary>
        /// <param name="opaqueData">Opaque data to be applied to the GeometryContent object of the next triangle.</param>
        public void SetOpaqueData(OpaqueDataDictionary opaqueData)
        {
            _currentGeometryContent = new GeometryContent();
            _meshContent.Geometry.Add(_currentGeometryContent);
            _currentGeometryContent.OpaqueData.Add("default", opaqueData);
        }

        /// <summary>
        /// Sets the specified vertex data with new data.
        /// </summary>
        /// <param name="vertexDataIndex">Index of the vertex data channel being set. This should match the index returned by CreateVertexChannel.</param>
        /// <param name="channelData">New data values for the vertex data. The data type being set must match the data type for the vertex channel specified by vertexDataIndex.</param>
        public void SetVertexChannelData(int vertexDataIndex, Object channelData)
        {
            _currentGeometryContent.Vertices.Channels[vertexDataIndex].Items.Add(channelData);
        }

        /// <summary>
        /// Initializes the creation of a mesh.
        /// </summary>
        /// <param name="name">Name of the mesh.</param>
        /// <returns>Object used when building the mesh.</returns>
        public static MeshBuilder StartMesh(string name)
        {
            return new MeshBuilder(name);
        }
    }
}
