// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class CharExSerializer : ElementSerializer<CharEx>
    {
        public CharExSerializer() :
            base("CharEx", 1)
        {
        }

        protected internal override CharEx Deserialize(string[] inputs, ref int index)
        {
            return new CharEx(inputs[index++]);
            
        }

        protected internal override void Serialize(CharEx value, List<string> results)
        {
            results.Add(value.ToString());
        }
    }
}