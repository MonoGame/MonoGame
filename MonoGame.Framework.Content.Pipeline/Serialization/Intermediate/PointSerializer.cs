// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class PointSerializer : ElementSerializer<Point>
    {
        public PointSerializer() :
            base("Point", 2)
        {
        }

        protected internal override Point Deserialize(string[] inputs, ref int index)
        {
            return new Point(   XmlConvert.ToInt32(inputs[index++]),
                                XmlConvert.ToInt32(inputs[index++]));
        }

        protected internal override void Serialize(Point value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value.X));
            results.Add(XmlConvert.ToString(value.Y));
        }
    }
}