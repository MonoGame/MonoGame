#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using ProtoBuf;

#if WINRT
using System.Runtime.Serialization;
#endif

#if iOS
using ProtoBuf;
#endif

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Describe a 32-bit packed color.
    /// </summary>
#if WINRT
    [DataContract]
#else
    [Serializable, ProtoContract]
#endif
    public struct Color : IEquatable<Color>
    {
        // ARGB
        private uint _packedValue;

        private Color(uint packedValue)
        {
            _packedValue = packedValue;
            // ARGB
            //_packedValue = (packedValue << 8) | ((packedValue & 0xff000000) >> 24);
            // ABGR			
            //_packedValue = (packedValue & 0xff00ff00) | ((packedValue & 0x000000ff) << 16) | ((packedValue & 0x00ff0000) >> 16);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="color">A <see cref="Vector4"/> representing color.</param>
        public Color(Vector4 color)
        {
            _packedValue = 0;

            R = (byte)MathHelper.Clamp(color.X * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(color.Y * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(color.Z * 255, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(color.W * 255, Byte.MinValue, Byte.MaxValue);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="color">A <see cref="Vector3"/> representing color.</param>
        public Color(Vector3 color)
        {
            _packedValue = 0;

            R = (byte)MathHelper.Clamp(color.X * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(color.Y * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(color.Z * 255, Byte.MinValue, Byte.MaxValue);
            A = 255;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="color">A <see cref="Color"/> for RGB values of new <see cref="Color"/> instance.</param>
        /// <param name="alpha">Alpha component value.</param>
        public Color(Color color, int alpha)
        {
            _packedValue = 0;

            R = color.R;
            G = color.G;
            B = color.B;
            A = (byte)MathHelper.Clamp(alpha, Byte.MinValue, Byte.MaxValue);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="color">A <see cref="Color"/> for RGB values of new <see cref="Color"/> instance.</param>
        /// <param name="alpha">Alpha component value.</param>
        public Color(Color color, float alpha)
        {
            _packedValue = 0;

            R = color.R;
            G = color.G;
            B = color.B;
            A = (byte)MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value</param>
        public Color(float r, float g, float b)
        {
            _packedValue = 0;

            R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
            A = 255;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value</param>
        public Color(int r, int g, int b)
        {
            _packedValue = 0;
            R = (byte)MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
            A = (byte)255;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value</param>
        /// <param name="alpha">Alpha component value.</param>
        public Color(int r, int g, int b, int alpha)
        {
            _packedValue = 0;
            R = (byte)MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha, Byte.MinValue, Byte.MaxValue);
        }

        /// <summary>
        /// Creates a new instance of <see cref="Color"/> struct.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value</param>
        /// <param name="alpha">Alpha component value.</param>
        public Color(float r, float g, float b, float alpha)
        {
            _packedValue = 0;

            R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
        }

        /// <summary>
        /// Gets or sets the blue component of <see cref="Color"/>.
        /// </summary>
#if WINRT
        [DataMember]
#else
        [ProtoMember(1)]
#endif
        public byte B
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 16);
                }
            }
            set
            {
                unchecked
                {
                    this._packedValue = (this._packedValue & 0xff00ffff) | (uint)(value << 16);
                }
            }
        }

        /// <summary>
        /// Gets or sets the green component of <see cref="Color"/>.
        /// </summary>
#if WINRT
        [DataMember]
#else
        [ProtoMember(2)]
#endif
        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 8);
                }
            }
            set
            {
                unchecked
                {
                    this._packedValue = (this._packedValue & 0xffff00ff) | ((uint)(value << 8));
                }
            }
        }

        /// <summary>
        /// Gets or sets the red component of <see cref="Color"/>.
        /// </summary>
#if WINRT
        [DataMember]
#else
        [ProtoMember(3)]
#endif
        public byte R
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue);
                }
            }
            set
            {
                unchecked
                {
                    this._packedValue = (this._packedValue & 0xffffff00) | value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the alpha component of <see cref="Color"/>.
        /// </summary>
#if WINRT
        [DataMember]
#else
        [ProtoMember(4)]
#endif
        public byte A
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 24);
                }
            }
            set
            {
                unchecked
                {
                    this._packedValue = (this._packedValue & 0x00ffffff) | ((uint)(value << 24));
                }
            }
        }

        /// <summary>
        /// Compares whether two <see cref="Color"/> instances are equal.
        /// </summary>
        /// <param name="a"><see cref="Color"/> instance on the left of the equal sign.</param>
        /// <param name="b"><see cref="Color"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Color a, Color b)
        {
            return (a.A == b.A &&
                a.R == b.R &&
                a.G == b.G &&
                a.B == b.B);
        }

        /// <summary>
        /// Compares whether two <see cref="Color"/> instances are not equal.
        /// </summary>
        /// <param name="a"><see cref="Color"/> instance on the left of the not equal sign.</param>
        /// <param name="b"><see cref="Color"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(Color a, Color b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Gets the hash code for <see cref="Color"/> instance.
        /// </summary>
        /// <returns>Hash code of the object.</returns>
        public override int GetHashCode()
        {
            return this._packedValue.GetHashCode();
        }

        /// <summary>
        /// Compares whether current instance is equal to specified object.
        /// </summary>
        /// <param name="obj">The <see cref="Color"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is Color) && this.Equals((Color)obj));
        }

        /// <summary>
        /// Transparent color (R:0,G:0,B:0,A:0).
        /// </summary>
        public static Color Transparent
        {
            get
            {
                return new Color(0);
            }
        }

        /// <summary>
        /// AliceBlue color (R:240,G:248,B:255,A:255).
        /// </summary>
        public static Color AliceBlue
        {
            get
            {
                return new Color(0xfffff8f0);
            }
        }

        /// <summary>
        /// AntiqueWhite color (R:250,G:235,B:215,A:255).
        /// </summary>
        public static Color AntiqueWhite
        {
            get
            {
                return new Color(0xffd7ebfa);
            }
        }

        /// <summary>
        /// Aqua color (R:0,G:255,B:255,A:255).
        /// </summary>
        public static Color Aqua
        {
            get
            {
                return new Color(0xffffff00);
            }
        }

        /// <summary>
        /// Aquamarine color (R:127,G:255,B:212,A:255).
        /// </summary>
        public static Color Aquamarine
        {
            get
            {
                return new Color(0xffd4ff7f);
            }
        }

        /// <summary>
        /// Azure color (R:240,G:255,B:255,A:255).
        /// </summary>
        public static Color Azure
        {
            get
            {
                return new Color(0xfffffff0);
            }
        }

        /// <summary>
        /// Beige color (R:245,G:245,B:220,A:255).
        /// </summary>
        public static Color Beige
        {
            get
            {
                return new Color(0xffdcf5f5);
            }
        }

        /// <summary>
        /// Bisque color (R:255,G:228,B:196,A:255).
        /// </summary>
        public static Color Bisque
        {
            get
            {
                return new Color(0xffc4e4ff);
            }
        }

        /// <summary>
        /// Black color (R:0,G:0,B:0,A:255).
        /// </summary>
        public static Color Black
        {
            get
            {
                return new Color(0xff000000);
            }
        }

        /// <summary>
        /// BlanchedAlmond color (R:255,G:235,B:205,A:255).
        /// </summary>
        public static Color BlanchedAlmond
        {
            get
            {
                return new Color(0xffcdebff);
            }
        }

        /// <summary>
        /// Blue color (R:0,G:0,B:255,A:255).
        /// </summary>
        public static Color Blue
        {
            get
            {
                return new Color(0xffff0000);
            }
        }

        /// <summary>
        /// BlueViolet color (R:138,G:43,B:226,A:255).
        /// </summary>
        public static Color BlueViolet
        {
            get
            {
                return new Color(0xffe22b8a);
            }
        }

        /// <summary>
        /// Brown color (R:165,G:42,B:42,A:255).
        /// </summary>
        public static Color Brown
        {
            get
            {
                return new Color(0xff2a2aa5);
            }
        }

        /// <summary>
        /// BurlyWood color (R:222,G:184,B:135,A:255).
        /// </summary>
        public static Color BurlyWood
        {
            get
            {
                return new Color(0xff87b8de);
            }
        }

        /// <summary>
        /// CadetBlue color (R:95,G:158,B:160,A:255).
        /// </summary>
        public static Color CadetBlue
        {
            get
            {
                return new Color(0xffa09e5f);
            }
        }

        /// <summary>
        /// Chartreuse color (R:127,G:255,B:0,A:255).
        /// </summary>
        public static Color Chartreuse
        {
            get
            {
                return new Color(0xff00ff7f);
            }
        }

        /// <summary>
        /// Chocolate color (R:210,G:105,B:30,A:255).
        /// </summary>
        public static Color Chocolate
        {
            get
            {
                return new Color(0xff1e69d2);
            }
        }

        /// <summary>
        /// Coral color (R:255,G:127,B:80,A:255).
        /// </summary>
        public static Color Coral
        {
            get
            {
                return new Color(0xff507fff);
            }
        }

        /// <summary>
        /// CornflowerBlue color (R:100,G:149,B:237,A:255).
        /// </summary>
        public static Color CornflowerBlue
        {
            get
            {
                return new Color(0xffed9564);
            }
        }

        /// <summary>
        /// Cornsilk color (R:255,G:248,B:220,A:255).
        /// </summary>
        public static Color Cornsilk
        {
            get
            {
                return new Color(0xffdcf8ff);
            }
        }

        /// <summary>
        /// Crimson color (R:220,G:20,B:60,A:255).
        /// </summary>
        public static Color Crimson
        {
            get
            {
                return new Color(0xff3c14dc);
            }
        }

        /// <summary>
        /// Cyan color (R:0,G:255,B:255,A:255).
        /// </summary>
        public static Color Cyan
        {
            get
            {
                return new Color(0xffffff00);
            }
        }

        /// <summary>
        /// DarkBlue color (R:0,G:0,B:139,A:255).
        /// </summary>
        public static Color DarkBlue
        {
            get
            {
                return new Color(0xff8b0000);
            }
        }

        /// <summary>
        /// DarkCyan color (R:0,G:139,B:139,A:255).
        /// </summary>
        public static Color DarkCyan
        {
            get
            {
                return new Color(0xff8b8b00);
            }
        }

        /// <summary>
        /// DarkGoldenrod color (R:184,G:134,B:11,A:255).
        /// </summary>
        public static Color DarkGoldenrod
        {
            get
            {
                return new Color(0xff0b86b8);
            }
        }

        /// <summary>
        /// DarkGray color (R:169,G:169,B:169,A:255).
        /// </summary>
        public static Color DarkGray
        {
            get
            {
                return new Color(0xffa9a9a9);
            }
        }

        /// <summary>
        /// DarkGreen color (R:0,G:100,B:0,A:255).
        /// </summary>
        public static Color DarkGreen
        {
            get
            {
                return new Color(0xff006400);
            }
        }

        /// <summary>
        /// Salmon color (R:250,G:128,B:114,A:255).
        /// </summary>
        public static Color Salmon
        {
            get
            {
                return new Color(0xff7280fa);
            }
        }

        /// <summary>
        /// Silver color (R:192,G:192,B:192,A:255).
        /// </summary>
        public static Color Silver
        {
            get
            {
                return new Color(0xffc0c0c0);
            }
        }

        /// <summary>
        /// SkyBlue color (R:135,G:206,B:235,A:255).
        /// </summary>
        public static Color SkyBlue
        {
            get
            {
                return new Color(0xffebce87);
            }
        }

        /// <summary>
        /// SlateBlue color (R:106,G:90,B:205,A:255).
        /// </summary>
        public static Color SlateBlue
        {
            get
            {
                return new Color(0xffcd5a6a);
            }
        }

        /// <summary>
        /// SlateGray color (R:112,G:128,B:144,A:255).
        /// </summary>
        public static Color SlateGray
        {
            get
            {
                return new Color(0xff908070);
            }
        }

        /// <summary>
        /// Snow color (R:255,G:250,B:250,A:255).
        /// </summary>
        public static Color Snow
        {
            get
            {
                return new Color(0xfffafaff);
            }
        }

        /// <summary>
        /// SpringGreen color (R:0,G:255,B:127,A:255).
        /// </summary>
        public static Color SpringGreen
        {
            get
            {
                return new Color(0xff7fff00);
            }
        }

        /// <summary>
        /// SteelBlue color (R:70,G:130,B:180,A:255).
        /// </summary>
        public static Color SteelBlue
        {
            get
            {
                return new Color(0xffb48246);
            }
        }

        /// <summary>
        /// Tan color (R:210,G:180,B:140,A:255).
        /// </summary>
        public static Color Tan
        {
            get
            {
                return new Color(0xff8cb4d2);
            }
        }

        /// <summary>
        /// Teal color (R:0,G:128,B:128,A:255).
        /// </summary>
        public static Color Teal
        {
            get
            {
                return new Color(0xff808000);
            }
        }

        /// <summary>
        /// Thistle color (R:216,G:191,B:216,A:255).
        /// </summary>
        public static Color Thistle
        {
            get
            {
                return new Color(0xffd8bfd8);
            }
        }

        /// <summary>
        /// Tomato color (R:255,G:99,B:71,A:255).
        /// </summary>
        public static Color Tomato
        {
            get
            {
                return new Color(0xff4763ff);
            }
        }

        /// <summary>
        /// Turquoise color (R:64,G:224,B:208,A:255).
        /// </summary>
        public static Color Turquoise
        {
            get
            {
                return new Color(0xffd0e040);
            }
        }

        /// <summary>
        /// Violet color (R:238,G:130,B:238,A:255).
        /// </summary>
        public static Color Violet
        {
            get
            {
                return new Color(0xffee82ee);
            }
        }

        /// <summary>
        /// Wheat color (R:245,G:222,B:179,A:255).
        /// </summary>
        public static Color Wheat
        {
            get
            {
                return new Color(0xffb3def5);
            }
        }

        /// <summary>
        /// White color (R:255,G:255,B:255,A:255).
        /// </summary>
        public static Color White
        {
            get
            {
                return new Color(uint.MaxValue);
            }
        }

        /// <summary>
        /// WhiteSmoke color (R:245,G:245,B:245,A:255).
        /// </summary>
        public static Color WhiteSmoke
        {
            get
            {
                return new Color(0xfff5f5f5);
            }
        }

        /// <summary>
        /// Yellow color (R:255,G:255,B:0,A:255).
        /// </summary>
        public static Color Yellow
        {
            get
            {
                return new Color(0xff00ffff);
            }
        }

        /// <summary>
        /// YellowGreen color (R:154,G:205,B:50,A:255).
        /// </summary>
        public static Color YellowGreen
        {
            get
            {
                return new Color(0xff32cd9a);
            }
        }

        /// <summary>
        /// Performs linear interpolation of <see cref="Color"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Color"/>.</param>
        /// <param name="value2">Destination <see cref="Color"/>.</param>
        /// <param name="amount">Interpolation factor.</param>
        /// <returns>Interpolated <see cref="Color"/>.</returns>
        public static Color Lerp(Color value1, Color value2, Single amount)
        {
            return new Color((int)MathHelper.Lerp(value1.R, value2.R, amount),
                                (int)MathHelper.Lerp(value1.G, value2.G, amount),
                                (int)MathHelper.Lerp(value1.B, value2.B, amount),
                                (int)MathHelper.Lerp(value1.A, value2.A, amount));
        }

        /// <summary>
        /// Multiply <see cref="Color"/> by value.
        /// </summary>
        /// <param name="value">Source <see cref="Color"/>.</param>
        /// <param name="scale">Multiplicator.</param>
        /// <returns>Multiplication result.</returns>
        public static Color Multiply(Color value, float scale)
        {
            return new Color((int)(value.R * scale), (int)(value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
        }

        /// <summary>
        /// Multiply <see cref="Color"/> by value.
        /// </summary>
        /// <param name="value">Source <see cref="Color"/>.</param>
        /// <param name="scale">Multiplicator.</param>
        /// <returns>Multiplication result.</returns>
        public static Color operator *(Color value, float scale)
        {
            return new Color((int)(value.R * scale), (int)(value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
        }

        /// <summary>
        /// Converts <see cref="Color"/> to <see cref="Vector3"/>.
        /// </summary>
        /// <returns>Converted color.</returns>
        public Vector3 ToVector3()
        {
            return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
        }

        /// <summary>
        /// Converts <see cref="Color"/> to <see cref="Vector4"/>.
        /// </summary>
        /// <returns>Converted color.</returns>
        public Vector4 ToVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        /// <summary>
        /// Gets or sets packed value of this <see cref="Color"/>.
        /// </summary>
        public UInt32 PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }

        /// <summary>
        /// Converts the color values of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the color value of this instance.</returns>
        public override string ToString()
        {
            return string.Format("[Color: R={0}, G={1}, B={2}, A={3}, PackedValue={4}]", R, G, B, A, PackedValue);
        }

        /// <summary>
        /// Translate a non-premultipled alpha <see cref="Color"/> to a <see cref="Color"/> that contains premultiplied alpha.
        /// </summary>
        /// <param name="vector">A <see cref="Vector4"/> representing color.</param>
        /// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
        public static Color FromNonPremultiplied(Vector4 vector)
        {
            return new Color(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W, vector.W);
        }

        /// <summary>
        /// Translate a non-premultipled alpha <see cref="Color"/> to a <see cref="Color"/> that contains premultiplied alpha.
        /// </summary>
        /// <param name="r">Red component value.</param>
        /// <param name="g">Green component value.</param>
        /// <param name="b">Blue component value.</param>
        /// <param name="a">Alpha component value.</param>
        /// <returns>A <see cref="Color"/> which contains premultiplied alpha data.</returns>
        public static Color FromNonPremultiplied(int r, int g, int b, int a)
        {
            return new Color((byte)(r * a / 255), (byte)(g * a / 255), (byte)(b * a / 255), a);
        }

        #region IEquatable<Color> Members

        /// <summary>
        /// Compares whether current instance is equal to specified <see cref="Color"/>.
        /// </summary>
        /// <param name="other">The <see cref="Color"/> to compare.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public bool Equals(Color other)
        {
            return this.PackedValue == other.PackedValue;
        }

        #endregion
    }
}
