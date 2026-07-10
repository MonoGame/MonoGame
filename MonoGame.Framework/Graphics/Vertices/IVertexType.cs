namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Vertex type interface which is implemented by a custom vertex type structure.
    /// </summary>
    public interface IVertexType
    {
        /// <summary>
        /// Vertex declaration, which defines per-vertex data.
        /// </summary>
        VertexDeclaration VertexDeclaration
        {
            get;
        }
    }
}
