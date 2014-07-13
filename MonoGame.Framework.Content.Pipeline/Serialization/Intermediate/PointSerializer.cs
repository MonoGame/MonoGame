// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class PointSerializer : ContentTypeSerializer<Point>
    {
        public PointSerializer() :
            base("Point")
        {
        }

        protected internal override Point Deserialize(IntermediateReader input, ContentSerializerAttribute format, Point existingInstance)
        {
            var str = input.Xml.ReadString();
            var elems = str.Split(' ');
            return new Point(   XmlConvert.ToInt32(elems[0]),
                                XmlConvert.ToInt32(elems[1]));
        }

        protected internal override void Serialize(IntermediateWriter output, Point value, ContentSerializerAttribute format)
        {
            var str = XmlConvert.ToString(value.X) + " " +
                      XmlConvert.ToString(value.Y);
            output.Xml.WriteString(str);
        }
    }
}