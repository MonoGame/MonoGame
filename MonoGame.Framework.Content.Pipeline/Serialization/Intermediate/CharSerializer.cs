// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class CharSerializer : ElementSerializer<char>
    {
        public CharSerializer() :
            base("char", 1)
        {
        }

        protected internal override char Deserialize(string[] inputs, ref int index)
        {
            return XmlConvert.ToChar(inputs[index++]);
        }

        protected internal override void Serialize(char value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}