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
    /// <summary>
    /// Describe a 32-bit packed color.
    /// </summary>
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
	
	/// <summary>
        /// Gets or sets the green component of <see cref="Color"/>.
        /// </summary>
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
	
	/// <summary>
        /// Gets or sets the red component of <see cref="Color"/>.
        /// </summary>
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

	/// <summary>
        /// Gets or sets the alpha component of <see cref="Color"/>.
        /// </summary>
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
        /// TransparentBlack color (R:0,G:0,B:0,A:0).
        /// </summary>
        public static Color TransparentBlack
        {
            get
            {
                return new Color(0);
            }
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
        /// DarkKhaki color (R:189,G:183,B:107,A:255).
        /// </summary>
        public static Color DarkKhaki
        {
            get
            {
                return new Color(0xff6bb7bd);
            }
        }

        /// <summary>
        /// DarkMagenta color (R:139,G:0,B:139,A:255).
        /// </summary>
        public static Color DarkMagenta
        {
            get
            {
                return new Color(0xff8b008b);
            }
        }

        /// <summary>
        /// DarkOliveGreen color (R:85,G:107,B:47,A:255).
        /// </summary>
        public static Color DarkOliveGreen
        {
            get
            {
                return new Color(0xff2f6b55);
            }
        }

        /// <summary>
        /// DarkOrange color (R:255,G:140,B:0,A:255).
        /// </summary>
        public static Color DarkOrange
        {
            get
            {
                return new Color(0xff008cff);
            }
        }

        /// <summary>
        /// DarkOrchid color (R:153,G:50,B:204,A:255).
        /// </summary>
        public static Color DarkOrchid
        {
            get
            {
                return new Color(0xffcc3299);
            }
        }

        /// <summary>
        /// DarkRed color (R:139,G:0,B:0,A:255).
        /// </summary>
        public static Color DarkRed
        {
            get
            {
                return new Color(0xff00008b);
            }
        }
        
	/// <summary>
        /// DarkSalmon color (R:233,G:150,B:122,A:255).
        /// </summary>
        public static Color DarkSalmon
        {
            get
            {
                return new Color(0xff7a96e9);
            }
        }

        /// <summary>
        /// DarkSeaGreen color (R:143,G:188,B:139,A:255).
        /// </summary>
        public static Color DarkSeaGreen
        {
            get
            {
                return new Color(0xff8bbc8f);
            }
        }

        /// <summary>
        /// DarkSlateBlue color (R:72,G:61,B:139,A:255).
        /// </summary>
        public static Color DarkSlateBlue
        {
            get
            {
                return new Color(0xff8b3d48);
            }
        }

        /// <summary>
        /// DarkSlateGray color (R:47,G:79,B:79,A:255).
        /// </summary>
        public static Color DarkSlateGray
        {
            get
            {
                return new Color(0xff4f4f2f);
            }
        }

        /// <summary>
        /// DarkTurquoise color (R:0,G:206,B:209,A:255).
        /// </summary>
        public static Color DarkTurquoise
        {
            get
            {
                return new Color(0xffd1ce00);
            }
        }

        /// <summary>
        /// DarkViolet color (R:148,G:0,B:211,A:255).
        /// </summary>
        public static Color DarkViolet
        {
            get
            {
                return new Color(0xffd30094);
            }
        }
         
        /// <summary>
        /// DeepPink color (R:255,G:20,B:147,A:255).
        /// </summary>
        public static Color DeepPink
        {
            get
            {
                return new Color(0xff9314ff);
            }
        }

        /// <summary>
        /// DeepSkyBlue color (R:0,G:191,B:255,A:255).
        /// </summary>
        public static Color DeepSkyBlue
        {
            get
            {
                return new Color(0xffffbf00);
            }
        }

        /// <summary>
        /// DimGray color (R:105,G:105,B:105,A:255).
        /// </summary>
        public static Color DimGray
        {
            get
            {
                return new Color(0xff696969);
            }
        }

        /// <summary>
        /// DodgerBlue color (R:30,G:144,B:255,A:255).
        /// </summary>
        public static Color DodgerBlue
        {
            get
            {
                return new Color(0xffff901e);
            }
        }

        /// <summary>
        /// Firebrick color (R:178,G:34,B:34,A:255).
        /// </summary>
        public static Color Firebrick
        {
            get
            {
                return new Color(0xff2222b2);
            }
        }

        /// <summary>
        /// FloralWhite color (R:255,G:250,B:240,A:255).
        /// </summary>
        public static Color FloralWhite
        {
            get
            {
                return new Color(0xfff0faff);
            }
        }

        /// <summary>
        /// ForestGreen color (R:34,G:139,B:34,A:255).
        /// </summary>
        public static Color ForestGreen
        {
            get
            {
                return new Color(0xff228b22);
            }
        }
        
	/// <summary>
        /// Fuchsia color (R:255,G:0,B:255,A:255).
        /// </summary>
        public static Color Fuchsia
        {
            get
            {
                return new Color(0xffff00ff);
            }
        }

        /// <summary>
        /// Gainsboro color (R:220,G:220,B:220,A:255).
        /// </summary>
        public static Color Gainsboro
        {
            get
            {
                return new Color(0xffdcdcdc);
            }
        }

        /// <summary>
        /// GhostWhite color (R:248,G:248,B:255,A:255).
        /// </summary>
        public static Color GhostWhite
        {
            get
            {
                return new Color(0xfffff8f8);
            }
        }

        /// <summary>
        /// Gold color (R:255,G:215,B:0,A:255).
        /// </summary>
        public static Color Gold
        {
            get
            {
                return new Color(0xff00d7ff);
            }
        }

        /// <summary>
        /// Goldenrod color (R:218,G:165,B:32,A:255).
        /// </summary>
        public static Color Goldenrod
        {
            get
            {
                return new Color(0xff20a5da);
            }
        }

        /// <summary>
        /// Gray color (R:128,G:128,B:128,A:255).
        /// </summary>
        public static Color Gray
        {
            get
            {
                return new Color(0xff808080);
            }
        }

        /// <summary>
        /// Green color (R:0,G:128,B:0,A:255).
        /// </summary>
        public static Color Green
        {
            get
            {
                return new Color(0xff008000);
            }
        }

        /// <summary>
        /// GreenYellow color (R:173,G:255,B:47,A:255).
        /// </summary>
        public static Color GreenYellow
        {
            get
            {
                return new Color(0xff2fffad);
            }
        }

        /// <summary>
        /// Honeydew color (R:240,G:255,B:240,A:255).
        /// </summary>
        public static Color Honeydew
        {
            get
            {
                return new Color(0xfff0fff0);
            }
        }

        /// <summary>
        /// HotPink color (R:255,G:105,B:180,A:255).
        /// </summary>
        public static Color HotPink
        {
            get
            {
                return new Color(0xffb469ff);
            }
        }
        
        /// <summary>
        /// IndianRed color (R:205,G:92,B:92,A:255).
        /// </summary>
        public static Color IndianRed
        {
            get
            {
                return new Color(0xff5c5ccd);
            }
        }
        
        /// <summary>
        /// Indigo color (R:75,G:0,B:130,A:255).
        /// </summary>
        public static Color Indigo
        {
            get
            {
                return new Color(0xff82004b);
            }
        }
        
        /// <summary>
        /// Ivory color (R:255,G:255,B:240,A:255).
        /// </summary>
        public static Color Ivory
        {
            get
            {
                return new Color(0xfff0ffff);
            }
        }
        
        /// <summary>
        /// Khaki color (R:240,G:230,B:140,A:255).
        /// </summary>
        public static Color Khaki
        {
            get
            {
                return new Color(0xff8ce6f0);
            }
        }
        
        /// <summary>
        /// Lavender color (R:230,G:230,B:250,A:255).
        /// </summary>
        public static Color Lavender
        {
            get
            {
                return new Color(0xfffae6e6);
            }
        }
        
        /// <summary>
        /// LavenderBlush color (R:255,G:240,B:245,A:255).
        /// </summary>
        public static Color LavenderBlush
        {
            get
            {
                return new Color(0xfff5f0ff);
            }
        }
        
        /// <summary>
        /// LawnGreen color (R:124,G:252,B:0,A:255).
        /// </summary>
        public static Color LawnGreen
        {
            get
            {
                return new Color(0xff00fc7c);
            }
        }

        /// <summary>
        /// LemonChiffon color (R:255,G:250,B:205,A:255).
        /// </summary>
        public static Color LemonChiffon
        {
            get
            {
                return new Color(0xffcdfaff);
            }
        }

        /// <summary>
        /// LightBlue color (R:173,G:216,B:230,A:255).
        /// </summary>
        public static Color LightBlue
        {
            get
            {
                return new Color(0xffe6d8ad);
            }
        }

        /// <summary>
        /// LightCoral color (R:240,G:128,B:128,A:255).
        /// </summary>
        public static Color LightCoral
        {
            get
            {
                return new Color(0xff8080f0);
            }
        }
        
        /// <summary>
        /// LightCyan color (R:224,G:255,B:255,A:255).
        /// </summary>
        public static Color LightCyan
        {
            get
            {
                return new Color(0xffffffe0);
            }
        }

        /// <summary>
        /// LightGoldenrodYellow color (R:250,G:250,B:210,A:255).
        /// </summary>
        public static Color LightGoldenrodYellow
        {
            get
            {
                return new Color(0xffd2fafa);
            }
        }
        
        /// <summary>
        /// LightGray color (R:211,G:211,B:211,A:255).
        /// </summary>
        public static Color LightGray
        {
            get
            {
                return new Color(0xffd3d3d3);
            }
        }

        /// <summary>
        /// LightGreen color (R:144,G:238,B:144,A:255).
        /// </summary>
        public static Color LightGreen
        {
            get
            {
                return new Color(0xff90ee90);
            }
        }

        /// <summary>
        /// LightPink color (R:255,G:182,B:193,A:255).
        /// </summary>
        public static Color LightPink
        {
            get
            {
                return new Color(0xffc1b6ff);
            }
        }

        /// <summary>
        /// LightSalmon color (R:255,G:160,B:122,A:255).
        /// </summary>
        public static Color LightSalmon
        {
            get
            {
                return new Color(0xff7aa0ff);
            }
        }

        /// <summary>
        /// LightSeaGreen color (R:32,G:178,B:170,A:255).
        /// </summary>
        public static Color LightSeaGreen
        {
            get
            {
                return new Color(0xffaab220);
            }
        }

        /// <summary>
        /// LightSkyBlue color (R:135,G:206,B:250,A:255).
        /// </summary>
        public static Color LightSkyBlue
        {
            get
            {
                return new Color(0xffface87);
            }
        }

        /// <summary>
        /// LightSlateGray color (R:119,G:136,B:153,A:255).
        /// </summary>
        public static Color LightSlateGray
        {
            get
            {
                return new Color(0xff998877);
            }
        }

        /// <summary>
        /// LightSteelBlue color (R:176,G:196,B:222,A:255).
        /// </summary>
        public static Color LightSteelBlue
        {
            get
            {
                return new Color(0xffdec4b0);
            }
        }

        /// <summary>
        /// LightYellow color (R:255,G:255,B:224,A:255).
        /// </summary>
        public static Color LightYellow
        {
            get
            {
                return new Color(0xffe0ffff);
            }
        }

        /// <summary>
        /// Lime color (R:0,G:255,B:0,A:255).
        /// </summary>
        public static Color Lime
        {
            get
            {
                return new Color(0xff00ff00);
            }
        }

        /// <summary>
        /// LimeGreen color (R:50,G:205,B:50,A:255).
        /// </summary>
        public static Color LimeGreen
        {
            get
            {
                return new Color(0xff32cd32);
            }
        }

        /// <summary>
        /// Linen color (R:250,G:240,B:230,A:255).
        /// </summary>
        public static Color Linen
        {
            get
            {
                return new Color(0xffe6f0fa);
            }
        }

        /// <summary>
        /// Magenta color (R:255,G:0,B:255,A:255).
        /// </summary>
        public static Color Magenta
        {
            get
            {
                return new Color(0xffff00ff);
            }
        }

        /// <summary>
        /// Maroon color (R:128,G:0,B:0,A:255).
        /// </summary>
        public static Color Maroon
        {
            get
            {
                return new Color(0xff000080);
            }
        }

        /// <summary>
        /// MediumAquamarine color (R:102,G:205,B:170,A:255).
        /// </summary>
        public static Color MediumAquamarine
        {
            get
            {
                return new Color(0xffaacd66);
            }
        }

        /// <summary>
        /// MediumBlue color (R:0,G:0,B:205,A:255).
        /// </summary>
        public static Color MediumBlue
        {
            get
            {
                return new Color(0xffcd0000);
            }
        }

        /// <summary>
        /// MediumOrchid color (R:186,G:85,B:211,A:255).
        /// </summary>
        public static Color MediumOrchid
        {
            get
            {
                return new Color(0xffd355ba);
            }
        }

        /// <summary>
        /// MediumPurple color (R:147,G:112,B:219,A:255).
        /// </summary>
        public static Color MediumPurple
        {
            get
            {
                return new Color(0xffdb7093);
            }
        }
        /// <summary>
        /// MediumSeaGreen color (R:60,G:179,B:113,A:255).
        /// </summary>
        public static Color MediumSeaGreen
        {
            get
            {
                return new Color(0xff71b33c);
            }
        }

        /// <summary>
        /// MediumSlateBlue color (R:123,G:104,B:238,A:255).
        /// </summary>
        public static Color MediumSlateBlue
        {
            get
            {
                return new Color(0xffee687b);
            }
        }

        /// <summary>
        /// MediumSpringGreen color (R:0,G:250,B:154,A:255).
        /// </summary>
        public static Color MediumSpringGreen
        {
            get
            {
                return new Color(0xff9afa00);
            }
        }

        /// <summary>
        /// MediumTurquoise color (R:72,G:209,B:204,A:255).
        /// </summary>
        public static Color MediumTurquoise
        {
            get
            {
                return new Color(0xffccd148);
            }
        }

        /// <summary>
        /// MediumVioletRed color (R:199,G:21,B:133,A:255).
        /// </summary>
        public static Color MediumVioletRed
        {
            get
            {
                return new Color(0xff8515c7);
            }
        }

        /// <summary>
        /// MidnightBlue color (R:25,G:25,B:112,A:255).
        /// </summary>
        public static Color MidnightBlue
        {
            get
            {
                return new Color(0xff701919);
            }
        }

        /// <summary>
        /// MintCream color (R:245,G:255,B:250,A:255).
        /// </summary>
        public static Color MintCream
        {
            get
            {
                return new Color(0xfffafff5);
            }
        }

        /// <summary>
        /// MistyRose color (R:255,G:228,B:225,A:255).
        /// </summary>
        public static Color MistyRose
        {
            get
            {
                return new Color(0xffe1e4ff);
            }
        }

        /// <summary>
        /// Moccasin color (R:255,G:228,B:181,A:255).
        /// </summary>
        public static Color Moccasin
        {
            get
            {
                return new Color(0xffb5e4ff);
            }
        }

        /// <summary>
        /// NavajoWhite color (R:255,G:222,B:173,A:255).
        /// </summary>
        public static Color NavajoWhite
        {
            get
            {
                return new Color(0xffaddeff);
            }
        }

        /// <summary>
        /// Navy color (R:0,G:0,B:128,A:255).
        /// </summary>
        public static Color Navy
        {
            get
            {
                return new Color(0xff800000);
            }
        }

        /// <summary>
        /// OldLace color (R:253,G:245,B:230,A:255).
        /// </summary>
        public static Color OldLace
        {
            get
            {
                return new Color(0xffe6f5fd);
            }
        }

        /// <summary>
        /// Olive color (R:128,G:128,B:0,A:255).
        /// </summary>
        public static Color Olive
        {
            get
            {
                return new Color(0xff008080);
            }
        }

        /// <summary>
        /// OliveDrab color (R:107,G:142,B:35,A:255).
        /// </summary>
        public static Color OliveDrab
        {
            get
            {
                return new Color(0xff238e6b);
            }
        }

        /// <summary>
        /// Orange color (R:255,G:165,B:0,A:255).
        /// </summary>
        public static Color Orange
        {
            get
            {
                return new Color(0xff00a5ff);
            }
        }

        /// <summary>
        /// OrangeRed color (R:255,G:69,B:0,A:255).
        /// </summary>
        public static Color OrangeRed
        {
            get
            {
                return new Color(0xff0045ff);
            }
        }

        /// <summary>
        /// Orchid color (R:218,G:112,B:214,A:255).
        /// </summary>
        public static Color Orchid
        {
            get
            {
                return new Color(0xffd670da);
            }
        }

        /// <summary>
        /// PaleGoldenrod color (R:238,G:232,B:170,A:255).
        /// </summary>
        public static Color PaleGoldenrod
        {
            get
            {
                return new Color(0xffaae8ee);
            }
        }

        /// <summary>
        /// PaleGreen color (R:152,G:251,B:152,A:255).
        /// </summary>
        public static Color PaleGreen
        {
            get
            {
                return new Color(0xff98fb98);
            }
        }

        /// <summary>
        /// PaleTurquoise color (R:175,G:238,B:238,A:255).
        /// </summary>
        public static Color PaleTurquoise
        {
            get
            {
                return new Color(0xffeeeeaf);
            }
        }
        /// <summary>
        /// PaleVioletRed color (R:219,G:112,B:147,A:255).
        /// </summary>
        public static Color PaleVioletRed
        {
            get
            {
                return new Color(0xff9370db);
            }
        }

        /// <summary>
        /// PapayaWhip color (R:255,G:239,B:213,A:255).
        /// </summary>
        public static Color PapayaWhip
        {
            get
            {
                return new Color(0xffd5efff);
            }
        }

        /// <summary>
        /// PeachPuff color (R:255,G:218,B:185,A:255).
        /// </summary>
        public static Color PeachPuff
        {
            get
            {
                return new Color(0xffb9daff);
            }
        }

        /// <summary>
        /// Peru color (R:205,G:133,B:63,A:255).
        /// </summary>
        public static Color Peru
        {
            get
            {
                return new Color(0xff3f85cd);
            }
        }

        /// <summary>
        /// Pink color (R:255,G:192,B:203,A:255).
        /// </summary>
        public static Color Pink
        {
            get
            {
                return new Color(0xffcbc0ff);
            }
        }

        /// <summary>
        /// Plum color (R:221,G:160,B:221,A:255).
        /// </summary>
        public static Color Plum
        {
            get
            {
                return new Color(0xffdda0dd);
            }
        }

        /// <summary>
        /// PowderBlue color (R:176,G:224,B:230,A:255).
        /// </summary>
        public static Color PowderBlue
        {
            get
            {
                return new Color(0xffe6e0b0);
            }
        }

        /// <summary>
        ///  Purple color (R:128,G:0,B:128,A:255).
        /// </summary>
        public static Color Purple
        {
            get
            {
                return new Color(0xff800080);
            }
        }

        /// <summary>
        /// Red color (R:255,G:0,B:0,A:255).
        /// </summary>
        public static Color Red
        {
            get
            {
                return new Color(0xff0000ff);
            }
        }

        /// <summary>
        /// RosyBrown color (R:188,G:143,B:143,A:255).
        /// </summary>
        public static Color RosyBrown
        {
            get
            {
                return new Color(0xff8f8fbc);
            }
        }

        /// <summary>
        /// RoyalBlue color (R:65,G:105,B:225,A:255).
        /// </summary>
        public static Color RoyalBlue
        {
            get
            {
                return new Color(0xffe16941);
            }
        }

    	/// <summary>
        /// SaddleBrown color (R:139,G:69,B:19,A:255).
        /// </summary>
        public static Color SaddleBrown
        {
            get
            {
                return new Color(0xff13458b);
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
        /// SandyBrown color (R:244,G:164,B:96,A:255).
        /// </summary>
        public static Color SandyBrown
        {
            get
            {
                return new Color(0xff60a4f4);
            }
        }
        
        /// <summary>
        /// SeaGreen color (R:46,G:139,B:87,A:255).
        /// </summary>
        public static Color SeaGreen
        {
            get
            {
                return new Color(0xff578b2e);
            }
        }
        
    	/// <summary>
        /// SeaShell color (R:255,G:245,B:238,A:255).
        /// </summary>
        public static Color SeaShell
        {
            get
            {
                return new Color(0xffeef5ff);
            }
        }
        
    	/// <summary>
        /// Sienna color (R:160,G:82,B:45,A:255).
        /// </summary>
        public static Color Sienna
        {
            get
            {
                return new Color(0xff2d52a0);
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
            byte Red   = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.R, value2.R, amount), Byte.MinValue, Byte.MaxValue);   
			byte Green = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.G, value2.G, amount), Byte.MinValue, Byte.MaxValue);
			byte Blue  = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.B, value2.B, amount), Byte.MinValue, Byte.MaxValue);
			byte Alpha = (byte)MathHelper.Clamp(MathHelper.Lerp(value1.A, value2.A, amount), Byte.MinValue, Byte.MaxValue);
			
            return new Color( Red, Green, Blue, Alpha );
        }
		
	/// <summary>
        /// Multiply <see cref="Color"/> by value.
        /// </summary>
        /// <param name="value">Source <see cref="Color"/>.</param>
        /// <param name="scale">Multiplicator.</param>
        /// <returns>Multiplication result.</returns>
	public static Color Multiply( Color value, float scale)
	{
	    byte Red = (byte)(MathHelper.Clamp(value.R * scale, Byte.MinValue, Byte.MaxValue));
	    byte Green = (byte)(MathHelper.Clamp(value.G * scale, Byte.MinValue, Byte.MaxValue));
	    byte Blue = (byte)(MathHelper.Clamp(value.B * scale, Byte.MinValue, Byte.MaxValue));
	    byte Alpha = (byte)(MathHelper.Clamp(value.A * scale, Byte.MinValue, Byte.MaxValue)); 
	    return new Color( Red, Green, Blue, Alpha );
	}
	
	/// <summary>
        /// Multiply <see cref="Color"/> by value.
        /// </summary>
        /// <param name="value">Source <see cref="Color"/>.</param>
        /// <param name="scale">Multiplicator.</param>
        /// <returns>Multiplication result.</returns>
	public static Color operator *(Color value, float scale)
        {
            return Multiply(value, scale);
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
	public override string ToString ()
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
            return new Color((byte)(r * a / 255),(byte)(g * a / 255), (byte)(b * a / 255), a);
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
