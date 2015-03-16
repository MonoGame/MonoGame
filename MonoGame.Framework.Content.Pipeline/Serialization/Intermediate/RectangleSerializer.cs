// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class RectangleSerializer : ElementSerializer<Rectangle>
    {
        public RectangleSerializer() :
            base("Rectangle", 4)
        {
        }

        protected internal override Rectangle Deserialize(string[] inputs, ref int index)
        {
            return new Rectangle(   XmlConvert.ToInt32(inputs[index++]),
                                    XmlConvert.ToInt32(inputs[index++]),
                                    XmlConvert.ToInt32(inputs[index++]),
                                    XmlConvert.ToInt32(inputs[index++]));
        }

        protected internal override void Serialize(Rectangle value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value.X));
            results.Add(XmlConvert.ToString(value.Y));
            results.Add(XmlConvert.ToString(value.Width));
            results.Add(XmlConvert.ToString(value.Height));
        }
    }
}