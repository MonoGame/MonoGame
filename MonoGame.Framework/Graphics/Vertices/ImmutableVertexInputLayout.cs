// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if DIRECTX
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Immutable version of <see cref="VertexInputLayout"/>. Can be used as a key in the
    /// <see cref="InputLayoutCache"/>.
    /// </summary>
    internal sealed class ImmutableVertexInputLayout : VertexInputLayout
    {
        private readonly int _hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableVertexInputLayout"/> class.
        /// </summary>
        /// <param name="vertexDeclarations">The vertex declarations per resource slot.</param>
        /// <param name="instanceFrequencies">The instance frequencies per resource slot.</param>
        /// <remarks>
        /// The specified arrays are stored internally - the arrays are not copied.
        /// </remarks>
        public ImmutableVertexInputLayout(VertexDeclaration[] vertexDeclarations, int[] instanceFrequencies)
            : base(vertexDeclarations, instanceFrequencies, vertexDeclarations.Length)
        {
            Debug.Assert(VertexDeclarations.Length == Count);
            Debug.Assert(InstanceFrequencies.Length == Count);

            // Pre-calculate hash code for fast lookup in dictionary.
            _hashCode = base.GetHashCode();
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
            return _hashCode;
        }
    }
}
#endif
