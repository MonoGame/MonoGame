using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	// This should really be XNA's VertexPositionColorTexture
	// but I'm not sure we want to use Vector3s if we don't have to.
    internal struct VertexPosition2ColorTexture : IVertexType
	{
		public Vector2 Position;
		public Color Color;
		public Vector2 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration;
		
		public VertexPosition2ColorTexture ( Vector2 position, Color color, Vector2 texCoord )
		{
			Position = position;
			Color = color;
			TextureCoordinate = texCoord;
		}
		
		public static int GetSize()
		{
				return sizeof(float)*4+sizeof(uint);
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
            // TODO: FIc gethashcode
            return 0;
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position + " Color:" + this.Color + " TextureCoordinate:" + this.TextureCoordinate + "}}";
        }

        public static bool operator ==(VertexPosition2ColorTexture left, VertexPosition2ColorTexture right)
        {
            return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.TextureCoordinate == right.TextureCoordinate));
        }

        public static bool operator !=(VertexPosition2ColorTexture left, VertexPosition2ColorTexture right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != base.GetType())
                return false;

            return (this == ((VertexPosition2ColorTexture)obj));
        }

        static VertexPosition2ColorTexture()
        {

            var elements = new VertexElement[] 
            { 
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0), 
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0), 
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0) 
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
 
}

