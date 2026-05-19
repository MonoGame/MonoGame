using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position and color.
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColor : IVertexType
	{
        /// <inheritdoc cref="VertexPosition.Position"/>
        [DataMember]
		public Vector3 Position;

        /// <summary>
        /// The vertex color.
        /// </summary>
        [DataMember]
		public Color Color;

        /// <inheritdoc cref="IVertexType.VertexDeclaration"/>
		public static readonly VertexDeclaration VertexDeclaration;

        /// <summary>
        /// Creates an instance of <see cref="VertexPositionColor"/>.
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        /// <param name="color">Color of the vertex.</param>
        public VertexPositionColor(Vector3 position, Color color)
		{
			this.Position = position;
			Color = color;
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
	            return (Position.GetHashCode() * 397) ^ Color.GetHashCode();
	        }
	    }

        /// <inheritdoc cref="VertexPosition.ToString()"/>
	    public override string ToString()
		{
            return "{{Position:" + this.Position + " Color:" + this.Color + "}}";
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionColor"/> are equal
        /// </summary>
        /// <param name="left">The object on the left of the equality operator.</param>
        /// <param name="right">The object on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are the same; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator == (VertexPositionColor left, VertexPositionColor right)
		{
			return ((left.Color == right.Color) && (left.Position == right.Position));
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPositionColor"/> are different
        /// </summary>
        /// <param name="left">The object on the left of the inequality operator.</param>
        /// <param name="right">The object on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator != (VertexPositionColor left, VertexPositionColor right)
		{
			return !(left == right);
		}

        /// <inheritdoc cref="VertexPosition.Equals(object)"/>
		public override bool Equals(object obj)
		{
			if (obj == null) {
				return false;
			}
			if (obj.GetType () != base.GetType ()) {
				return false;
			}
			return (this == ((VertexPositionColor)obj));
		}

		static VertexPositionColor()
		{
			VertexElement[] elements = new VertexElement[] { new VertexElement (0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement (12, VertexElementFormat.Color, VertexElementUsage.Color, 0) };
			VertexDeclaration declaration = new VertexDeclaration (elements);
			VertexDeclaration = declaration;
		}
	}
}
