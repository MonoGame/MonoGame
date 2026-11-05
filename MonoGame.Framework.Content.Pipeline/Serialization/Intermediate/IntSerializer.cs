// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

[ContentTypeSerializer]
class IntSerializer : ElementSerializer<int>
{
    public IntSerializer() : base("int", 1)
    {
    }

    protected override int Deserialize(string[] inputs, ref int index) => XmlConvert.ToInt32(inputs[index++]);

    protected override void Serialize(int value, List<string> results) => results.Add(XmlConvert.ToString(value));
}
