// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class ColorSerializer() : ElementSerializer<Color>("Color", 1)
    {
        protected override Color Deserialize(string[] inputs, ref int index)
        {
            var value = uint.Parse(inputs[index++], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new Color(
                (int)(value >> 16 & 0xFF),
                (int)(value >> 8 & 0xFF),
                (int)(value >> 0 & 0xFF),
                (int)(value >> 24 & 0xFF)
            );
        }

        protected override void Serialize(Color value, List<string> results)
            => results.Add($"{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}");
    }
}
