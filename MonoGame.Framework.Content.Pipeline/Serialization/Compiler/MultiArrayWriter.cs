// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the array value to the output.
    /// </summary>
    [ContentTypeWriter]
    class MultiArrayWriter<T> : BuiltInContentWriter<Array>
    {
        ContentTypeWriter _elementWriter;

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            _elementWriter = output.GetTypeWriter(typeof(T));
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return string.Concat(typeof(ContentTypeReader).Namespace,
                                    ".",
                                    "MultiArrayReader`1[[",
                                    _elementWriter.GetRuntimeType(targetPlatform),
                                    "]]");
        }

        protected internal override void Write(ContentWriter output, Array value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var rank = value.Rank;

            // Dimension sizes
            output.Write(rank);
            for (int dimension = 0; dimension < rank; dimension++)
                output.Write(value.GetLength(dimension));

            // Values
            var indices = new int[rank];
            for (int i = 0; i < value.Length; i++)
            {
                CalcIndices(value, i, indices);
                output.WriteObject(value.GetValue(indices), _elementWriter);
            }
        }

        static void CalcIndices(Array array, int index, int[] indices)
        {
            if (array.Rank != indices.Length)
                throw new Exception("indices");

            for (int d = 0; d < indices.Length; d++)
            {
                if (index == 0)
                    indices[d] = 0;
                else
                {
                    indices[d] = index % array.GetLength(d);
                    index /= array.GetLength(d);
                }
            }

            if (index != 0)
                throw new ArgumentOutOfRangeException("index");
        }
    }
}
