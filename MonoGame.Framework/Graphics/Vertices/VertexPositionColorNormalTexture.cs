using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionColorNormalTexture(Vector3 position, Color color, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position + " Color:" + this.Color + " Normal:" + this.Normal + " TextureCoordinate:" + this.TextureCoordinate + "}}";
        }

        public static bool operator ==(VertexPositionColorNormalTexture left, VertexPositionColorNormalTexture right)
        {
            return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.Normal == right.Normal) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        public static bool operator !=(VertexPositionColorNormalTexture left, VertexPositionColorNormalTexture right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((VertexPositionColorNormalTexture)obj));
        }

        static VertexPositionColorNormalTexture()
        {
            var elements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}
