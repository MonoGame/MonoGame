// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class Vector4Serializer : ContentTypeSerializer<Vector4>
    {
        public Vector4Serializer() :
            base("Vector4")
        {
        }

        protected internal override Vector4 Deserialize(IntermediateReader input, ContentSerializerAttribute format, Vector4 existingInstance)
        {
            var str = input.Xml.ReadString();
            var elems = str.Split(' ');
            return new Vector4( XmlConvert.ToSingle(elems[0]),
                                XmlConvert.ToSingle(elems[1]),
                                XmlConvert.ToSingle(elems[2]),
                                XmlConvert.ToSingle(elems[3]));
        }

        protected internal override void Serialize(IntermediateWriter output, Vector4 value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value.X) + " " +
                      XmlConvert.ToString(value.Y) + " " +
                      XmlConvert.ToString(value.Z) + " " +
                      XmlConvert.ToString(value.W);
            output.Xml.WriteString(str);
        }
    }
}