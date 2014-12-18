// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    internal class CharacterCollection : ICollection<char>
    {
        private List<char> _items;

        public CharacterCollection()
        {
            _items = new List<char>();
        }

        public CharacterCollection(IEnumerable<char> characters)
        {
            _items = new List<char>();
            foreach (var c in characters)
                Add(c);
        }

        #region ICollection<char> Members

        public void Add(char item)
        {
            if (!_items.Contains(item))
                _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(char item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(char[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(char item)
        {
            return _items.Remove(item);
        }

        #endregion

        #region IEnumerable<char> Members

        public IEnumerator<char> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion
    }

	/// <summary>
	/// Provides information to the FontDescriptionProcessor describing which font to rasterize, which font size to utilize, and which Unicode characters to include in the processor output.
	/// </summary>
	public class FontDescription : ContentItem
	{
        private char? defaultCharacter;
        private string fontName;
        private float size;
        private float spacing;
        private FontDescriptionStyle style;
        private bool useKerning;
	    private CharacterCollection characters = new CharacterCollection();

		/// <summary>
		/// Gets or sets the name of the font, such as "Times New Roman" or "Arial". This value cannot be null or empty.
		/// </summary>
        [ContentSerializer(AllowNull = false)]
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
					throw new ArgumentOutOfRangeException("Size must be greater than zero.");
				size = value;
			}
		}

		/// <summary>
		/// Gets or sets the amount of space, in pixels, to insert between letters in a string.
		/// </summary>
        [ContentSerializer(Optional = true)]
		public float Spacing
		{
			get
			{
				return spacing;
			}
			set
			{
				spacing = value;
			}
		}

        /// <summary>
        /// Indicates if kerning information is used when drawing characters.
        /// </summary>
        [ContentSerializer(Optional = true)]
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
        /// Gets or sets the default character for the font.
        /// </summary>
        [ContentSerializer(Optional = true)]
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

        [ContentSerializer(CollectionItemName = "CharacterRegion")]
        internal CharacterRegion[] CharacterRegions
        {
            get
            {
                var regions = new List<CharacterRegion>();
                var chars = Characters.ToList();
                chars.Sort();

                var start = chars[0];
                var end = chars[0];

                for (var i=1; i < chars.Count; i++)
                {
                    if (chars[i] != (end+1))
                    {
                        regions.Add(new CharacterRegion(start, end));
                        start = chars[i];
                    }
                    end = chars[i];
                }

                regions.Add(new CharacterRegion(start, end));

                return regions.ToArray();
            }

            set
            {
                for (int index = 0; index < value.Length; ++index)
                {
                    CharacterRegion characterRegion = value[index];
                    if (characterRegion.End < characterRegion.Start)
                        throw new ArgumentException("CharacterRegion.End must be greater than CharacterRegion.Start");

                    for (var start = characterRegion.Start; start <= characterRegion.End; start++)
                        Characters.Add(start);
                }
            }
        }
		
	    [ContentSerializerIgnore]
	    public ICollection<char> Characters
	    {
	        get { return characters; } 
            internal set { characters = new CharacterCollection(value); }
	    }

        internal FontDescription()
        {
        }

		/// <summary>
		/// Initializes a new instance of FontDescription and initializes its members to the specified font, size, and spacing, using FontDescriptionStyle.Regular as the default value for Style.
		/// </summary>
		/// <param name="fontName">The name of the font, such as Times New Roman.</param>
		/// <param name="size">The size, in points, of the font.</param>
		/// <param name="spacing">The amount of space, in pixels, to insert between letters in a string.</param>
		public FontDescription(string fontName, float size, float spacing)
			: this(fontName, size, spacing, FontDescriptionStyle.Regular, false)
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
            : this(fontName, size, spacing, fontStyle, false)
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
		}
	}
}
