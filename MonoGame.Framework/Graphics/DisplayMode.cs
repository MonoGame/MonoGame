#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors:
 * Rob Loach

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
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    [DataContract]
    public class DisplayMode
    {
        #region Fields

        private SurfaceFormat format;
        private int height;
        private int width;

        #endregion Fields

        #region Properties
        
        public float AspectRatio {
            get { return (float)width / (float)height; }
        }

        public SurfaceFormat Format {
            get { return format; }
        }

        public int Height {
            get { return this.height; }
        }

        public int Width {
            get { return this.width; }
        }
        
        public Rectangle TitleSafeArea {
            get { return GraphicsDevice.GetTitleSafeArea(0, 0, width, height); }
        }

        #endregion Properties

        #region Constructors
        
        internal DisplayMode(int width, int height, SurfaceFormat format)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        #endregion Constructors

        #region Operators

        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }

        public static bool operator ==(DisplayMode left, DisplayMode right)
        {
            if (ReferenceEquals(left, right)) //Same object or both are null
            {
                return true;
            }
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }
            return (left.format == right.format) &&
                (left.height == right.height) &&
                (left.width == right.width);
        }

        #endregion Operators

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is DisplayMode && this == (DisplayMode)obj;
        }

        public override int GetHashCode()
        {
            return (this.width.GetHashCode() ^ this.height.GetHashCode() ^ this.format.GetHashCode());
        }

        public override string ToString()
        {
            return "{Width:" + this.width + " Height:" + this.height + " Format:" + this.Format + " AspectRatio:" + this.AspectRatio + "}";
        }

        #endregion Public Methods
    }
}
