// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Globalization;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class ColorSerializer : ContentTypeSerializer<Color>
    {
        public ColorSerializer() :
            base("Color")
        {
        }

        protected internal override Color Deserialize(IntermediateReader input, ContentSerializerAttribute format, Color existingInstance)
        {
            var str = input.Xml.ReadString();
            var packed = uint.Parse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new Color(   packed << 0,
                                packed << 8, 
                                packed << 16,
                                packed << 24);
        }

        protected internal override void Serialize(IntermediateWriter output, Color value, ContentSerializerAttribute format)
        {
            var swizzled = value.PackedValue;
            var str = XmlConvert.ToString(swizzled);
            output.Xml.WriteString(str);
        }
    }
}