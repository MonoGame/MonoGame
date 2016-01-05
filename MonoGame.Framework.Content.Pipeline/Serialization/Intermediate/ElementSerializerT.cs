// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    abstract class ElementSerializer<T> : ContentTypeSerializer<T>
    {
        private static readonly char [] _seperators = { ' ', '\t', '\n' };

        private const string _writeSeperator = " ";

        private readonly int _elementCount;

        protected ElementSerializer(string xmlTypeName, int elementCount) :
            base(xmlTypeName)
        {
            _elementCount = elementCount;
        }

        protected void ThrowElementCountException()
        {
            throw new InvalidContentException("Not have enough entries in space-separated list!");
        }

        protected internal abstract T Deserialize(string [] inputs, ref int index);

        protected internal abstract void Serialize(T value, List<string> results);

        private static string[] ReadElements(IntermediateReader input)
        {
            if (input.Xml.IsEmptyElement)
                return new string[0];

            string str = string.Empty;
            while (input.Xml.NodeType != XmlNodeType.EndElement)
            {
                if (input.Xml.NodeType == XmlNodeType.Comment)
                    input.Xml.Read();
                else
                    str += input.Xml.ReadString();
            }

            // Special case for char ' '
            if (str.Length > 0 && str.Trim() == string.Empty)
                return new string[] { str };

            var elements = str.Split(_seperators, StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length == 1 && string.IsNullOrEmpty(elements[0]))
                return new string[0];

            return elements;
        }

        protected internal void Deserialize(IntermediateReader input, List<T> results)
        {
            var elements = ReadElements(input);
                            
            for (var index = 0; index < elements.Length;)
            {
                if (elements.Length - index < _elementCount)
                    ThrowElementCountException();

                var elem = Deserialize(elements, ref index);
                results.Add(elem);
            }
        }

        protected internal override T Deserialize(IntermediateReader input, ContentSerializerAttribute format, T existingInstance)
        {
            var elements = ReadElements(input);

            if (elements.Length < _elementCount)
                ThrowElementCountException();

            var index = 0;
            return Deserialize(elements, ref index);
        }

        protected internal void Serialize(IntermediateWriter output, List<T> values)
        {
            var elements = new List<string>();
            for (var i = 0; i < values.Count; i++)
                Serialize(values[i], elements);
            var str = string.Join(_writeSeperator, elements);
            output.Xml.WriteString(str);
        }

        protected internal override void Serialize(IntermediateWriter output, T value, ContentSerializerAttribute format)
        {
            var elements = new List<string>();
            Serialize(value, elements);
            var str = string.Join(_writeSeperator, elements);
            output.Xml.WriteString(str);
        }
    }
}
