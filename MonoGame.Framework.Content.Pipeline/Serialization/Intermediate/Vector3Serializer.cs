// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class Vector3Serializer : ContentTypeSerializer<Vector3>
    {
        public Vector3Serializer() :
            base("Vector3")
        {
        }

        protected internal override Vector3 Deserialize(IntermediateReader input, ContentSerializerAttribute format, Vector3 existingInstance)
        {
            var str = input.Xml.ReadString();
            var elems = str.Split(' ');
            return new Vector3( XmlConvert.ToSingle(elems[0]),
                                XmlConvert.ToSingle(elems[1]),
                                XmlConvert.ToSingle(elems[2]));
        }

        protected internal override void Serialize(IntermediateWriter output, Vector3 value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value.X) + " " +
                      XmlConvert.ToString(value.Y) + " " +
                      XmlConvert.ToString(value.Z);
            output.Xml.WriteString(str);
        }
    }
}