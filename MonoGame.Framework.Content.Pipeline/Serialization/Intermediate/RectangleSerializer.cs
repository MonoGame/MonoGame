// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class RectangleSerializer : ContentTypeSerializer<Rectangle>
    {
        public RectangleSerializer() :
            base("Rectangle")
        {
        }

        protected internal override Rectangle Deserialize(IntermediateReader input, ContentSerializerAttribute format, Rectangle existingInstance)
        {
            var str = input.Xml.ReadString();
            var elems = str.Split(' ');
            return new Rectangle(   XmlConvert.ToInt32(elems[0]),
                                    XmlConvert.ToInt32(elems[1]),
                                    XmlConvert.ToInt32(elems[2]),
                                    XmlConvert.ToInt32(elems[3]));
        }

        protected internal override void Serialize(IntermediateWriter output, Rectangle value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value.X) + " " +
                      XmlConvert.ToString(value.Y) + " " +
                      XmlConvert.ToString(value.Width) + " " +
                      XmlConvert.ToString(value.Height);
            output.Xml.WriteString(str);
        }
    }
}