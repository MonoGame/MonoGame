// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class LongSerializer : ElementSerializer<long>
    {
        public LongSerializer() :
            base("long", 1)
        {
        }

        protected internal override long Deserialize(string[] inputs, ref int index)
        {
            return XmlConvert.ToInt64(inputs[index++]);
        }

        protected internal override void Serialize(long value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}