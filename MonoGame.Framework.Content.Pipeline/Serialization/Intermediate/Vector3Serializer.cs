// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class Vector3Serializer : ElementSerializer<Vector3>
    {
        public Vector3Serializer() :
            base("Vector3", 3)
        {
        }

        protected internal override Vector3 Deserialize(string[] inputs, ref int index)
        {
            return new Vector3( XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]));
        }

        protected internal override void Serialize(Vector3 value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value.X));
            results.Add(XmlConvert.ToString(value.Y));
            results.Add(XmlConvert.ToString(value.Z));
        }
    }
}