// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    partial class VertexBufferBindings
    {
        /// <summary>
        /// Creates an <see cref="ImmutableVertexInputLayout"/> that can be used as a key in the
        /// <see cref="InputLayoutCache"/>.
        /// </summary>
        /// <returns>The <see cref="ImmutableVertexInputLayout"/>.</returns>
        public ImmutableVertexInputLayout ToImmutable()
        {
            int count = Count;

            var vertexDeclarations = new VertexDeclaration[count];
            Array.Copy(VertexDeclarations, vertexDeclarations, count);

            var instanceFrequencies = new int[count];
            Array.Copy(InstanceFrequencies, instanceFrequencies, count);

            return new ImmutableVertexInputLayout(vertexDeclarations, instanceFrequencies);
        }
    }
}
