// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class VertexDeclarationWriter : BuiltInContentWriter<VertexDeclarationContent>
    {
        protected internal override void Write(ContentWriter output, VertexDeclarationContent value)
        {
            // If fpr whatever reason there isn't a vertex stride defined, it's going to
            // cause problems after reading it in, so better to fail early here.
            output.Write((uint)value.VertexStride.Value);
            output.Write((uint)value.VertexElements.Count);
            foreach (var element in value.VertexElements)
            {
                output.Write((uint)element.Offset);
                output.Write((int)element.VertexElementFormat);
                output.Write((int)element.VertexElementUsage);
                output.Write((uint)element.UsageIndex);
            }
        }
    }
}
