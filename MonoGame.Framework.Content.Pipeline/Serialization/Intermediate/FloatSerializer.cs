// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class FloatSerializer : ElementSerializer<float>
    {
        public FloatSerializer() :
            base("float", 1)
        {
        }

        protected internal override float Deserialize(string[] inputs, ref int index)
        {
            return XmlConvert.ToSingle(inputs[index++]);
        }

        protected internal override void Serialize(float value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}