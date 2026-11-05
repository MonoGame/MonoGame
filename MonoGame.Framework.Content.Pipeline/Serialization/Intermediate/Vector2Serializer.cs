// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

[ContentTypeSerializer]
class Vector2Serializer() : ElementSerializer<Vector2>("Vector2", 2)
{
    protected override Vector2 Deserialize(string[] inputs, ref int index)
        => new(XmlConvert.ToSingle(inputs[index++]), XmlConvert.ToSingle(inputs[index++]));

    protected override void Serialize(Vector2 value, List<string> results)
    {
        results.Add(XmlConvert.ToString(value.X));
        results.Add(XmlConvert.ToString(value.Y));
    }
}
