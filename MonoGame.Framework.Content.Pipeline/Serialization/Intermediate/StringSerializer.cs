// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class StringSerializer : ContentTypeSerializer<string>
    {
        public StringSerializer() :
            base("string")
        {
        }

        protected internal override string Deserialize(IntermediateReader input, ContentSerializerAttribute format, string existingInstance)
        {
            return input.Xml.ReadString();
        }

        protected internal override void Serialize(IntermediateWriter output, string value, ContentSerializerAttribute format)
        {
            output.Xml.WriteString(value);
        }
    }
}