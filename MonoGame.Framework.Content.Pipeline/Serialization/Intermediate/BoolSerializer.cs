// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class BoolSerializer : ContentTypeSerializer<bool>
    {
        public BoolSerializer() :
            base("bool")
        {
        }

        protected internal override bool Deserialize(IntermediateReader input, ContentSerializerAttribute format, bool existingInstance)
        {
            var str = input.Xml.ReadString();
            return XmlConvert.ToBoolean(str);
        }

        protected internal override void Serialize(IntermediateWriter output, bool value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value);
            output.Xml.WriteString(str);
        }
    }
}