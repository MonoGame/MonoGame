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

			switch (splitStr.Length)
			{
				case 1:
				// Only a single character (eg. "a").
				return new CharacterRegion(new CharEx(splitStr[0]), new CharEx(splitStr[0]));

				case 2:
				// Range of characters (eg. "a-z").
				return new CharacterRegion(new CharEx(splitStr[0]), new CharEx(splitStr[1]));

				default:
				throw new ArgumentException();
			}
		}

		static CharEx ConvertCharacter(string value)
		{
            // CharEx Contains this in its constructor
            return new CharEx(value);
		}
	}
}
