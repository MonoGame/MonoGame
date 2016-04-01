// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines a single element in a vertex.
    /// </summary>
    public partial struct VertexElement : IEquatable<VertexElement>
    {
        private int _offset;
        private VertexElementFormat _format;
        private VertexElementUsage _usage;
        private int _usageIndex;

        /// <summary>
        /// Gets or sets the offset in bytes from the beginning of the stream to the vertex element.
        /// </summary>
        /// <value>The offset in bytes.</value>
        public int Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        /// <summary>
        /// Gets or sets the data format.
        /// </summary>
        /// <value>The data format.</value>
        public VertexElementFormat VertexElementFormat
        {
            get { return _format; }
            set { _format = value; }
        }

        /// <summary>
        /// Gets or sets the HLSL semantic of the element in the vertex shader input.
        /// </summary>
        /// <value>The HLSL semantic of the element in the vertex shader input.</value>
        public VertexElementUsage VertexElementUsage
        {
            get { return _usage; }
            set { _usage = value; }
        }

        /// <summary>
        /// Gets or sets the semantic index.
        /// </summary>
        /// <value>
        /// The semantic index, which is required if the semantic is used for more than one vertex
        /// element.
        /// </value>
        /// <remarks>
        /// Usage indices in a vertex declaration usually start with 0. When multiple vertex buffers
        /// are bound to the input assembler stage (see <see cref="GraphicsDevice.SetVertexBuffers"/>),
        /// MonoGame internally adjusts the usage indices based on the order in which the vertex
        /// buffers are bound.
        /// </remarks>
        public int UsageIndex
        {
            get { return _usageIndex; }
            set { _usageIndex = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexElement"/> struct.
        /// </summary>
        /// <param name="offset">The offset in bytes from the beginning of the stream to the vertex element.</param>
        /// <param name="elementFormat">The element format.</param>
        /// <param name="elementUsage">The HLSL semantic of the element in the vertex shader input-signature.</param>
        /// <param name="usageIndex">The semantic index, which is required if the semantic is used for more than one vertex element.</param>
        public VertexElement(int offset, VertexElementFormat elementFormat, VertexElementUsage elementUsage, int usageIndex)
        {
            _offset = offset;
            _format = elementFormat;
            _usageIndex = usageIndex;
            _usage = elementUsage;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode

            // Optimized hash:
            // - DirectX 11 has max 32 registers. A register is max 16 byte. _offset is in the range
            //   0 to 512 (exclusive). --> _offset needs 9 bit.
            // - VertexElementFormat has 12 values. --> _format needs 4 bit.
            // - VertexElementUsage has 13 values. --> _usage needs 4 bit.
            // - DirectX 11 has max 32 registers. --> _usageIndex needs 6 bit.
            // (Note: If these assumptions are correct we get a unique hash code. If these 
            // assumptions are not correct, we still get a useful hash code because we use XOR.)
            int hashCode = _offset;
            hashCode ^= (int)_format << 9;
            hashCode ^= (int)_usage << (9 + 4);
            hashCode ^= _usageIndex << (9 + 4 + 4);
            return hashCode;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "{Offset:" + _offset + " Format:" + _format + " Usage:" + _usage + " UsageIndex: " + _usageIndex + "}";
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is VertexElement && Equals((VertexElement)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="VertexElement"/> is equal to this
        /// instance.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="VertexElement"/> is equal to this
        /// instance; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(VertexElement other)
        {
            return _offset == other._offset
                   && _format == other._format
                   && _usage == other._usage
                   && _usageIndex == other._usageIndex;
        }

        /// <summary>
        /// Compares two <see cref="VertexElement"/> instances to determine whether they are the
        /// same.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// the same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(VertexElement left, VertexElement right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="VertexElement"/> instances to determine whether they are
        /// different.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// the different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(VertexElement left, VertexElement right)
        {
            return !left.Equals(right);
        }
    }
}
