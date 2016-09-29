using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	public class CharacterRegionTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}


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