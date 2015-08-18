// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class ByteSerializer : ElementSerializer<byte>
    {
        public ByteSerializer() :
            base("byte", 1)
        {
        }

        protected internal override byte Deserialize(string[] inputs, ref int index)
        {
            return XmlConvert.ToByte(inputs[index++]);
        }

        protected internal override void Serialize(byte value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}