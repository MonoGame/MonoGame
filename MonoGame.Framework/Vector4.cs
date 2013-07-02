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
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    // Implementation note: See "Vectors.tt" for common methods between the vector types.
    public partial struct Vector4
    {
        #region Constructors

        /// <summary>Initialize the components of the <see cref="Vector4"/>.</summary>
        /// <param name="value">The <see cref="Vector2"/> to use for the values of the <see cref="X"/> and <see cref="Y"/> components.</param>
        /// <param name="z">The value to use for the <see cref="Z"/> component.</param>
        /// <param name="w">The value to use for the <see cref="W"/> component.</param>
        public Vector4(Vector2 value, float z, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>Initialize the components of the <see cref="Vector4"/>.</summary>
        /// <param name="value">The <see cref="Vector3"/> to use for the values of the <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/> components.</param>
        /// <param name="w">The value to use for the <see cref="W"/> component.</param>
        public Vector4(Vector3 value, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = value.Z;
            this.W = w;
        }

        #endregion
    }
}
