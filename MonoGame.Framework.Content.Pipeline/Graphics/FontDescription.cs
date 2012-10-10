#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides information to the FontDescriptionProcessor describing which font to rasterize, which font size to utilize, and which Unicode characters to include in the processor output.
    /// </summary>
    public class FontDescription : ContentItem
    {
        List<char> characters;
        char? defaultCharacter;
        string fontName;
        float size;
        float spacing;
        FontDescriptionStyle style;
        bool useKerning;

        /// <summary>
        /// Gets the collection of characters provided by this FontDescription.
        /// </summary>
        [ContentSerializerIgnoreAttribute]
        public ICollection<char> Characters
        {
            get
            {
                return characters;
            }
        }

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
                if (value <= 0.0f)
                    throw new ArgumentOutOfRangeException("Spacing is less than or equal to zero. Specify a value for this property that is greater than zero.");
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
        }
    }
}
