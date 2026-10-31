// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class PlaneSerializer() : ElementSerializer<Plane>("Plane", 4)
    {
        protected override Plane Deserialize(string[] inputs, ref int index)
        {
            return new Plane(
                XmlConvert.ToSingle(inputs[index++]),
                XmlConvert.ToSingle(inputs[index++]),
                XmlConvert.ToSingle(inputs[index++]),
                XmlConvert.ToSingle(inputs[index++])
            );
        }

        protected override void Serialize(Plane value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value.Normal.X));
            results.Add(XmlConvert.ToString(value.Normal.Y));
            results.Add(XmlConvert.ToString(value.Normal.Z));
            results.Add(XmlConvert.ToString(value.D));
        }
    }
}
