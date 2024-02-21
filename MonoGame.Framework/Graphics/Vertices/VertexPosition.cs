// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes a custom vertex format structure that contains position.
    /// </summary>
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPosition : IVertexType
	{
        /// <summary>
        /// The XYZ vertex position.
        /// </summary>
        [DataMember]
		public Vector3 Position;
        /// <inheritdoc cref="IVertexType.VertexDeclaration"/>
		public static readonly VertexDeclaration VertexDeclaration;

        /// <summary>
        /// Creates an instance of <see cref="VertexPosition"/>.
        /// </summary>
        /// <param name="position">Position of the vertex.</param>
        public VertexPosition(Vector3 position)
		{
			Position = position;
		}

		VertexDeclaration IVertexType.VertexDeclaration
        {
			get { return VertexDeclaration; }
		}

        /// <inheritdoc/>
        public override int GetHashCode()
	    {
	        return Position.GetHashCode();
	    }

        /// <summary>
        /// Retrieves a string representation of this object.
        /// </summary>
        /// <returns>String representation of this object.</returns>
        public override string ToString()
		{
            return "{{Position:" + Position + "}}";
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPosition"/> are equal
        /// </summary>
        /// <param name="left">The vertex on the left of the equality operator.</param>
        /// <param name="right">The vertex on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the vertices are the same; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator == (VertexPosition left, VertexPosition right)
		{
			return left.Position == right.Position;
		}

        /// <summary>
        /// Returns a value that indicates whether two <see cref="VertexPosition"/> are different
        /// </summary>
        /// <param name="left">The vertex on the left of the equality operator.</param>
        /// <param name="right">The vertex on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if the vertices are different; <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator != (VertexPosition left, VertexPosition right)
		{
			return !(left == right);
		}

        /// <summary>
        /// Compares an object with the vertex.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the object is equal to the current vertex; <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return this == (VertexPosition) obj;
        }

        static VertexPosition()
		{
			VertexElement[] elements = { new VertexElement (0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0) };
            VertexDeclaration declaration = new VertexDeclaration(elements);
			VertexDeclaration = declaration;
		}
	}
}
