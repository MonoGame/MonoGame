// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Drawing;
using System.Xml.Serialization;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    /// <summary>
    /// Helper for serializing color types with the XmlSerializer.
    /// </summary>
    public class XmlColor
    {
        private Color _color;

        /// <summary>
        /// Creates a new instance of XmlColor with <see cref="Color.Empty"/> color.
        /// </summary>
        public XmlColor()
        {            
        }

        /// <summary>
        /// Creates a new instance of XmlColor with provided color
        /// </summary>
        /// <param name="c">Color to be stored</param>
        public XmlColor(Color c)
        {
            _color = c;
        }

        /// <summary>
        /// Implicit XmlColor -> Color conversion
        /// </summary>
        public static implicit operator Color(XmlColor x)
        {
            return x._color;
        }

        /// <summary>
        /// Implicit Color -> XmlColor conversion
        /// </summary>
        public static implicit operator XmlColor(Color c)
        {
            return new XmlColor(c);
        }

        /// <summary>
        /// Returns a string representation of the supplied <see cref="Color"/>.
        /// </summary>
        /// <remarks>
        /// This string value can be used in <see cref="ToColor(string)"/> to get identical color object.
        /// </remarks>
        /// <param name="color">Color to be converted.</param>
        /// <returns>
        /// The predefined name of the color, if it's named,
        /// or a string in the format <b>"R, G, B, A"</b>, if the color is not named,
        /// with every component in the [0..255] range.
        /// </returns>
        public static string FromColor(Color color)
        {
            if (color.IsNamedColor)
                return color.Name;
            return string.Format("{0}, {1}, {2}, {3}", color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Returns a new <see cref="Color"/> object from its string representation.
        /// </summary>
        /// <param name="value">
        /// A predefined color name,
        /// OR a string in the format <b>"R, G, B, A"</b>, where each component is in the [0..255] range.<para/>
        /// </param>
        public static Color ToColor(string value)
        {            
            if (!value.Contains(','))
                return Color.FromName(value);

            int r, g, b, a;
            var colors = value.Split(',');
            int.TryParse(colors.Length > 0 ? colors[0] : string.Empty, out r);
            int.TryParse(colors.Length > 1 ? colors[1] : string.Empty, out g);
            int.TryParse(colors.Length > 2 ? colors[2] : string.Empty, out b);
            int.TryParse(colors.Length > 3 ? colors[3] : string.Empty, out a);

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Default color value as a string, used for serialization and deserialization.
        /// </summary>
        [XmlText]
        public string Default
        {
            get { return FromColor(_color); }
            set { _color = ToColor(value); }
        }
    }
}
