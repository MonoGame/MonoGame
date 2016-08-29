// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalColorTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public Vector2 TextureCoordinate;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionNormalColorTexture(Vector3 position, Vector3 normal, Color color, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
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

        public override string ToString()
        {
            return "{{Position:" + Position + " Normal:" + Normal + " Color:" + Color + " TextureCoordinate:" + TextureCoordinate + "}}";
        }
        public bool Equals(VertexPositionNormalColorTexture other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal) && Color.Equals(other.Color) && TextureCoordinate.Equals(other.TextureCoordinate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexPositionNormalColorTexture && Equals((VertexPositionNormalColorTexture) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ Normal.GetHashCode();
                hashCode = (hashCode*397) ^ Color.GetHashCode();
                hashCode = (hashCode*397) ^ TextureCoordinate.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VertexPositionNormalColorTexture left, VertexPositionNormalColorTexture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionNormalColorTexture left, VertexPositionNormalColorTexture right)
        {
            return !(left == right);
        }

        static VertexPositionNormalColorTexture()
        {
            var elements = new [] 
            { 
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), 
                new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0), 
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) 
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}