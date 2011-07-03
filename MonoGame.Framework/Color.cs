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

namespace Microsoft.Xna.Framework
{
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

        public Color(Color color, byte alpha)
        {
            _packedValue = 0;

            R = color.R;
            G = color.G;
            B = color.B;
            A = alpha;
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
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)255;
        }


        public Color(int r, int g, int b, int alpha)
        {
            _packedValue = 0;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)alpha;
        }

        public Color(float r, float g, float b, float alpha)
        {
            _packedValue = 0;
			
            R = (byte)MathHelper.Clamp(r * 255, Byte.MinValue, Byte.MaxValue);
            G = (byte)MathHelper.Clamp(g * 255, Byte.MinValue, Byte.MaxValue);
            B = (byte)MathHelper.Clamp(b * 255, Byte.MinValue, Byte.MaxValue);
            A = (byte)MathHelper.Clamp(alpha * 255, Byte.MinValue, Byte.MaxValue);
        }

        public byte B
        {
            get
            {
                return (byte)this._packedValue;
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffffff00) | value;
            }
        }

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
        public byte R
        {
            get
            {
                return (byte)(this._packedValue >> 16);
            }
            set
            {
                this._packedValue = (this._packedValue & 0xff00ffff) | ((uint)(value << 16 ));
            }
        }
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
		
		public uint GLPackedValue
		{
			get { return (_packedValue & 0xff00ff00) | ((_packedValue & 0x000000ff) << 16) | ((_packedValue & 0x00ff0000) >> 16); }  
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
			get{
				return new Color(0);
			}
		}
        public static Color TransparentWhite
        {
            get
            {
                return new Color(0xffffff);
            }
        }
        public static Color AliceBlue
        {
            get
            {
                return new Color(0xfff0f8ff);
            }
        }
        public static Color AntiqueWhite
        {
            get
            {
                return new Color(0xfffaebd7);
            }
        }
        public static Color Aqua
        {
            get
            {
                return new Color(0xff00ffff);
            }
        }
        public static Color Aquamarine
        {
            get
            {
                return new Color(0xff7fffd4);
            }
        }
        public static Color Azure
        {
            get
            {
                return new Color(0xfff0ffff);
            }
        }
        public static Color Beige
        {
            get
            {
                return new Color(0xfff5f5dc);
            }
        }
        public static Color Bisque
        {
            get
            {
                return new Color(0xffffe4c4);
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
                return new Color(0xffffebcd);
            }
        }
        public static Color Blue
        {
            get
            {
                return new Color(0xff0000ff);
            }
        }
        public static Color BlueViolet
        {
            get
            {
                return new Color(0xff8a2be2);
            }
        }
        public static Color Brown
        {
            get
            {
                return new Color(0xffa52a2a);
            }
        }
        public static Color BurlyWood
        {
            get
            {
                return new Color(0xffdeb887);
            }
        }
        public static Color CadetBlue
        {
            get
            {
                return new Color(0xff5f9ea0);
            }
        }
        public static Color Chartreuse
        {
            get
            {
                return new Color(0xff7fff00);
            }
        }
        public static Color Chocolate
        {
            get
            {
                return new Color(0xffd2691e);
            }
        }
        public static Color Coral
        {
            get
            {
                return new Color(0xffff7f50);
            }
        }
        public static Color CornflowerBlue
        {
            get
            {
                return new Color(0xff6495ed);
            }
        }
        public static Color Cornsilk
        {
            get
            {
                return new Color(0xfffff8dc);
            }
        }
        public static Color Crimson
        {
            get
            {
                return new Color(0xffdc143c);
            }
        }
        public static Color Cyan
        {
            get
            {
                return new Color(0xff00ffff);
            }
        }
        public static Color DarkBlue
        {
            get
            {
                return new Color(0xff00008b);
            }
        }
        public static Color DarkCyan
        {
            get
            {
                return new Color(0xff008b8b);
            }
        }
        public static Color DarkGoldenrod
        {
            get
            {
                return new Color(0xffb8860b);
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
                return new Color(0xffbdb76b);
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
                return new Color(0xff556b2f);
            }
        }
        public static Color DarkOrange
        {
            get
            {
                return new Color(0xffff8c00);
            }
        }
        public static Color DarkOrchid
        {
            get
            {
                return new Color(0xff9932cc);
            }
        }
        public static Color DarkRed
        {
            get
            {
                return new Color(0xff8b0000);
            }
        }
        public static Color DarkSalmon
        {
            get
            {
                return new Color(0xffe9967a);
            }
        }
        public static Color DarkSeaGreen
        {
            get
            {
                return new Color(0xff8fbc8b);
            }
        }
        public static Color DarkSlateBlue
        {
            get
            {
                return new Color(0xff483d8b);
            }
        }
        public static Color DarkSlateGray
        {
            get
            {
                return new Color(0xff2f4f4f);
            }
        }
        public static Color DarkTurquoise
        {
            get
            {
                return new Color(0xff00ced1);
            }
        }
        public static Color DarkViolet
        {
            get
            {
                return new Color(0xff9400d3);
            }
        }
        public static Color DeepPink
        {
            get
            {
                return new Color(0xffff1493);
            }
        }
        public static Color DeepSkyBlue
        {
            get
            {
                return new Color(0xff00bfff);
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
                return new Color(0xff1e90ff);
            }
        }
        public static Color Firebrick
        {
            get
            {
                return new Color(0xffb22222);
            }
        }
        public static Color FloralWhite
        {
            get
            {
                return new Color(0xfffffaf0);
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
                return new Color(0xfff8f8ff);
            }
        }
        public static Color Gold
        {
            get
            {
                return new Color(0xffffd700);
            }
        }
        public static Color Goldenrod
        {
            get
            {
                return new Color(0xffdaa520);
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
                return new Color(0xffadff2f);
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
                return new Color(0xffff69b4);
            }
        }
        public static Color IndianRed
        {
            get
            {
                return new Color(0xffcd5c5c);
            }
        }
        public static Color Indigo
        {
            get
            {
                return new Color(0xff4b0082);
            }
        }
        public static Color Ivory
        {
            get
            {
                return new Color(0xfffffff0);
            }
        }
        public static Color Khaki
        {
            get
            {
                return new Color(0xfff0e68c);
            }
        }
        public static Color Lavender
        {
            get
            {
                return new Color(0xffe6e6fa);
            }
        }
        public static Color LavenderBlush
        {
            get
            {
                return new Color(0xfffff0f5);
            }
        }
        public static Color LawnGreen
        {
            get
            {
                return new Color(0xff7cfc00);
            }
        }
        public static Color LemonChiffon
        {
            get
            {
                return new Color(0xfffffacd);
            }
        }
        public static Color LightBlue
        {
            get
            {
                return new Color(0xffadd8e6);
            }
        }
        public static Color LightCoral
        {
            get
            {
                return new Color(0xfff08080);
            }
        }
        public static Color LightCyan
        {
            get
            {
                return new Color(0xffe0ffff);
            }
        }
        public static Color LightGoldenrodYellow
        {
            get
            {
                return new Color(0xfffafad2);
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
                return new Color(0xffffb6c1);
            }
        }
        public static Color LightSalmon
        {
            get
            {
                return new Color(0xffffa07a);
            }
        }
        public static Color LightSeaGreen
        {
            get
            {
                return new Color(0xff20b2aa);
            }
        }
        public static Color LightSkyBlue
        {
            get
            {
                return new Color(0xff87cefa);
            }
        }
        public static Color LightSlateGray
        {
            get
            {
                return new Color(0xff778899);
            }
        }
        public static Color LightSteelBlue
        {
            get
            {
                return new Color(0xffb0c4de);
            }
        }
        public static Color LightYellow
        {
            get
            {
                return new Color(0xffffffe0);
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
                return new Color(0xfffaf0e6);
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
                return new Color(0xff800000);
            }
        }
        public static Color MediumAquamarine
        {
            get
            {
                return new Color(0xff66cdaa);
            }
        }
        public static Color MediumBlue
        {
            get
            {
                return new Color(0xff0000cd);
            }
        }
        public static Color MediumOrchid
        {
            get
            {
                return new Color(0xffba55d3);
            }
        }
        public static Color MediumPurple
        {
            get
            {
                return new Color(0xff9370db);
            }
        }
        public static Color MediumSeaGreen
        {
            get
            {
                return new Color(0xff3cb371);
            }
        }
        public static Color MediumSlateBlue
        {
            get
            {
                return new Color(0xff7b68ee);
            }
        }
        public static Color MediumSpringGreen
        {
            get
            {
                return new Color(0xff00fa9a);
            }
        }
        public static Color MediumTurquoise
        {
            get
            {
                return new Color(0xff48d1cc);
            }
        }
        public static Color MediumVioletRed
        {
            get
            {
                return new Color(0xffc71585);
            }
        }
        public static Color MidnightBlue
        {
            get
            {
                return new Color(0xff191970);
            }
        }
        public static Color MintCream
        {
            get
            {
                return new Color(0xfff5fffa);
            }
        }
        public static Color MistyRose
        {
            get
            {
                return new Color(0xffffe4e1);
            }
        }
        public static Color Moccasin
        {
            get
            {
                return new Color(0xffffe4b5);
            }
        }
        public static Color NavajoWhite
        {
            get
            {
                return new Color(0xffffdead);
            }
        }
        public static Color Navy
        {
            get
            {
                return new Color(0xff000080);
            }
        }
        public static Color OldLace
        {
            get
            {
                return new Color(0xfffdf5e6);
            }
        }
        public static Color Olive
        {
            get
            {
                return new Color(0xff808000);
            }
        }
        public static Color OliveDrab
        {
            get
            {
                return new Color(0xff6b8e23);
            }
        }
        public static Color Orange
        {
            get
            {
                return new Color(0xffffa500);
            }
        }
        public static Color OrangeRed
        {
            get
            {
                return new Color(0xffff4500);
            }
        }
        public static Color Orchid
        {
            get
            {
                return new Color(0xffda70d6);
            }
        }
        public static Color PaleGoldenrod
        {
            get
            {
                return new Color(0xffeee8aa);
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
                return new Color(0xffafeeee);
            }
        }
        public static Color PaleVioletRed
        {
            get
            {
                return new Color(0xffdb7093);
            }
        }
        public static Color PapayaWhip
        {
            get
            {
                return new Color(0xffffefd5);
            }
        }
        public static Color PeachPuff
        {
            get
            {
                return new Color(0xffffdab9);
            }
        }
        public static Color Peru
        {
            get
            {
                return new Color(0xffcd853f);
            }
        }
        public static Color Pink
        {
            get
            {
                return new Color(0xffffc0cb);
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
                return new Color(0xffb0e0e6);
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
                return new Color(0xffff0000);
            }
        }
        public static Color RosyBrown
        {
            get
            {
                return new Color(0xffbc8f8f);
            }
        }
        public static Color RoyalBlue
        {
            get
            {
                return new Color(0xff4169e1);
            }
        }
        public static Color SaddleBrown
        {
            get
            {
                return new Color(0xff8b4513);
            }
        }
        public static Color Salmon
        {
            get
            {
                return new Color(0xfffa8072);
            }
        }
        public static Color SandyBrown
        {
            get
            {
                return new Color(0xfff4a460);
            }
        }
        public static Color SeaGreen
        {
            get
            {
                return new Color(0xff2e8b57);
            }
        }
        public static Color SeaShell
        {
            get
            {
                return new Color(0xfffff5ee);
            }
        }
        public static Color Sienna
        {
            get
            {
                return new Color(0xffa0522d);
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
                return new Color(0xff87ceeb);
            }
        }
        public static Color SlateBlue
        {
            get
            {
                return new Color(0xff6a5acd);
            }
        }
        public static Color SlateGray
        {
            get
            {
                return new Color(0xff708090);
            }
        }
        public static Color Snow
        {
            get
            {
                return new Color(0xfffffafa);
            }
        }
        public static Color SpringGreen
        {
            get
            {
                return new Color(0xff00ff7f);
            }
        }
        public static Color SteelBlue
        {
            get
            {
                return new Color(0xff4682b4);
            }
        }
        public static Color Tan
        {
            get
            {
                return new Color(0xffd2b48c);
            }
        }
        public static Color Teal
        {
            get
            {
                return new Color(0xff008080);
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
                return new Color(0xffff6347);
            }
        }
        public static Color Turquoise
        {
            get
            {
                return new Color(0xff40e0d0);
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
                return new Color(0xfff5deb3);
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
                return new Color(0xffffff00);
            }
        }
        public static Color YellowGreen
        {
            get
            {
                return new Color(0xff9acd32);
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
            Vector3 vector = new Vector3();
            vector.X = R/255.0f;
            vector.Y = G/255.0f;
            vector.Z = B/255.0f;
            return vector;
        }

        public Vector4 ToVector4()
        {
            return new Vector4(R/255.0f, G/255.0f, B/255.0f, A/255.0f);
        }
		
		public Vector4 ToEAGLColor()
		{
			float r = R/255.0f;
			float g = G/255.0f;
			float b = B/255.0f;
			float a = A/255.0f;
			
			return new Vector4(r, g, b, a);
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


        #region IEquatable<Color> Members

        public bool Equals(Color other)
        {
			return this.GLPackedValue == other.GLPackedValue;
        }

        #endregion
    }
}
