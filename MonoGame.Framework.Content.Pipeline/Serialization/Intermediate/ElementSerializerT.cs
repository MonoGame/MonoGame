// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

abstract class ElementSerializer<T>(string xmlTypeName, int elementCount) : ContentTypeSerializer<T>(xmlTypeName)
{
    private static void ThrowElementCountException() => throw new InvalidContentException("Not have enough entries in space-separated list!");

    protected abstract T Deserialize(string [] inputs, ref int index);

    protected abstract void Serialize(T? value, List<string> results);

    protected internal void Deserialize(IntermediateReader input, List<T> results)
    {
        var elements = PackedElementsHelper.ReadElements(input);

        for (var index = 0; index < elements.Length;)
        {
            if (elements.Length - index < elementCount)
                ElementSerializer<T>.ThrowElementCountException();

            var elem = Deserialize(elements, ref index);
            results.Add(elem);
        }
    }

    protected internal override T Deserialize(IntermediateReader input, ContentSerializerAttribute format, T? existingInstance)
    {
        var elements = PackedElementsHelper.ReadElements(input);

        if (elements.Length < elementCount)
            ElementSerializer<T>.ThrowElementCountException();

        var index = 0;
        return Deserialize(elements, ref index);
    }

    protected internal void Serialize(IntermediateWriter output, List<T> values)
    {
        var elements = new List<string>();
        for (var i = 0; i < values.Count; i++)
            Serialize(values[i], elements);
        var str = PackedElementsHelper.JoinElements(elements);
        output.Xml.WriteString(str);
    }

    protected internal override void Serialize(IntermediateWriter output, T? value, ContentSerializerAttribute format)
    {
        var elements = new List<string>();
        Serialize(value, elements);
        var str = PackedElementsHelper.JoinElements(elements);
        output.Xml.WriteString(str);
    }
}
