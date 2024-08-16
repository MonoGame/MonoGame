using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position,
    /// color, and one set of texture coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorTexture : IVertexType
    {
        /// <inheritdoc cref="VertexPosition.Position"/>
        public Vector3 Position;
        /// <inheritdoc cref="VertexPositionColor.Color"/>
        public Color Color;
        /// <inheritdoc cref="VertexPositionTexture.TextureCoordinate"/>
        public Vector2 TextureCoordinate;
        /// <inheritdoc cref="IVertexType.VertexDeclaration"/>
        public static readonly VertexDeclaration VertexDeclaration;

        /// <summary>
        /// Creates an instance of <see cref="VertexPositionColorTexture"/>.
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        /// <param name="color">Color of the vertex.</param>
        /// <param name="textureCoordinate">Texture coordinate of the vertex.</param>
        public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }
		
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc cref="VertexPosition.ToString()"/>
        public override string ToString()
        {
            return "{{Position:" + this.Position + " Color:" + this.Color + " TextureCoordinate:" + this.TextureCoordinate + "}}";
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionColorTexture"/> are equal
        /// </summary>
        /// <param name="left">The object on the left of the equality operator.</param>
        /// <param name="right">The object on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are the same; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionColorTexture"/> are different
        /// </summary>
        /// <param name="left">The object on the left of the inequality operator.</param>
        /// <param name="right">The object on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return !(left == right);
        }

        /// <inheritdoc cref="VertexPosition.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((VertexPositionColorTexture)obj));
        }

        static VertexPositionColorTexture()
        {
            var elements = new VertexElement[] 
            { 
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0), 
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) 
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
