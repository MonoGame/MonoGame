using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors;

/// <summary>
/// Class that provides methods to convert a <see cref="Color"/> to and from a string.
/// </summary>
public class StringToColorConverter : TypeConverter
{
    /// <inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    /// <inheritdoc/>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Color color)
            return $"{color.R},{color.G},{color.B},{color.A}";

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string strValue)
        {
            // Check if the string is in the older XNA "{R:0 G:0 B:0 A:0}" format
            if (strValue.StartsWith('{') && strValue.EndsWith('}'))
            {
                strValue = strValue.Trim(['{', '}']);
                var parts = strValue.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4)
                {
                    var r = int.Parse(parts[0].Split(':')[1], CultureInfo.InvariantCulture);
                    var g = int.Parse(parts[1].Split(':')[1], CultureInfo.InvariantCulture);
                    var b = int.Parse(parts[2].Split(':')[1], CultureInfo.InvariantCulture);
                    var a = int.Parse(parts[3].Split(':')[1], CultureInfo.InvariantCulture);
                    return new Color(r, g, b, a);
                }
            }
            else // Assume the string is in the MonoGame "r,g,b,a" format
            {
                var values = strValue.Split([','], StringSplitOptions.None);
                if (values.Length == 4)
                {
                    var r = int.Parse(values[0].Trim(), CultureInfo.InvariantCulture);
                    var g = int.Parse(values[1].Trim(), CultureInfo.InvariantCulture);
                    var b = int.Parse(values[2].Trim(), CultureInfo.InvariantCulture);
                    var a = int.Parse(values[3].Trim(), CultureInfo.InvariantCulture);
                    return new Color(r, g, b, a);
                }
            }

            throw new ArgumentException($"Could not convert from string({value}) to Color, expected format is 'r,g,b,a' or '{{R:0 G:0 B:0 A:0}}'");
        }

        return base.ConvertFrom(context, culture, value);
    }

}
