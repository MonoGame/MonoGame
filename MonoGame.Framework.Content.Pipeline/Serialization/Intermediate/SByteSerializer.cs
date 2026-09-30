// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

[ContentTypeSerializer]
class SByteSerializer() : ElementSerializer<sbyte>("sbyte", 1)
{
    protected override sbyte Deserialize(string[] inputs, ref int index) => XmlConvert.ToSByte(inputs[index++]);

    protected override void Serialize(sbyte value, List<string> results) => results.Add(XmlConvert.ToString(value));
}
