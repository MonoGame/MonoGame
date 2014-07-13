// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class Vector2Serializer : ContentTypeSerializer<Vector2>
    {
        public Vector2Serializer() :
            base("Vector2")
        {
        }

        protected internal override Vector2 Deserialize(IntermediateReader input, ContentSerializerAttribute format, Vector2 existingInstance)
        {
            var str = input.Xml.ReadString();
            var elems = str.Split(' ');
            return new Vector2( XmlConvert.ToSingle(elems[0]),
                                XmlConvert.ToSingle(elems[1]));
        }

        protected internal override void Serialize(IntermediateWriter output, Vector2 value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value.X) + " " +
                      XmlConvert.ToString(value.Y);
            output.Xml.WriteString(str);
        }
    }
}