// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    class IndexBufferWriter : BuiltInContentWriter<IndexCollection>
    {
        protected internal override void Write(ContentWriter output, IndexCollection value)
        {
            // Check if the buffer and can be saved as Int16.
            var shortIndices = true;
            foreach(var index in value)
            {
                if(index > ushort.MaxValue)
                {
                    shortIndices = false;
                    break;
                }
            }

            output.Write(shortIndices);

            var byteCount = shortIndices
                                ? value.Count * 2
                                : value.Count * 4;

            output.Write(byteCount);
            if (shortIndices)
            {
                foreach (var item in value)
                    output.Write((ushort)item);
            }
            else
            {
                foreach (var item in value)
                    output.Write(item);
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(ContentReader);
            var readerType = type.Namespace + ".IndexBufferReader, " + type.Assembly.FullName;
            return readerType;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            var type = typeof(ContentReader);
            var readerType = type.Namespace + ".IndexBufferReader, " + type.AssemblyQualifiedName;
            return readerType;
        }
    }
}
