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

namespace Microsoft.Xna.Framework
{
    public struct Vector3 : IEquatable<Vector3>
    {
        #region Private Static Fields

        private static Vector3 zero = new Vector3(0f, 0f, 0f);
        private static Vector3 one = new Vector3(1f, 1f, 1f);
        private static Vector3 left = new Vector3(-1f, 0f, 0f);
        private static Vector3 right = new Vector3(1f, 0f, 0f);
        private static Vector3 up = new Vector3(0f, 1f, 0f);
        private static Vector3 down = new Vector3(0f, -1f, 0f);
        private static Vector3 forward = new Vector3(0f, 0f, -1f);
        private static Vector3 backward = new Vector3(0f, 0f, 1f);
        private static Vector3 unitX = new Vector3(1f, 0f, 0f);
        private static Vector3 unitY = new Vector3(0f, 1f, 0f);
        private static Vector3 unitZ = new Vector3(0f, 0f, 1f);

        #endregion Private Fields

        #region Public Fields

        public float X;
        public float Y;
        public float Z;

        #endregion

        #region Public Static Properties

        public static Vector3 Zero
        {
            get
            {
                return zero;
            }
        }

        public static Vector3 One
        {
            get
            {
                return one;
            }
        }

        public static Vector3 Left
        {
            get
            {
                return left;
            }
        }

        public static Vector3 Right
        {
            get
            {
                return right;
            }
        }

        public static Vector3 Up
        {
            get
            {
                return up;
            }
        }

        public static Vector3 Down
        {
            get
            {
                return down;
            }
        }

        public static Vector3 Forward
        {
            get
            {
                return forward;
            }
        }

        public static Vector3 Backward
        {
            get
            {
                return backward;
            }
        }

        public static Vector3 UnitX
        {
            get
            {
                return unitX;
            }
        }

        public static Vector3 UnitY
        {
            get
            {
                return unitY;
            }
        }

        public static Vector3 UnitZ
        {
            get
            {
                return unitZ;
            }
        }

        #endregion

        #region Constructors

        public Vector3(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public Vector3(Vector2 value, float z)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
        }

        #endregion

        #region Public Static Methods

        public static Vector3 Add(Vector3 value1, Vector3 value2)
        {
            Vector3 result;
            Add(ref value1, ref value2, out result);
            return result;
        }

        public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
        }


        public static Vector3 Subtract(Vector3 value1, Vector3 value2)
        {
            Vector3 result;
            Subtract(ref value1, ref value2, out result);
            return result;
        }

        public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
        }


        public static Vector3 Multiply(Vector3 value1, float scaleFactor)
        {
            Vector3 result;
            Multiply(ref value1, scaleFactor, out result);
            return result;
        }

        public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
        }


        public static Vector3 Multiply(Vector3 value1, Vector3 value2)
        {
            Vector3 result;
            Multiply(ref value1, ref value2, out result);
            return result;
        }

        public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
        }


        public static Vector3 Divide(Vector3 value1, float value2)
        {
            Vector3 result;
            Divide(ref value1, value2, out result);
            return result;
        }

        public static void Divide(ref Vector3 value1, float value2, out Vector3 result)
        {
            float divFactor = 1f / value2;
            result.X = value1.X * divFactor;
            result.Y = value1.Y * divFactor;
            result.Z = value1.Z * divFactor;
        }


        public static Vector3 Divide(Vector3 value1, Vector3 value2)
        {
            Vector3 result;
            Divide(ref value1, ref value2, out result);
            return result;
        }

        public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
        }

        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            float result;
            Dot(ref vector1, ref vector2, out result);
            return result;
        }

        public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out float result)
        {
            result = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
        }


        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            Vector3 result;
            Cross(ref vector1, ref vector2, out result);
            return result;
        }

        public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
        {
            result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
        }


        public static Vector3 Normalize(Vector3 value)
        {
            Vector3 result;
            Normalize(ref value, out result);
            return result;
        }

        public static void Normalize(ref Vector3 value, out Vector3 result)
        {
            float divFactor = 1f / value.Length();
            result.X = value.X * divFactor;
            result.Y = value.Y * divFactor;
            result.Z = value.Z * divFactor;
        }


        public static float Distance(Vector3 value1, Vector3 value2)
        {
            float result;
            Distance(ref value1, ref value2, out result);
            return result;
        }

        public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            Vector3 resultVector;
            Subtract(ref value1, ref value2, out resultVector);
            result = resultVector.Length();
        }


        public static float DistanceSquared(Vector3 value1, Vector3 value2)
        {
            float result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

        public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
        {
            Vector3 resultVector;
            Subtract(ref value1, ref value2, out resultVector);
            result = resultVector.LengthSquared();
        }


        public static Vector3 Barycentric(Vector3 value1, Vector3 value2, Vector3 value3, float amount1, float amount2)
        {
            Vector3 result;
            Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out result);
            return result;
        }

        public static void Barycentric(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, float amount1, float amount2, out Vector3 result)
        {
            result.X = MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
            result.Y = MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
            result.Z = MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2);
        }


        public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
        {
            Vector3 result;
            CatmullRom(ref value1, ref value2, ref value3, ref value4, amount, out result);
            return result;
        }

        public static void CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4, float amount, out Vector3 result)
        {
            result.X = MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
            result.Y = MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
            result.Z = MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount);
        }


        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            Vector3 result;
            Clamp(ref value1, ref min, ref max, out result);
            return result;
        }

        public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result)
        {
            result.X = MathHelper.Clamp(value1.X, min.X, max.X);
            result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
            result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
        }


        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
        {
            Vector3 result;
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, amount, out result);
            return result;
        }

        public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
        {
            result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
            result.Z = MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
        }


        public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
        {
            Vector3 result;
            Lerp(ref value1, ref value2, amount, out result);
            return result;
        }

        public static void Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            result.X = MathHelper.Lerp(value1.X, value2.X, amount);
            result.Y = MathHelper.Lerp(value1.Y, value2.Y, amount);
            result.Z = MathHelper.Lerp(value1.Z, value2.Z, amount);
        }


        public static Vector3 Max(Vector3 value1, Vector3 value2)
        {
            Vector3 result;
            Max(ref value1, ref value2, out result);
            return result;
        }

        public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X > value2.X ? value1.X : value2.X;
            result.Y = value1.Y > value2.Y ? value1.Y : value2.Y;
            result.Z = value1.Z > value2.Z ? value1.Z : value2.Z;
        }


        public static Vector3 Min(Vector3 value1, Vector3 value2)
        {
            Vector3 result;
            Min(ref value1, ref value2, out result);
            return result;
        }

        public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
        {
            result.X = value1.X < value2.X ? value1.X : value2.X;
            result.Y = value1.Y < value2.Y ? value1.Y : value2.Y;
            result.Z = value1.Z < value2.Z ? value1.Z : value2.Z;
        }


        public static Vector3 Negate(Vector3 value)
        {
            Vector3 result;
            Negate(ref value, out result);
            return result;
        }

        public static void Negate(ref Vector3 value, out Vector3 result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
        }


        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            Vector3 result;
            Vector3.Reflect(ref vector, ref normal, out result);
            return result;
        }

        public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
        {
            float scaleFactor = Vector3.Dot(vector, normal) * 2;
            Vector3 tmp;
            Multiply(ref normal, scaleFactor, out tmp);
            Subtract(ref vector, ref tmp, out result);
        }


        public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
        {
            Vector3 result;
            Vector3.SmoothStep(ref value1, ref value2, amount, out result);
            return result;
        }

        public static void SmoothStep(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
        {
            result.X = MathHelper.SmoothStep(value1.X, value2.X, amount);
            result.Y = MathHelper.SmoothStep(value1.Y, value2.Y, amount);
            result.Z = MathHelper.SmoothStep(value1.Z, value2.Z, amount);
        }

        #region Transformations
        public static Vector3 Transform(Vector3 position, Matrix matrix)
        {
            Vector3 result;
            Transform(ref position, ref matrix, out result);
            return result;
        }

        //source: XNI
        public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            result.X = (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41;
            result.Y = (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42;
            result.Z = (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43;
        }

        public static Vector3 Transform(Vector3 value, Quaternion quaternion)
        {
            Vector3 result;
            Transform(ref value, ref quaternion, out result);
            return result;
        }

        public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector3 result)
        {
            float x = 2 * (rotation.Y * value.Z - rotation.Z * value.Y);
            float y = 2 * (rotation.Z * value.X - rotation.X * value.Z);
            float z = 2 * (rotation.X * value.Y - rotation.Y * value.X);

            result.X = value.X + x * rotation.W + (rotation.Y * z - rotation.Z * y);
            result.Y = value.Y + y * rotation.W + (rotation.Z * x - rotation.X * z);
            result.Z = value.Z + z * rotation.W + (rotation.X * y - rotation.Y * x);
        }

        public static void Transform(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
        {
            for (int i = 0; i < sourceArray.Length; i++)
                Transform(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }

        public static void Transform(Vector3[] sourceArray, ref Quaternion rotation, Vector3[] destinationArray)
        {
            for (int i = 0; i < sourceArray.Length; i++)
                Transform(ref sourceArray[i], ref rotation, out destinationArray[i]);
        }

        public static void Transform(Vector3[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3[] destinationArray, int destinationIndex, int length)
        {
            length += sourceIndex;
            for (int i = sourceIndex; i < length; i++, destinationIndex++)
                Transform(ref sourceArray[i], ref matrix, out destinationArray[destinationIndex]);
        }

        public static void Transform(Vector3[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector3[] destinationArray, int destinationIndex, int length)
        {
            length += sourceIndex;
            for (int i = sourceIndex; i < length; i++, destinationIndex++)
                Transform(ref sourceArray[i], ref rotation, out destinationArray[destinationIndex]);
        }

        public static Vector3 TransformNormal(Vector3 normal, Matrix matrix)
        {
            Vector3 result;
            TransformNormal(ref normal, ref matrix, out result);
            return result;
        }

        public static void TransformNormal(ref Vector3 normal, ref Matrix matrix, out Vector3 result)
        {
            result.X = (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31);
            result.Y = (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32);
            result.Z = (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33);
        }

        public static void TransformNormal(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
        {
            for (int i = 0; i < sourceArray.Length; i++)
                TransformNormal(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }

        public static void TransformNormal(Vector3[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3[] destinationArray, int destinationIndex, int length)
        {
            length += sourceIndex;
            for (int i = sourceIndex; i < length; i++, destinationIndex++)
                TransformNormal(ref sourceArray[i], ref matrix, out destinationArray[destinationIndex]);
        }
        #endregion

        #endregion

        #region Operator Overloading

        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            Vector3 result = new Vector3();
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            return result;
        }

        public static Vector3 operator /(Vector3 value1, float divider)
        {
            Vector3 result = new Vector3();
            float divFactor = 1f / divider;
            result.X = value1.X * divFactor;
            result.Y = value1.Y * divFactor;
            result.Z = value1.Z * divFactor;
            return result;
        }

        public static Vector3 operator /(Vector3 value1, Vector3 value2)
        {
            Vector3 result = new Vector3();
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            return result;
        }

        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            return !value1.Equals(value2);
        }

        public static Vector3 operator *(Vector3 value, float scaleFactor)
        {
            Vector3 result = new Vector3();
            result.X = value.X * scaleFactor;
            result.Y = value.Y * scaleFactor;
            result.Z = value.Z * scaleFactor;
            return result;
        }

        public static Vector3 operator *(float scaleFactor, Vector3 value)
        {
            Vector3 result = new Vector3();
            result.X = value.X * scaleFactor;
            result.Y = value.Y * scaleFactor;
            result.Z = value.Z * scaleFactor;
            return result;
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            Vector3 result = new Vector3();
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            return result;
        }

        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            Vector3 result = new Vector3();
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            return result;
        }

        public static Vector3 operator -(Vector3 value)
        {
            Vector3 result = new Vector3();
            result.X = value.X;
            result.Y = value.Y;
            result.Z = value.Z;
            return result;
        }

        #endregion

        #region Public Methods

        public float Length()
        {
            return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
        }

        public float LengthSquared()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        public void Normalize()
        {
            float divFactor = 1f / this.Length();
            this.X = this.X * divFactor;
            this.Y = this.Y * divFactor;
            this.Z = this.Z * divFactor;
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode();
        }

        public override string ToString()
        {
            var currentCulture = System.Globalization.CultureInfo.CurrentCulture;
            return string.Format(currentCulture, "{{X:{0} Y:{1} Z:{2}}}", new object[]
	        {
		        this.X.ToString(currentCulture), 
                this.Y.ToString(currentCulture),
                this.Z.ToString(currentCulture)
	        });
        }

        #endregion

        #region IEquatable implementation

        public override bool Equals(Object obj)
        {
            return (obj is Vector3) ? this.Equals((Vector3)obj) : false;
        }

        public bool Equals(Vector3 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        #endregion
    }
       
}
