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

#if WINRT
using System.Runtime.Serialization;
#endif

namespace Microsoft.Xna.Framework
{
    #if WINRT
    [DataContract]
    #else
    [Serializable]
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

        public Color(Vector4 color)
        {
            _packedValue = 0;
			
			R = (byte)MathHelper.Clamp(color.X * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(color.Y * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(color.Z * 255, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(color.W * 255, Byte.MinValue, Byte.MaxValue);
        }

        public Color(Vector3 color)
        {
            _packedValue = 0;

            R = (byte)MathHelper.Clamp(color.X * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(color.Y * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(color.Z * 255, Byte.MinValue, Byte.MaxValue);
            A = 255;
        }

        public Color(Color color, int alpha)
        {
            _packedValue = 0;

            R = color.R;
            G = color.G;
            B = color.B;
            A = (byte)MathHelper.Clamp(alpha, Byte.MinValue, Byte.MaxValue);
        }

        public Color(Color color, float alpha)
        {
            _packedValue = 0;

            R = color.R;
            G = color.G;
            B = color.B;
            A = (byte)MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
        }

        public Color(float r, float g, float b)
        {
            _packedValue = 0;
			
            R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
            A = 255;
        }

        public Color(int r, int g, int b)
        {
            _packedValue = 0;
            R = (byte)MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
            A = (byte)255;
        }


        public Color(int r, int g, int b, int alpha)
        {
            _packedValue = 0;
            R = (byte)MathHelper.Clamp(r, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha, Byte.MinValue, Byte.MaxValue);
        }

        public Color(float r, float g, float b, float alpha)
        {
            _packedValue = 0;
			
            R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
        }

#if WINRT
        [DataMember]
#endif
        public byte B
        {
            get
            {
                return (byte)(this._packedValue >> 16);
            }
            set
            {
                this._packedValue = (this._packedValue & 0xff00ffff) | (uint)(value << 16);
            }
        }

#if WINRT
        [DataMember]
#endif
        public byte G
        {
            get
            {
                return (byte)(this._packedValue >> 8);
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffff00ff) | ((uint)(value << 8));
            }
        }

#if WINRT
        [DataMember]
#endif
        public byte R
        {
            get
            {
                return (byte)(this._packedValue);
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffffff00) | value;
            }
        }

#if WINRT
        [DataMember]
#endif
        public byte A
        {
            get
            {
                return (byte)(this._packedValue >> 24);
            }
            set
            {
                this._packedValue = (this._packedValue & 0x00ffffff) | ((uint)(value << 24));
            }
        }
		
		
        public static bool operator ==(Color a, Color b)
        {
            return (a.A == b.A &&
                a.R == b.R &&
                a.G == b.G &&
                a.B == b.B);
        }

        public static bool operator !=(Color a, Color b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this._packedValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj is Color) && this.Equals((Color)obj));
        }

        public static Color TransparentBlack
        {
            get
            {
                return new Color(0);
            }
        }
         public static Color Transparent
	    {
	        get
	        {
	            return new Color(0);
	        }
	    }
	    public static Color AliceBlue
	    {
	        get
	        {
	            return new Color(0xfffff8f0);
	        }
	    }
	    public static Color AntiqueWhite
	    {
	        get
	        {
	            return new Color(0xffd7ebfa);
	        }
	    }
	    public static Color Aqua
	    {
	        get
	        {
	            return new Color(0xffffff00);
	        }
	    }
	    public static Color Aquamarine
	    {
	        get
	        {
	            return new Color(0xffd4ff7f);
	        }
	    }
	    public static Color Azure
	    {
	        get
	        {
	            return new Color(0xfffffff0);
	        }
	    }
	    public static Color Beige
	    {
	        get
	        {
	            return new Color(0xffdcf5f5);
	        }
	    }
	    public static Color Bisque
	    {
	        get
	        {
	            return new Color(0xffc4e4ff);
	        }
	    }
	    public static Color Black
	    {
	        get
	        {
	            return new Color(0xff000000);
	        }
	    }
	    public static Color BlanchedAlmond
	    {
	        get
	        {
	            return new Color(0xffcdebff);
	        }
	    }
	    public static Color Blue
	    {
	        get
	        {
	            return new Color(0xffff0000);
	        }
	    }
	    public static Color BlueViolet
	    {
	        get
	        {
	            return new Color(0xffe22b8a);
	        }
	    }
	    public static Color Brown
	    {
	        get
	        {
	            return new Color(0xff2a2aa5);
	        }
	    }
	    public static Color BurlyWood
	    {
	        get
	        {
	            return new Color(0xff87b8de);
	        }
	    }
	    public static Color CadetBlue
	    {
	        get
	        {
	            return new Color(0xffa09e5f);
	        }
	    }
	    public static Color Chartreuse
	    {
	        get
	        {
	            return new Color(0xff00ff7f);
	        }
	    }
	    public static Color Chocolate
	    {
	        get
	        {
	            return new Color(0xff1e69d2);
	        }
	    }
	    public static Color Coral
	    {
	        get
	        {
	            return new Color(0xff507fff);
	        }
	    }
	    public static Color CornflowerBlue
	    {
	        get
	        {
	            return new Color(0xffed9564);
	        }
	    }
	    public static Color Cornsilk
	    {
	        get
	        {
	            return new Color(0xffdcf8ff);
	        }
	    }
	    public static Color Crimson
	    {
	        get
	        {
	            return new Color(0xff3c14dc);
	        }
	    }
	    public static Color Cyan
	    {
	        get
	        {
	            return new Color(0xffffff00);
	        }
	    }
	    public static Color DarkBlue
	    {
	        get
	        {
	            return new Color(0xff8b0000);
	        }
	    }
	    public static Color DarkCyan
	    {
	        get
	        {
	            return new Color(0xff8b8b00);
	        }
	    }
	    public static Color DarkGoldenrod
	    {
	        get
	        {
	            return new Color(0xff0b86b8);
	        }
	    }
	    public static Color DarkGray
	    {
	        get
	        {
	            return new Color(0xffa9a9a9);
	        }
	    }
	    public static Color DarkGreen
	    {
	        get
	        {
	            return new Color(0xff006400);
	        }
	    }
	    public static Color DarkKhaki
	    {
	        get
	        {
	            return new Color(0xff6bb7bd);
	        }
	    }
	    public static Color DarkMagenta
	    {
	        get
	        {
	            return new Color(0xff8b008b);
	        }
	    }
	    public static Color DarkOliveGreen
	    {
	        get
	        {
	            return new Color(0xff2f6b55);
	        }
	    }
	    public static Color DarkOrange
	    {
	        get
	        {
	            return new Color(0xff008cff);
	        }
	    }
	    public static Color DarkOrchid
	    {
	        get
	        {
	            return new Color(0xffcc3299);
	        }
	    }
	    public static Color DarkRed
	    {
	        get
	        {
	            return new Color(0xff00008b);
	        }
	    }
	    public static Color DarkSalmon
	    {
	        get
	        {
	            return new Color(0xff7a96e9);
	        }
	    }
	    public static Color DarkSeaGreen
	    {
	        get
	        {
	            return new Color(0xff8bbc8f);
	        }
	    }
	    public static Color DarkSlateBlue
	    {
	        get
	        {
	            return new Color(0xff8b3d48);
	        }
	    }
	    public static Color DarkSlateGray
	    {
	        get
	        {
	            return new Color(0xff4f4f2f);
	        }
	    }
	    public static Color DarkTurquoise
	    {
	        get
	        {
	            return new Color(0xffd1ce00);
	        }
	    }
	    public static Color DarkViolet
	    {
	        get
	        {
	            return new Color(0xffd30094);
	        }
	    }
	    public static Color DeepPink
	    {
	        get
	        {
	            return new Color(0xff9314ff);
	        }
	    }
	    public static Color DeepSkyBlue
	    {
	        get
	        {
	            return new Color(0xffffbf00);
	        }
	    }
	    public static Color DimGray
	    {
	        get
	        {
	            return new Color(0xff696969);
	        }
	    }
	    public static Color DodgerBlue
	    {
	        get
	        {
	            return new Color(0xffff901e);
	        }
	    }
	    public static Color Firebrick
	    {
	        get
	        {
	            return new Color(0xff2222b2);
	        }
	    }
	    public static Color FloralWhite
	    {
	        get
	        {
	            return new Color(0xfff0faff);
	        }
	    }
	    public static Color ForestGreen
	    {
	        get
	        {
	            return new Color(0xff228b22);
	        }
	    }
	    public static Color Fuchsia
	    {
	        get
	        {
	            return new Color(0xffff00ff);
	        }
	    }
	    public static Color Gainsboro
	    {
	        get
	        {
	            return new Color(0xffdcdcdc);
	        }
	    }
	    public static Color GhostWhite
	    {
	        get
	        {
	            return new Color(0xfffff8f8);
	        }
	    }
	    public static Color Gold
	    {
	        get
	        {
	            return new Color(0xff00d7ff);
	        }
	    }
	    public static Color Goldenrod
	    {
	        get
	        {
	            return new Color(0xff20a5da);
	        }
	    }
	    public static Color Gray
	    {
	        get
	        {
	            return new Color(0xff808080);
	        }
	    }
	    public static Color Green
	    {
	        get
	        {
	            return new Color(0xff008000);
	        }
	    }
	    public static Color GreenYellow
	    {
	        get
	        {
	            return new Color(0xff2fffad);
	        }
	    }
	    public static Color Honeydew
	    {
	        get
	        {
	            return new Color(0xfff0fff0);
	        }
	    }
	    public static Color HotPink
	    {
	        get
	        {
	            return new Color(0xffb469ff);
	        }
	    }
	    public static Color IndianRed
	    {
	        get
	        {
	            return new Color(0xff5c5ccd);
	        }
	    }
	    public static Color Indigo
	    {
	        get
	        {
	            return new Color(0xff82004b);
	        }
	    }
	    public static Color Ivory
	    {
	        get
	        {
	            return new Color(0xfff0ffff);
	        }
	    }
	    public static Color Khaki
	    {
	        get
	        {
	            return new Color(0xff8ce6f0);
	        }
	    }
	    public static Color Lavender
	    {
	        get
	        {
	            return new Color(0xfffae6e6);
	        }
	    }
	    public static Color LavenderBlush
	    {
	        get
	        {
	            return new Color(0xfff5f0ff);
	        }
	    }
	    public static Color LawnGreen
	    {
	        get
	        {
	            return new Color(0xff00fc7c);
	        }
	    }
	    public static Color LemonChiffon
	    {
	        get
	        {
	            return new Color(0xffcdfaff);
	        }
	    }
	    public static Color LightBlue
	    {
	        get
	        {
	            return new Color(0xffe6d8ad);
	        }
	    }
	    public static Color LightCoral
	    {
	        get
	        {
	            return new Color(0xff8080f0);
	        }
	    }
	    public static Color LightCyan
	    {
	        get
	        {
	            return new Color(0xffffffe0);
	        }
	    }
	    public static Color LightGoldenrodYellow
	    {
	        get
	        {
	            return new Color(0xffd2fafa);
	        }
	    }
	    public static Color LightGreen
	    {
	        get
	        {
	            return new Color(0xff90ee90);
	        }
	    }
	    public static Color LightGray
	    {
	        get
	        {
	            return new Color(0xffd3d3d3);
	        }
	    }
	    public static Color LightPink
	    {
	        get
	        {
	            return new Color(0xffc1b6ff);
	        }
	    }
	    public static Color LightSalmon
	    {
	        get
	        {
	            return new Color(0xff7aa0ff);
	        }
	    }
	    public static Color LightSeaGreen
	    {
	        get
	        {
	            return new Color(0xffaab220);
	        }
	    }
	    public static Color LightSkyBlue
	    {
	        get
	        {
	            return new Color(0xffface87);
	        }
	    }
	    public static Color LightSlateGray
	    {
	        get
	        {
	            return new Color(0xff998877);
	        }
	    }
	    public static Color LightSteelBlue
	    {
	        get
	        {
	            return new Color(0xffdec4b0);
	        }
	    }
	    public static Color LightYellow
	    {
	        get
	        {
	            return new Color(0xffe0ffff);
	        }
	    }
	    public static Color Lime
	    {
	        get
	        {
	            return new Color(0xff00ff00);
	        }
	    }
	    public static Color LimeGreen
	    {
	        get
	        {
	            return new Color(0xff32cd32);
	        }
	    }
	    public static Color Linen
	    {
	        get
	        {
	            return new Color(0xffe6f0fa);
	        }
	    }
	    public static Color Magenta
	    {
	        get
	        {
	            return new Color(0xffff00ff);
	        }
	    }
	    public static Color Maroon
	    {
	        get
	        {
	            return new Color(0xff000080);
	        }
	    }
	    public static Color MediumAquamarine
	    {
	        get
	        {
	            return new Color(0xffaacd66);
	        }
	    }
	    public static Color MediumBlue
	    {
	        get
	        {
	            return new Color(0xffcd0000);
	        }
	    }
	    public static Color MediumOrchid
	    {
	        get
	        {
	            return new Color(0xffd355ba);
	        }
	    }
	    public static Color MediumPurple
	    {
	        get
	        {
	            return new Color(0xffdb7093);
	        }
	    }
	    public static Color MediumSeaGreen
	    {
	        get
	        {
	            return new Color(0xff71b33c);
	        }
	    }
	    public static Color MediumSlateBlue
	    {
	        get
	        {
	            return new Color(0xffee687b);
	        }
	    }
	    public static Color MediumSpringGreen
	    {
	        get
	        {
	            return new Color(0xff9afa00);
	        }
	    }
	    public static Color MediumTurquoise
	    {
	        get
	        {
	            return new Color(0xffccd148);
	        }
	    }
	    public static Color MediumVioletRed
	    {
	        get
	        {
	            return new Color(0xff8515c7);
	        }
	    }
	    public static Color MidnightBlue
	    {
	        get
	        {
	            return new Color(0xff701919);
	        }
	    }
	    public static Color MintCream
	    {
	        get
	        {
	            return new Color(0xfffafff5);
	        }
	    }
	    public static Color MistyRose
	    {
	        get
	        {
	            return new Color(0xffe1e4ff);
	        }
	    }
	    public static Color Moccasin
	    {
	        get
	        {
	            return new Color(0xffb5e4ff);
	        }
	    }
	    public static Color NavajoWhite
	    {
	        get
	        {
	            return new Color(0xffaddeff);
	        }
	    }
	    public static Color Navy
	    {
	        get
	        {
	            return new Color(0xff800000);
	        }
	    }
	    public static Color OldLace
	    {
	        get
	        {
	            return new Color(0xffe6f5fd);
	        }
	    }
	    public static Color Olive
	    {
	        get
	        {
	            return new Color(0xff008080);
	        }
	    }
	    public static Color OliveDrab
	    {
	        get
	        {
	            return new Color(0xff238e6b);
	        }
	    }
	    public static Color Orange
	    {
	        get
	        {
	            return new Color(0xff00a5ff);
	        }
	    }
	    public static Color OrangeRed
	    {
	        get
	        {
	            return new Color(0xff0045ff);
	        }
	    }
	    public static Color Orchid
	    {
	        get
	        {
	            return new Color(0xffd670da);
	        }
	    }
	    public static Color PaleGoldenrod
	    {
	        get
	        {
	            return new Color(0xffaae8ee);
	        }
	    }
	    public static Color PaleGreen
	    {
	        get
	        {
	            return new Color(0xff98fb98);
	        }
	    }
	    public static Color PaleTurquoise
	    {
	        get
	        {
	            return new Color(0xffeeeeaf);
	        }
	    }
	    public static Color PaleVioletRed
	    {
	        get
	        {
	            return new Color(0xff9370db);
	        }
	    }
	    public static Color PapayaWhip
	    {
	        get
	        {
	            return new Color(0xffd5efff);
	        }
	    }
	    public static Color PeachPuff
	    {
	        get
	        {
	            return new Color(0xffb9daff);
	        }
	    }
	    public static Color Peru
	    {
	        get
	        {
	            return new Color(0xff3f85cd);
	        }
	    }
	    public static Color Pink
	    {
	        get
	        {
	            return new Color(0xffcbc0ff);
	        }
	    }
	    public static Color Plum
	    {
	        get
	        {
	            return new Color(0xffdda0dd);
	        }
	    }
	    public static Color PowderBlue
	    {
	        get
	        {
	            return new Color(0xffe6e0b0);
	        }
	    }
	    public static Color Purple
	    {
	        get
	        {
	            return new Color(0xff800080);
	        }
	    }
	    public static Color Red
	    {
	        get
	        {
	            return new Color(0xff0000ff);
	        }
	    }
	    public static Color RosyBrown
	    {
	        get
	        {
	            return new Color(0xff8f8fbc);
	        }
	    }
	    public static Color RoyalBlue
	    {
	        get
	        {
	            return new Color(0xffe16941);
	        }
	    }
	    public static Color SaddleBrown
	    {
	        get
	        {
	            return new Color(0xff13458b);
	        }
	    }
	    public static Color Salmon
	    {
	        get
	        {
	            return new Color(0xff7280fa);
	        }
	    }
	    public static Color SandyBrown
	    {
	        get
	        {
	            return new Color(0xff60a4f4);
	        }
	    }
	    public static Color SeaGreen
	    {
	        get
	        {
	            return new Color(0xff578b2e);
	        }
	    }
	    public static Color SeaShell
	    {
	        get
	        {
	            return new Color(0xffeef5ff);
	        }
	    }
	    public static Color Sienna
	    {
	        get
	        {
	            return new Color(0xff2d52a0);
	        }
	    }
	    public static Color Silver
	    {
	        get
	        {
	            return new Color(0xffc0c0c0);
	        }
	    }
	    public static Color SkyBlue
	    {
	        get
	        {
	            return new Color(0xffebce87);
	        }
	    }
	    public static Color SlateBlue
	    {
	        get
	        {
	            return new Color(0xffcd5a6a);
	        }
	    }
	    public static Color SlateGray
	    {
	        get
	        {
	            return new Color(0xff908070);
	        }
	    }
	    public static Color Snow
	    {
	        get
	        {
	            return new Color(0xfffafaff);
	        }
	    }
	    public static Color SpringGreen
	    {
	        get
	        {
	            return new Color(0xff7fff00);
	        }
	    }
	    public static Color SteelBlue
	    {
	        get
	        {
	            return new Color(0xffb48246);
	        }
	    }
	    public static Color Tan
	    {
	        get
	        {
	            return new Color(0xff8cb4d2);
	        }
	    }
	    public static Color Teal
	    {
	        get
	        {
	            return new Color(0xff808000);
	        }
	    }
	    public static Color Thistle
	    {
	        get
	        {
	            return new Color(0xffd8bfd8);
	        }
	    }
	    public static Color Tomato
	    {
	        get
	        {
	            return new Color(0xff4763ff);
	        }
	    }
	    public static Color Turquoise
	    {
	        get
	        {
	            return new Color(0xffd0e040);
	        }
	    }
	    public static Color Violet
	    {
	        get
	        {
	            return new Color(0xffee82ee);
	        }
	    }
	    public static Color Wheat
	    {
	        get
	        {
	            return new Color(0xffb3def5);
	        }
	    }
	    public static Color White
	    {
	        get
	        {
	            return new Color(uint.MaxValue);
	        }
	    }
	    public static Color WhiteSmoke
	    {
	        get
	        {
	            return new Color(0xfff5f5f5);
	        }
	    }
	    public static Color Yellow
	    {
	        get
	        {
	            return new Color(0xff00ffff);
	        }
	    }
	    public static Color YellowGreen
	    {
	        get
	        {
	            return new Color(0xff32cd9a);
	        }
	    }

        public static Color Lerp(Color value1, Color value2, Single amount)
        {
            byte Red   = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.R, value2.R, amount), Byte.MinValue, Byte.MaxValue);   
			byte Green = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.G, value2.G, amount), Byte.MinValue, Byte.MaxValue);
			byte Blue  = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.B, value2.B, amount), Byte.MinValue, Byte.MaxValue);
			byte Alpha = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.A, value2.A, amount), Byte.MinValue, Byte.MaxValue);
			
            return new Color( Red, Green, Blue, Alpha );
        }
		
		public static Color Multiply( Color value, float scale)
		{
			byte Red = (byte)(value.R * scale);
			byte Green = (byte)(value.G * scale);
			byte Blue = (byte)(value.B * scale);
			byte Alpha = (byte)(value.A * scale);
			
			return new Color( Red, Green, Blue, Alpha );
		}
		
		public static Color operator *(Color value, float scale)
        {
            return Multiply(value, scale);
        }		

        public Vector3 ToVector3()
        {
            return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
        }

        public Vector4 ToVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        public UInt32 PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }
		
		public override string ToString ()
		{
			return string.Format("[Color: R={0}, G={1}, B={2}, A={3}, PackedValue={4}]", R, G, B, A, PackedValue);
		}

        public static Color FromNonPremultiplied(Vector4 vector)
        {
            return new Color(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W, vector.W);
        }

        public static Color FromNonPremultiplied(int r, int g, int b, int a)
        {
            return new Color((byte)(r * a / 255),(byte)(g * a / 255), (byte)(b * a / 255), a);
        }

        #region IEquatable<Color> Members

        public bool Equals(Color other)
        {
			return this.PackedValue == other.PackedValue;
        }

        #endregion
    }
}
