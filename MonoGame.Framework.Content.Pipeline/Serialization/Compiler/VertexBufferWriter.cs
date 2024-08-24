// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class VertexBufferWriter : BuiltInContentWriter<VertexBufferContent>
    {
        protected internal override void Write(ContentWriter output, VertexBufferContent value)
        {
            output.WriteRawObject(value.VertexDeclaration);
            output.Write((uint)(value.VertexData.Length / value.VertexDeclaration.VertexStride));
            output.Write(value.VertexData);
        }
    }
}
