// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
            var str = inputs[index++];
            if (str.Length == 1)
                return XmlConvert.ToChar(str);

            // Try parsing it as a UTF code.
            int val;
            if (int.TryParse(str, out val))
                return char.ConvertFromUtf32(val)[0];

            // Last ditch effort to decode it as XML escape value.
            return XmlConvert.ToChar(XmlConvert.DecodeName(str));
        }

        protected internal override void Serialize(char value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value));
        }
    }
}