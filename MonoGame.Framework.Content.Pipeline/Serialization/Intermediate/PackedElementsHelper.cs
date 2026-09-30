// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    internal static class PackedElementsHelper
    {
        private static readonly char[] Seperators = [' ', '\t', '\n'];
        private const string WriteSeperator = " ";

        internal static string[] ReadElements(IntermediateReader input)
        {
            if (input.Xml.IsEmptyElement)
                return [];

            var str = string.Empty;
            while (input.Xml.NodeType != XmlNodeType.EndElement)
            {
                if (input.Xml.NodeType == XmlNodeType.Comment)
                    input.Xml.Read();
                else
                    str += input.Xml.ReadString();
            }

            // Special case for char ' '
            if (str.Length > 0 && str.Trim() == string.Empty)
                return [str];

            var elements = str.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length == 1 && string.IsNullOrEmpty(elements[0]))
                return [];

            return elements;
        }

        public static string JoinElements(IEnumerable<string> elements) => string.Join(WriteSeperator, elements);
    }
}
