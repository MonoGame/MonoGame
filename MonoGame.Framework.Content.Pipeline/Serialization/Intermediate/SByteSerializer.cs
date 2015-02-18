// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class SByteSerializer : ElementSerializer<sbyte>
    {
        public SByteSerializer() :
            base("sbyte", 1)
        {
        }

        protected internal override sbyte Deserialize(string[] inputs, ref int index)
        {
            if (inputs.Length > 0)
            {
                return XmlConvert.ToSByte(inputs[index++]);
            }
            return new sbyte();
        }

        protected internal override void Serialize(sbyte value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}
