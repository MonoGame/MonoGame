using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct VertexPositionTexture : IVertexType
    {
        /// <inheritdoc cref="VertexPosition.Position"/>
        public Vector3 Position;
        /// <summary>
        /// UV texture coordinates.
        /// </summary>
        public Vector2 TextureCoordinate;
        /// <inheritdoc cref="IVertexType.VertexDeclaration"/>
        public static readonly VertexDeclaration VertexDeclaration;

        /// <summary>
        /// Creates an instance of <see cref="VertexPositionTexture"/>.
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        /// <param name="textureCoordinate">Texture coordinate of the vertex.</param>
        public VertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
        {
            this.Position = position;
            this.TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        /// <inheritdoc cref="VertexPosition.GetHashCode()"/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ TextureCoordinate.GetHashCode();
            }
        }

        /// <inheritdoc cref="VertexPosition.ToString()"/>
        public override string ToString()
        {
            return "{{Position:" + this.Position + " TextureCoordinate:" + this.TextureCoordinate + "}}";
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionTexture"/> are equal
        /// </summary>
        /// <param name="left">The vertex on the left of the equality operator.</param>
        /// <param name="right">The vertex on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the vertices are the same; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(VertexPositionTexture left, VertexPositionTexture right)
        {
            return ((left.Position == right.Position) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionTexture"/> are different
        /// </summary>
        /// <param name="left">The vertex on the left of the equality operator.</param>
        /// <param name="right">The vertex on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the vertices are different; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator !=(VertexPositionTexture left, VertexPositionTexture right)
        {
            return !(left == right);
        }

        /// <inheritdoc cref="VertexPosition.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((VertexPositionTexture)obj));
        }

        static VertexPositionTexture()
        {
            VertexElement[] elements = new VertexElement[] { new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }

    }
}
