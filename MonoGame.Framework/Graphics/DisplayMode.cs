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
    /// <summary>
    /// Describes the display mode.
    /// </summary>
    [DataContract]
    public class DisplayMode
    {
        #region Fields

        private SurfaceFormat format;
        private int height;
        private int width;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a value indicating the aspect ratio of the display mode
        /// </summary>
        public float AspectRatio {
            get { return (float)width / (float)height; }
        }

        /// <summary>
        /// Gets a value indicating the surface format of the display mode.
        /// </summary>
        public SurfaceFormat Format {
            get { return format; }
        }

        /// <summary>
        /// Gets a value indicating the screen height, in pixels.
        /// </summary>
        public int Height {
            get { return this.height; }
        }

        /// <summary>
        /// Gets a value indicating the screen width, in pixels.
        /// </summary>
        public int Width {
            get { return this.width; }
        }

        /// <summary>
        /// Gets the bounds of the display that is guaranteed to be visible by the users screen.
        /// </summary>
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

        /// <summary>
        /// Compares the current instance of the DisplayMode class to another instance to determine whether they are
        /// different.
        /// </summary>
        /// <param name="left">The DisplayMode object on the left of the inequality operator.</param>
        /// <param name="right">The DisplayMode object on the right of the inequality operator.</param>
        /// <returns>true if the objects are different; otherwise, false.</returns>
        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compares the current instance of the DisplayMode class to another instance to determine whether they are
        /// equal.
        /// </summary>
        /// <param name="left">The DisplayMode object on the left of the equality operator.</param>
        /// <param name="right">The DisplayMode object on the right of the equality operator.</param>
        /// <returns>true if the objects are equal; otherwise, false.</returns>
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

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is DisplayMode && this == (DisplayMode)obj;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this.width.GetHashCode() ^ this.height.GetHashCode() ^ this.format.GetHashCode());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "{Width:" + this.width + " Height:" + this.height + " Format:" + this.Format + " AspectRatio:" + this.AspectRatio + "}";
        }

        #endregion Public Methods
    }
}
