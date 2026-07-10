// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class TimeSpanSerializer : ElementSerializer<TimeSpan>
    {
        public TimeSpanSerializer() :
            base("TimeSpan", 1)
        {
        }

        protected internal override TimeSpan Deserialize(string[] inputs, ref int index)
        {
            return XmlConvert.ToTimeSpan(inputs[index++]);
        }

        protected internal override void Serialize(TimeSpan value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}