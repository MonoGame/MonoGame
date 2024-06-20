using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties to convert a string to a CharacterRegion object.
    /// </summary>
	public class CharacterRegionTypeConverter : TypeConverter
	{
        /// <summary>
        /// Determines if the converter can convert from the specified source type.
        /// </summary>
        /// <param name="context">The type descriptor context to use.</param>
        /// <param name="sourceType">The source type to check.</param>
        /// <returns><c>true</c> if the converter can convert from the specified source type; otherwise, <c>false</c>.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

        /// <summary>
        /// Converts the given input object to a CharacterRegion object.
        /// </summary>
        /// <param name="context">The type descriptor context to use.</param>
        /// <param name="culture">The culture information associated with the conversion.</param>
        /// <param name="value">The object to be converted, must be a string.</param>
        /// <returns>A CharacterRegion object representing the input string.</returns>
        /// <exception cref="ArgumentException">Thrown if the input string is null or empty.</exception>
        /// <remarks>
        /// The input string can be in one of the following formats:
        /// - A single character (eg. "a").
        /// - A range of characters (eg. "a-z").
        /// </remarks>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			// Input must be a string.
			string source = value as string;

			if (string.IsNullOrEmpty(source))
			{
				throw new ArgumentException();
			}

			// Supported input formats:
			//  A
			//  A-Z
			//  32-127
			//  0x20-0x7F

			var splitStr = source.Split('-');
			var split = new char[splitStr.Length];
			for (int i = 0; i < splitStr.Length; i++)
			{
				split[i] = ConvertCharacter(splitStr[i]);
			}

			switch (split.Length)
			{
				case 1:
				// Only a single character (eg. "a").
				return new CharacterRegion(split[0], split[0]);

				case 2:
				// Range of characters (eg. "a-z").
				return new CharacterRegion(split[0], split[1]);

				default:
				throw new ArgumentException();
			}
		}


		static char ConvertCharacter(string value)
		{
			if (value.Length == 1)
			{
				// Single character directly specifies a codepoint.
				return value[0];
			}
			else
			{
				// Otherwise it must be an integer (eg. "32" or "0x20").
				return (char)(int)intConverter.ConvertFromInvariantString(value);
			}
		}


		static TypeConverter intConverter = TypeDescriptor.GetConverter(typeof(int));
	}
}
