using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    [SerializableAttribute]
    public struct VertexPositionColorTexture : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration;

        public Color Color;
        public Vector3 Position;
        public Vector2 TextureCoordinate;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
        {
            Color = color;
            Position = position;
            TextureCoordinate = textureCoordinate;
        }
       
        public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return !(left == right);
        }

        public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
        {
            return left.Color == right.Color && left.Position == right.Position &&
                   left.TextureCoordinate == right.TextureCoordinate;
        }


        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
