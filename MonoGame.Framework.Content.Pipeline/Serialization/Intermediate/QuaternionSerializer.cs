// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class QuaternionSerializer : ContentTypeSerializer<Quaternion>
    {
        public QuaternionSerializer() :
            base("Quaternion")
        {
        }

        protected internal override Quaternion Deserialize(IntermediateReader input, ContentSerializerAttribute format, Quaternion existingInstance)
        {
            var str = input.Xml.ReadString();
            var elems = str.Split(' ');
            return new Quaternion(XmlConvert.ToSingle(elems[0]),
                                  XmlConvert.ToSingle(elems[1]),
                                  XmlConvert.ToSingle(elems[2]),
                                  XmlConvert.ToSingle(elems[3]));
        }

        protected internal override void Serialize(IntermediateWriter output, Quaternion value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value.X) + " " +
                      XmlConvert.ToString(value.Y) + " " +
                      XmlConvert.ToString(value.Z) + " " +
                      XmlConvert.ToString(value.W);
            output.Xml.WriteString(str);
        }
    }
}