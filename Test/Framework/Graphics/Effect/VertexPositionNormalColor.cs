// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Graphics.Effect
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalColor : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;
        public static readonly VertexDeclaration VertexDeclaration;

        public VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color)
        {
            Position = position;
            Normal = normal;
            Color = color;
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
            return "{{Position:" + Position + " Normal:" + Normal + " Color:" + Color + "}}";
        }

        public bool Equals(VertexPositionNormalColor other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal) && Color.Equals(other.Color);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VertexPositionNormalColor && Equals((VertexPositionNormalColor) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode*397) ^ Normal.GetHashCode();
                hashCode = (hashCode*397) ^ Color.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VertexPositionNormalColor left, VertexPositionNormalColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexPositionNormalColor left, VertexPositionNormalColor right)
        {
            return !(left == right);
        }

        static VertexPositionNormalColor()
        {
            var elements = new [] 
            { 
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), 
                new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0), 
            };
            VertexDeclaration = new VertexDeclaration(elements);
        }
    }
}