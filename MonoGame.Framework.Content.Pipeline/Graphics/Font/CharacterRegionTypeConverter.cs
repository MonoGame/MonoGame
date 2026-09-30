using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class CharacterRegionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }


        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            // Input must be a string.
            var source = value as string;

            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            // Parse as single character or range (e.g., "A", "A-Z", "32-127", "0x20-0x7F").
            var splitStr = source.Split('-');
            var split = new char[splitStr.Length];
            for (var i = 0; i < splitStr.Length; i++)
            {
                split[i] = ConvertCharacter(splitStr[i]);
            }

            return split.Length switch
            {
                1 => new CharacterRegion(split[0], split[0]), // Only a single character (eg. "a").
                2 => new CharacterRegion(split[0], split[1]), // Range of characters (eg. "a-z").
                _ => throw new ArgumentException("Input format is not supported."),
            };
        }


        private static char ConvertCharacter(string value)
        {
            if (value.Length == 1)
            {
                // Single character directly specifies a codepoint.
                return value[0];
            }

            // Check if the value is hexadecimal with the "0x" prefix
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                // Remove "0x" prefix and parse as hexadecimal
                var numericPart = value.Substring(2);
                if (int.TryParse(numericPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexResult))
                {
                    return (char)hexResult;
                }

                throw new ArgumentException($"Invalid hexadecimal codepoint: {value}");
            }
            // Handle decimal entity format (e.g., "&#12354;")

            if (value.StartsWith("&#") && value.EndsWith(';'))
            {
                var numericPart = value.Substring(2, value.Length - 3);
                if (int.TryParse(numericPart, out var entityResult))
                {
                    return (char)entityResult;
                }

                throw new ArgumentException($"Invalid decimal entity codepoint: {value}");
            }

            if (int.TryParse(value, out var intResult))
            {
                return (char)intResult;
            }

            throw new ArgumentException($"Invalid character or codepoint: {value}");
        }
    }
}
