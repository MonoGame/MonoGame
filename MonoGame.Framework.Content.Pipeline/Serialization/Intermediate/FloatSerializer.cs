// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class FloatSerializer : ContentTypeSerializer<float>
    {
        public FloatSerializer() :
            base("float")
        {
        }

        protected internal override float Deserialize(IntermediateReader input, ContentSerializerAttribute format, float existingInstance)
        {
            var str = input.Xml.ReadString();
            return XmlConvert.ToSingle(str);
        }

        protected internal override void Serialize(IntermediateWriter output, float value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value);
            output.Xml.WriteString(str);
        }
    }
}