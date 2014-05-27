// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class IntSerializer : ContentTypeSerializer<int>
    {
        public IntSerializer() :
            base("int")
        {
        }

        protected internal override int Deserialize(IntermediateReader input, ContentSerializerAttribute format, int existingInstance)
        {
            var str = input.Xml.ReadString();
            return XmlConvert.ToInt32(str);
        }

        protected internal override void Serialize(IntermediateWriter output, int value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value);
            output.Xml.WriteString(str);
        }
    }
}