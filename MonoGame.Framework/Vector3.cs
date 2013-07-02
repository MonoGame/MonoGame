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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    // Implementation note: See "Vectors.tt" for common methods between the vector types.
    public partial struct Vector3
    {
        #region Private Fields

        /// <summary>Backing field for the <see cref="Up"/> property.</summary>
        private static Vector3 up = new Vector3(0f, 1f, 0f);

        /// <summary>Backing field for the <see cref="Down"/> property.</summary>
        private static Vector3 down = new Vector3(0f, -1f, 0f);

        /// <summary>Backing field for the <see cref="Right"/> property.</summary>
        private static Vector3 right = new Vector3(1f, 0f, 0f);

        /// <summary>Backing field for the <see cref="Left"/> property.</summary>
        private static Vector3 left = new Vector3(-1f, 0f, 0f);

        /// <summary>Backing field for the <see cref="Forward"/> property.</summary>
        private static Vector3 forward = new Vector3(0f, 0f, -1f);

        /// <summary>Backing field for the <see cref="Backward"/> property.</summary>
        private static Vector3 backward = new Vector3(0f, 0f, 1f);

        #endregion Private Fields


        #region Properties

        /// <summary>Get a {0, 1, 0} <see cref="Vector3"/>.</summary>
        public static Vector3 Up
        {
            get { return up; }
        }

        /// <summary>Get a {0, -1, 0} <see cref="Vector3"/>.</summary>
        public static Vector3 Down
        {
            get { return down; }
        }

        /// <summary>Get a {1, 0, 0} <see cref="Vector3"/>.</summary>
        public static Vector3 Right
        {
            get { return right; }
        }

        /// <summary>Get a {-1, 0, 0} <see cref="Vector3"/>.</summary>
        public static Vector3 Left
        {
            get { return left; }
        }

        /// <summary>Get a {0, 0, -1} <see cref="Vector3"/>.</summary>
        public static Vector3 Forward
        {
            get { return forward; }
        }

        /// <summary>Get a {0, 0, 1} <see cref="Vector3"/>.</summary>
        public static Vector3 Backward
        {
            get { return backward; }
        }

        #endregion Properties

        #region Constructors

        /// <summary>Initialize the components of the <see cref="Vector3"/>.</summary>
        /// <param name="value">The <see cref="Vector2"/> to use for the values of the <see cref="X"/> and <see cref="Y"/> components.</param>
        /// <param name="z">The value to use for the <see cref="Z"/> component.</param>
        public Vector3(Vector2 value, float z)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>Compute the <a href="http://en.wikipedia.org/wiki/Cross_product">cross product</a> of the input <see cref="Vector3"/> values, producing a vector that is perpendicular to both inputs.</summary>
        /// <remarks>The primary computer graphics use of the cross product is to produce normal vectors for a triangle. If the triangle is defined by <see cref="Vector3"/> values <c>v1</c>, <c>v2</c>, and <c>v3</c>, then <c>Normalize(Cross(v2 - v1, v3 - v1))</c> produces a normal vector. <c>Normalize(Cross(v3 - v1, v2 - v1))</c> produces a normal vector in the other direction, so winding is important.</remarks>
        /// <param name="vector1">The first <see cref="Vector3"/> value.</param>
        /// <param name="vector2">The second <see cref="Vector3"/> value.</param>
        /// <returns>A <see cref="Vector3"/> perpendicular to both <paramref name="vector1"/> and <paramref name="vector2"/>.</returns>
        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            Cross(ref vector1, ref vector2, out vector1);
            return vector1;
        }

        /// <summary>Compute the <a href="http://en.wikipedia.org/wiki/Cross_product">cross product</a> of the input <see cref="Vector3"/> values, producing a vector that is perpendicular to both inputs.</summary>
        /// <remarks>The primary computer graphics use of the cross product is to produce normal vectors for a triangle. If the triangle is defined by <see cref="Vector3"/> values <c>v1</c>, <c>v2</c>, and <c>v3</c>, then <c>Normalize(Cross(v2 - v1, v3 - v1))</c> produces a normal vector. <c>Normalize(Cross(v3 - v1, v2 - v1))</c> produces a normal vector in the other direction, so winding is important.</remarks>
        /// <param name="vector1">The first <see cref="Vector3"/> value. The contents will not be modified; pass by reference is only for optimization.</param>
        /// <param name="vector2">The second <see cref="Vector3"/> value. The contents will not be modified; pass by reference is only for optimization.</param>
        /// <param name="result">Receives a <see cref="Vector3"/> perpendicular to both <paramref name="vector1"/> and <paramref name="vector2"/>. The parameter may point to any member of the other parameters without affecting the result.</param>
        public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
        {
            result = new Vector3(
                vector1.Y * vector2.Z - vector2.Y * vector1.Z,
              -(vector1.X * vector2.Z - vector2.X * vector1.Z),
                vector1.X * vector2.Y - vector2.X * vector1.Y);
        }

        #endregion Public methods
    }
}
