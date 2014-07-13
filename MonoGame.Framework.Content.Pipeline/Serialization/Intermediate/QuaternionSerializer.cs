// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class QuaternionSerializer : ElementSerializer<Quaternion>
    {
        public QuaternionSerializer() :
            base("Quaternion", 4)
        {
        }

        protected internal override Quaternion Deserialize(string[] inputs, ref int index)
        {
            return new Quaternion(  XmlConvert.ToSingle(inputs[index++]),
                                    XmlConvert.ToSingle(inputs[index++]),
                                    XmlConvert.ToSingle(inputs[index++]),
                                    XmlConvert.ToSingle(inputs[index++]));
        }

        protected internal override void Serialize(Quaternion value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value.X));
            results.Add(XmlConvert.ToString(value.Y));
            results.Add(XmlConvert.ToString(value.Z));
            results.Add(XmlConvert.ToString(value.W));
        }
    }
}