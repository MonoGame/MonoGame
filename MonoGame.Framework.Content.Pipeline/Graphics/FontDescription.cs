// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	/// <summary>
	/// Provides information to the FontDescriptionProcessor describing which font to rasterize, which font size to utilize, and which Unicode characters to include in the processor output.
	/// </summary>
	public class FontDescription : ContentItem
	{
		char? defaultCharacter;
		string fontName;
		float size;
		float spacing;
		FontDescriptionStyle style;
		bool useKerning;
		List<CharacterRegion> characterRegions;

		/// <summary>
		/// Gets or sets the default character for the font.
		/// </summary>
		[ContentSerializerAttribute]
		public Nullable<char> DefaultCharacter
		{
			get
			{
				return defaultCharacter;
			}
			set
			{
				defaultCharacter = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the font, such as "Times New Roman" or "Arial". This value cannot be null or empty.
		/// </summary>
		public string FontName
		{
			get
			{
				return fontName;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("FontName is null or an empty string.");
				fontName = value;
			}
		}

		/// <summary>
		/// Gets or sets the size, in points, of the font.
		/// </summary>
		public float Size
		{
			get
			{
				return size;
			}
			set
			{
				if (value <= 0.0f)
					throw new ArgumentOutOfRangeException("Size is less than or equal to zero. Specify a value for this property that is greater than zero.");
				size = value;
			}
		}

		/// <summary>
		/// Gets or sets the amount of space, in pixels, to insert between letters in a string.
		/// </summary>
		public float Spacing
		{
			get
			{
				return spacing;
			}
			set
			{
				if (value < 0.0f)
					throw new ArgumentOutOfRangeException("Spacing is less than or equal to zero.");
				spacing = value;
			}
		}

		/// <summary>
		/// Gets or sets the style of the font, expressed as a combination of one or more FontDescriptionStyle flags.
		/// </summary>
		public FontDescriptionStyle Style
		{
			get
			{
				return style;
			}
			set
			{
				style = value;
			}
		}

		/// <summary>
		/// Indicates if kerning information is used when drawing characters.
		/// </summary>
		[ContentSerializerAttribute]
		public bool UseKerning
		{
			get
			{
				return useKerning;
			}
			set
			{
				useKerning = value;
			}
		}


		/// <summary>
		/// Gets the collection of characters provided by this FontDescription.
		/// </summary>
		[ContentSerializerIgnoreAttribute]
		public List<CharacterRegion> CharacterRegions
		{
			get
			{
				return characterRegions;
			}
		}

		/// <summary>
		/// Initializes a new instance of FontDescription and initializes its members to the specified font, size, and spacing, using FontDescriptionStyle.Regular as the default value for Style.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		public FontDescription(string fontName, float size, float spacing)
			: this(fontName, size, spacing, FontDescriptionStyle.Regular, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of FontDescription and initializes its members to the specified font, size, spacing, and style.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		/// <param name="fontStyle">The font style for the font.</param>
		public FontDescription(string fontName, float size, float spacing, FontDescriptionStyle fontStyle)
			: this(fontName, size, spacing, fontStyle, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of FontDescription using the specified values.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		/// <param name="fontStyle">The font style for the font.</param>
		/// <param name="useKerning">true if kerning information is used when drawing characters; false otherwise.</param>
		public FontDescription(string fontName, float size, float spacing, FontDescriptionStyle fontStyle, bool useKerning)
		{
			// Write to the properties so the validation is run
			FontName = fontName;
			Size = size;
			Spacing = spacing;
			Style = fontStyle;
			UseKerning = useKerning;
			characterRegions = new List<CharacterRegion> () { CharacterRegion.Default };
		}

		/// <summary>
		/// Initializes a new instance of FontDescription using the specified values.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		/// <param name="fontStyle">The font style for the font.</param>
		/// <param name="useKerning">true if kerning information is used when drawing characters; false otherwise.</param>
		/// <param name="charRegions">The characters to include. Defaults to the base ASCII set.</param>
		public FontDescription(string fontName, float size, float spacing, FontDescriptionStyle fontStyle, bool useKerning, List<CharacterRegion> charRegions)
		{
			// Write to the properties so the validation is run
			FontName = fontName;
			Size = size;
			Spacing = spacing;
			Style = fontStyle;
			UseKerning = useKerning;
			characterRegions = charRegions;
		}
	}
}
