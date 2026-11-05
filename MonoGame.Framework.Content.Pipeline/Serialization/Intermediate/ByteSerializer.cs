// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

[ContentTypeSerializer]
class ByteSerializer() : ElementSerializer<byte>("byte", 1)
{
    protected override byte Deserialize(string[] inputs, ref int index) => XmlConvert.ToByte(inputs[index++]);

    protected override void Serialize(byte value, List<string> results) => results.Add(XmlConvert.ToString(value));
}
