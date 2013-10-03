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
    [DataContract]
    public struct Vector4 : IEquatable<Vector4>
    {
        #region Private Fields

        private static Vector4 zeroVector = new Vector4();
        private static Vector4 unitVector = new Vector4(1f, 1f, 1f, 1f);
        private static Vector4 unitXVector = new Vector4(1f, 0f, 0f, 0f);
        private static Vector4 unitYVector = new Vector4(0f, 1f, 0f, 0f);
        private static Vector4 unitZVector = new Vector4(0f, 0f, 1f, 0f);
        private static Vector4 unitWVector = new Vector4(0f, 0f, 0f, 1f);

        #endregion Private Fields

        #region Public Fields
        
        [DataMember]
        public float X;

        [DataMember]
        public float Y;
      
        [DataMember]
        public float Z;
      
        [DataMember]
        public float W;

        #endregion Public Fields

        #region Properties

        public static Vector4 Zero
        {
            get { return zeroVector; }
        }

        public static Vector4 One
        {
            get { return unitVector; }
        }

        public static Vector4 UnitX
        {
            get { return unitXVector; }
        }

        public static Vector4 UnitY
        {
            get { return unitYVector; }
        }

        public static Vector4 UnitZ
        {
            get { return unitZVector; }
        }

        public static Vector4 UnitW
        {
            get { return unitWVector; }
        }

        #endregion Properties

        #region Constructors

        public Vector4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public Vector4(Vector2 value, float z, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = z;
            this.W = w;
        }

        public Vector4(Vector3 value, float w)
        {
            this.X = value.X;
            this.Y = value.Y;
            this.Z = value.Z;
            this.W = w;
        }

        public Vector4(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = value;
        }

        #endregion

        #region Public Methods

        public static Vector4 Add(Vector4 value1, Vector4 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            value1.W += value2.W;
            return value1;
        }

        public static void Add(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.X = value1.X + value2.X;
            result.Y = value1.Y + value2.Y;
            result.Z = value1.Z + value2.Z;
            result.W = value1.W + value2.W;
        }

        public static Vector4 Barycentric(Vector4 value1, Vector4 value2, Vector4 value3, float amount1, float amount2)
        {
            return new Vector4(
                MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
                MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2),
                MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2),
                MathHelper.Barycentric(value1.W, value2.W, value3.W, amount1, amount2));
        }

        public static void Barycentric(ref Vector4 value1, ref Vector4 value2, ref Vector4 value3, float amount1, float amount2, out Vector4 result)
        {
            result = new Vector4(
                MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
                MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2),
                MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2),
                MathHelper.Barycentric(value1.W, value2.W, value3.W, amount1, amount2));
        }

        public static Vector4 CatmullRom(Vector4 value1, Vector4 value2, Vector4 value3, Vector4 value4, float amount)
        {
            return new Vector4(
                MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
                MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount),
                MathHelper.CatmullRom(value1.W, value2.W, value3.W, value4.W, amount));
        }

        public static void CatmullRom(ref Vector4 value1, ref Vector4 value2, ref Vector4 value3, ref Vector4 value4, float amount, out Vector4 result)
        {
            result = new Vector4(
                MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
                MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount),
                MathHelper.CatmullRom(value1.W, value2.W, value3.W, value4.W, amount));
        }

        public static Vector4 Clamp(Vector4 value1, Vector4 min, Vector4 max)
        {
            return new Vector4(
                MathHelper.Clamp(value1.X, min.X, max.X),
                MathHelper.Clamp(value1.Y, min.Y, max.Y),
                MathHelper.Clamp(value1.Z, min.Z, max.Z),
                MathHelper.Clamp(value1.W, min.W, max.W));
        }

        public static void Clamp(ref Vector4 value1, ref Vector4 min, ref Vector4 max, out Vector4 result)
        {
            result = new Vector4(
                MathHelper.Clamp(value1.X, min.X, max.X),
                MathHelper.Clamp(value1.Y, min.Y, max.Y),
                MathHelper.Clamp(value1.Z, min.Z, max.Z),
                MathHelper.Clamp(value1.W, min.W, max.W));
        }

        public static float Distance(Vector4 value1, Vector4 value2)
        {
            return (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static void Distance(ref Vector4 value1, ref Vector4 value2, out float result)
        {
            result = (float)Math.Sqrt(DistanceSquared(value1, value2));
        }

        public static float DistanceSquared(Vector4 value1, Vector4 value2)
        {
            float result;
            DistanceSquared(ref value1, ref value2, out result);
            return result;
        }

        public static void DistanceSquared(ref Vector4 value1, ref Vector4 value2, out float result)
        {
            result = (value1.W - value2.W) * (value1.W - value2.W) +
                     (value1.X - value2.X) * (value1.X - value2.X) +
                     (value1.Y - value2.Y) * (value1.Y - value2.Y) +
                     (value1.Z - value2.Z) * (value1.Z - value2.Z);
        }

        public static Vector4 Divide(Vector4 value1, Vector4 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            value1.W /= value2.W;
            return value1;
        }

        public static Vector4 Divide(Vector4 value1, float divider)
        {
            float factor = 1f / divider;
            value1.X *= factor;
            value1.Y *= factor;
            value1.Z *= factor;
            value1.W *= factor;
            return value1;
        }

        public static void Divide(ref Vector4 value1, float divider, out Vector4 result)
        {
            float factor = 1f / divider;
            result.X = value1.X * factor;
            result.Y = value1.Y * factor;
            result.Z = value1.Z * factor;
            result.W = value1.W * factor;
        }

        public static void Divide(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.X = value1.X / value2.X;
            result.Y = value1.Y / value2.Y;
            result.Z = value1.Z / value2.Z;
            result.W = value1.W / value2.W;
        }

        public static float Dot(Vector4 vector1, Vector4 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W;
        }

        public static void Dot(ref Vector4 vector1, ref Vector4 vector2, out float result)
        {
            result = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z + vector1.W * vector2.W;
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector4) ? this == (Vector4)obj : false;
        }

        public bool Equals(Vector4 other)
        {
            return this.W == other.W
                && this.X == other.X
                && this.Y == other.Y
                && this.Z == other.Z;
        }

        public override int GetHashCode()
        {
            return (((int)(X * 10) & 0xFF) << 24) | (((int)(Y * 10) & 0xFF) << 16) | (((int)(Z * 10) & 0xFF) << 8) | ((int)(W * 10) & 0xFF);
        }

        public static Vector4 Hermite(Vector4 value1, Vector4 tangent1, Vector4 value2, Vector4 tangent2, float amount)
        {
            Vector4 result;
            result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
            result.Z = MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
            result.W = MathHelper.Hermite(value1.W, tangent1.W, value2.W, tangent2.W, amount);
            return result;
        }

        public static void Hermite(ref Vector4 value1, ref Vector4 tangent1, ref Vector4 value2, ref Vector4 tangent2, float amount, out Vector4 result)
        {
            result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
            result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
            result.Z = MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
            result.W = MathHelper.Hermite(value1.W, tangent1.W, value2.W, tangent2.W, amount);
        }

        public float Length()
        {
            return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W);
        }

        public float LengthSquared()
        {
            return (this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W);
        }

        public static Vector4 Lerp(Vector4 value1, Vector4 value2, float amount)
        {
            return new Vector4(
                MathHelper.Lerp(value1.X, value2.X, amount),
                MathHelper.Lerp(value1.Y, value2.Y, amount),
                MathHelper.Lerp(value1.Z, value2.Z, amount),
                MathHelper.Lerp(value1.W, value2.W, amount));
        }

        public static void Lerp(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result)
        {
            result = new Vector4(
                MathHelper.Lerp(value1.X, value2.X, amount),
                MathHelper.Lerp(value1.Y, value2.Y, amount),
                MathHelper.Lerp(value1.Z, value2.Z, amount),
                MathHelper.Lerp(value1.W, value2.W, amount));
        }

        public static Vector4 Max(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
               MathHelper.Max(value1.X, value2.X),
               MathHelper.Max(value1.Y, value2.Y),
               MathHelper.Max(value1.Z, value2.Z),
               MathHelper.Max(value1.W, value2.W));
        }

        public static void Max(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result = new Vector4(
               MathHelper.Max(value1.X, value2.X),
               MathHelper.Max(value1.Y, value2.Y),
               MathHelper.Max(value1.Z, value2.Z),
               MathHelper.Max(value1.W, value2.W));
        }

        public static Vector4 Min(Vector4 value1, Vector4 value2)
        {
            return new Vector4(
               MathHelper.Min(value1.X, value2.X),
               MathHelper.Min(value1.Y, value2.Y),
               MathHelper.Min(value1.Z, value2.Z),
               MathHelper.Min(value1.W, value2.W));
        }

        public static void Min(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result = new Vector4(
               MathHelper.Min(value1.X, value2.X),
               MathHelper.Min(value1.Y, value2.Y),
               MathHelper.Min(value1.Z, value2.Z),
               MathHelper.Min(value1.W, value2.W));
        }

        public static Vector4 Multiply(Vector4 value1, Vector4 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            value1.W *= value2.W;
            return value1;
        }

        public static Vector4 Multiply(Vector4 value1, float scaleFactor)
        {
            value1.W *= scaleFactor;
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            return value1;
        }

        public static void Multiply(ref Vector4 value1, float scaleFactor, out Vector4 result)
        {
            result.X = value1.X * scaleFactor;
            result.Y = value1.Y * scaleFactor;
            result.Z = value1.Z * scaleFactor;
            result.W = value1.W * scaleFactor;
        }

        public static void Multiply(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.X = value1.X * value2.X;
            result.Y = value1.Y * value2.Y;
            result.Z = value1.Z * value2.Z;
            result.W = value1.W * value2.W;
        }

        public static Vector4 Negate(Vector4 value)
        {
            value.X = -value.X;
            value.Y = -value.Y;
            value.Z = -value.Z;
            value.W = -value.W;
            return value;
        }

        public static void Negate(ref Vector4 value, out Vector4 result)
        {
            result = new Vector4(-value.X, -value.Y, -value.Z, -value.W);
        }

        public void Normalize()
        {
            Normalize(ref this, out this);
        }

        public static Vector4 Normalize(Vector4 vector)
        {
            Normalize(ref vector, out vector);
            return vector;
        }

        public static void Normalize(ref Vector4 vector, out Vector4 result)
        {
            float factor;
            DistanceSquared(ref vector, ref zeroVector, out factor);
            factor = 1f / (float)Math.Sqrt(factor);

            result.X = vector.X * factor;
            result.Y = vector.Y * factor;
            result.Z = vector.Z * factor;
            result.W = vector.W * factor;
        }

        public static Vector4 SmoothStep(Vector4 value1, Vector4 value2, float amount)
        {
            return new Vector4(
                MathHelper.SmoothStep(value1.X, value2.X, amount),
                MathHelper.SmoothStep(value1.Y, value2.Y, amount),
                MathHelper.SmoothStep(value1.Z, value2.Z, amount),
                MathHelper.SmoothStep(value1.W, value2.W, amount));
        }

        public static void SmoothStep(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result)
        {
            result = new Vector4(
                MathHelper.SmoothStep(value1.X, value2.X, amount),
                MathHelper.SmoothStep(value1.Y, value2.Y, amount),
                MathHelper.SmoothStep(value1.Z, value2.Z, amount),
                MathHelper.SmoothStep(value1.W, value2.W, amount));
        }

        public static Vector4 Subtract(Vector4 value1, Vector4 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            value1.W -= value2.W;
            return value1;
        }

        public static void Subtract(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
        {
            result.X = value1.X - value2.X;
            result.Y = value1.Y - value2.Y;
            result.Z = value1.Z - value2.Z;
            result.W = value1.W - value2.W;
        }
        
        public override string ToString()
        {
            return "{X:" + this.X + " Y:" + this.Y + " Z:" + this.Z + " W:" + this.W + "}";
        }

        public static void Transform(ref Vector4 vector, ref Matrix matrix, out Vector4 result)
        {
            result = new Vector4((vector.X * matrix.M11) + (vector.Y * matrix.M21) + (vector.Z * matrix.M31) + (vector.W * matrix.M41),
                                 (vector.X * matrix.M12) + (vector.Y * matrix.M22) + (vector.Z * matrix.M32) + (vector.W * matrix.M42),
                                 (vector.X * matrix.M13) + (vector.Y * matrix.M23) + (vector.Z * matrix.M33) + (vector.W * matrix.M43),
                                 (vector.X * matrix.M14) + (vector.Y * matrix.M24) + (vector.Z * matrix.M34) + (vector.W * matrix.M44));
        }

        public static void Transform(Vector4[] sourceArray,ref Quaternion rotation,Vector4[] destinationArray)
        {
            Transform(sourceArray, 0, ref rotation, destinationArray, 0, sourceArray.Length);
        }

        public static Vector4 Transform(Vector2 value,Quaternion rotation)
        {
            Vector4 result;
            Transform(ref value, ref rotation, out result);
            return result;
        }

        public static void Transform(ref Vector2 value,ref Quaternion rotation,out Vector4 result)
        {
            float num1 = rotation.X + rotation.X;
            float num2 = rotation.Y + rotation.Y;
            float num3 = rotation.Z + rotation.Z;
            float num4 = rotation.W * num1;
            float num5 = rotation.W * num2;
            float num6 = rotation.W * num3;
            float num7 = rotation.X * num1;
            float num8 = rotation.X * num2;
            float num9 = rotation.X * num3;
            float num10 = rotation.Y * num2;
            float num11 = rotation.Y * num3;
            float num12 = rotation.Z * num3;
            float num13 = (float)((double)value.X * (1.0 - (double)num10 - (double)num12) + (double)value.Y * ((double)num8 - (double)num6));
            float num14 = (float)((double)value.X * ((double)num8 + (double)num6) + (double)value.Y * (1.0 - (double)num7 - (double)num12));
            float num15 = (float)((double)value.X * ((double)num9 - (double)num5) + (double)value.Y * ((double)num11 + (double)num4));
            result = new Vector4(num13,num14,num15,1.0f);
        }

        public static Vector4 Transform(Vector3 value, Quaternion rotation)
        {
            Vector4 result;
            Transform(ref value, ref rotation, out result);
            return result;
        }

        public static Vector4 Transform(Vector4 value, Quaternion rotation)
        {
            Vector4 result;
            Transform(ref value, ref rotation, out result);
            return result;
        }

        public static void Transform(ref Vector3 value,ref Quaternion rotation,out Vector4 result)
        {
            float num1 = rotation.X + rotation.X;
            float num2 = rotation.Y + rotation.Y;
            float num3 = rotation.Z + rotation.Z;
            float num4 = rotation.W * num1;
            float num5 = rotation.W * num2;
            float num6 = rotation.W * num3;
            float num7 = rotation.X * num1;
            float num8 = rotation.X * num2;
            float num9 = rotation.X * num3;
            float num10 = rotation.Y * num2;
            float num11 = rotation.Y * num3;
            float num12 = rotation.Z * num3;
            float num13 = (float)((double)value.X * (1.0 - (double)num10 - (double)num12) + (double)value.Y * ((double)num8 - (double)num6) + (double)value.Z * ((double)num9 + (double)num5));
            float num14 = (float)((double)value.X * ((double)num8 + (double)num6) + (double)value.Y * (1.0 - (double)num7 - (double)num12) + (double)value.Z * ((double)num11 - (double)num4));
            float num15 = (float)((double)value.X * ((double)num9 - (double)num5) + (double)value.Y * ((double)num11 + (double)num4) + (double)value.Z * (1.0 - (double)num7 - (double)num10));
            result = new Vector4(num13,num14,num15,1f);
        }

        public static void Transform(ref Vector4 value,ref Quaternion rotation,out Vector4 result)
        {
            float num1 = rotation.X + rotation.X;
            float num2 = rotation.Y + rotation.Y;
            float num3 = rotation.Z + rotation.Z;
            float num4 = rotation.W * num1;
            float num5 = rotation.W * num2;
            float num6 = rotation.W * num3;
            float num7 = rotation.X * num1;
            float num8 = rotation.X * num2;
            float num9 = rotation.X * num3;
            float num10 = rotation.Y * num2;
            float num11 = rotation.Y * num3;
            float num12 = rotation.Z * num3;
            float num13 = (float)((double)value.X * (1.0 - (double)num10 - (double)num12) + (double)value.Y * ((double)num8 - (double)num6) + (double)value.Z * ((double)num9 + (double)num5));
            float num14 = (float)((double)value.X * ((double)num8 + (double)num6) + (double)value.Y * (1.0 - (double)num7 - (double)num12) + (double)value.Z * ((double)num11 - (double)num4));
            float num15 = (float)((double)value.X * ((double)num9 - (double)num5) + (double)value.Y * ((double)num11 + (double)num4) + (double)value.Z * (1.0 - (double)num7 - (double)num10));
            result = new Vector4(num13,num14,num15,value.W);
        }
 

        public static void Transform(Vector4[] sourceArray, int sourceIndex, ref Matrix matrix,Vector4[] destinationArray,int destinationIndex,int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if ((long)sourceArray.Length < (long)sourceIndex + (long)length)
                throw new ArgumentException("sourceArray.Length < sourceIndex+length");
            if ((long)destinationArray.Length < (long)destinationIndex + (long)length)
                throw new ArgumentException("destinationArray.Length < destinationIndex + length");
            for (; length > 0; --length)
            {
                float num1 = sourceArray[sourceIndex].X;
                float num2 = sourceArray[sourceIndex].Y;
                float num3 = sourceArray[sourceIndex].Z;
                float num4 = sourceArray[sourceIndex].W;
                destinationArray[destinationIndex].X = (float)((double)num1 * (double)matrix.M11 + (double)num2 * (double)matrix.M21 + (double)num3 * (double)matrix.M31 + (double)num4 * (double)matrix.M41);
                destinationArray[destinationIndex].Y = (float)((double)num1 * (double)matrix.M12 + (double)num2 * (double)matrix.M22 + (double)num3 * (double)matrix.M32 + (double)num4 * (double)matrix.M42);
                destinationArray[destinationIndex].Z = (float)((double)num1 * (double)matrix.M13 + (double)num2 * (double)matrix.M23 + (double)num3 * (double)matrix.M33 + (double)num4 * (double)matrix.M43);
                destinationArray[destinationIndex].W = (float)((double)num1 * (double)matrix.M14 + (double)num2 * (double)matrix.M24 + (double)num3 * (double)matrix.M34 + (double)num4 * (double)matrix.M44);
                ++sourceIndex;
                ++destinationIndex;
            }
        }

        public static void Transform(Vector4[] sourceArray, ref Matrix matrix, Vector4[] destinationArray)
        {
            Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
        }

        public static Vector4 Transform(Vector4 vector, Matrix matrix)
        {
            Transform(ref vector, ref matrix, out vector);
            return vector;
        }

        public static Vector4 Transform(Vector2 position, Matrix matrix)
        {
            Vector4 result;
            Transform(ref position, ref matrix, out result);
            return result;
        }

       
        public static void Transform(Vector4[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector4[] destinationArray, int destinationIndex, int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException("sourceArray");
            if (destinationArray == null)
                throw new ArgumentNullException("destinationArray");
            if ((long)sourceArray.Length < (long)sourceIndex + (long)length)
                throw new ArgumentException("sourceArray.Length < sourceIndex + length");
            if ((long)destinationArray.Length < (long)destinationIndex + (long)length)
                throw new ArgumentException("destinationArray.Length < destinationIndex + length");
            float num1 = rotation.X + rotation.X;
            float num2 = rotation.Y + rotation.Y;
            float num3 = rotation.Z + rotation.Z;
            float num4 = rotation.W * num1;
            float num5 = rotation.W * num2;
            float num6 = rotation.W * num3;
            float num7 = rotation.X * num1;
            float num8 = rotation.X * num2;
            float num9 = rotation.X * num3;
            float num10 = rotation.Y * num2;
            float num11 = rotation.Y * num3;
            float num12 = rotation.Z * num3;
            float num13 = 1f - num10 - num12;
            float num14 = num8 - num6;
            float num15 = num9 + num5;
            float num16 = num8 + num6;
            float num17 = 1f - num7 - num12;
            float num18 = num11 - num4;
            float num19 = num9 - num5;
            float num20 = num11 + num4;
            float num21 = 1f - num7 - num10;
            for (; length > 0; --length)
            {
                float num22 = sourceArray[sourceIndex].X;
                float num23 = sourceArray[sourceIndex].Y;
                float num24 = sourceArray[sourceIndex].Z;
                float num25 = sourceArray[sourceIndex].W;
                destinationArray[destinationIndex].X = (float)((double)num22 * (double)num13 + (double)num23 * (double)num14 + (double)num24 * (double)num15);
                destinationArray[destinationIndex].Y = (float)((double)num22 * (double)num16 + (double)num23 * (double)num17 + (double)num24 * (double)num18);
                destinationArray[destinationIndex].Z = (float)((double)num22 * (double)num19 + (double)num23 * (double)num20 + (double)num24 * (double)num21);
                destinationArray[destinationIndex].W = num25;
                ++sourceIndex;
                ++destinationIndex;
            }
        }

        public static void Transform(ref Vector2 position, ref Matrix matrix, out Vector4 result)
        {
            result = new Vector4((position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
                                 (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42,
                                 (position.X * matrix.M13) + (position.Y * matrix.M23) + matrix.M43,
                                 (position.X * matrix.M14) + (position.Y * matrix.M24) + matrix.M44);
        }

        public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector4 result)
        {
            result = new Vector4((position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
                                 (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
                                 (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43,
                                 (position.X * matrix.M14) + (position.Y * matrix.M24) + (position.Z * matrix.M34) + matrix.M44);
        }

        public static Vector4 Transform(Vector3 position, Matrix matrix)
        {
            Vector4 result;
            Transform(ref position, ref matrix, out result);
            return result;
        }

        #endregion Public Methods

        #region Operators

        public static Vector4 operator -(Vector4 value)
        {
            return new Vector4(-value.X, -value.Y, -value.Z, -value.W);
        }

        public static bool operator ==(Vector4 value1, Vector4 value2)
        {
            return 
                value1.X == value2.X
                && value1.Y == value2.Y
                && value1.Z == value2.Z
                && value1.W == value2.W;
        }

        public static bool operator !=(Vector4 value1, Vector4 value2)
        {
            return
                 value1.X != value2.X
                 || value1.Y != value2.Y
                 || value1.Z != value2.Z
                 || value1.W != value2.W;
        }

        public static Vector4 operator +(Vector4 value1, Vector4 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            value1.W += value2.W;
            return value1;
        }

        public static Vector4 operator -(Vector4 value1, Vector4 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            value1.W -= value2.W;
            return value1;
        }

        public static Vector4 operator *(Vector4 value1, Vector4 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            value1.W *= value2.W;
            return value1;
        }

        public static Vector4 operator *(Vector4 value1, float scaleFactor)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            value1.W *= scaleFactor;
            return value1;
        }

        public static Vector4 operator *(float scaleFactor, Vector4 value1)
        {
            value1.X *= scaleFactor;
            value1.Y *= scaleFactor;
            value1.Z *= scaleFactor;
            value1.W *= scaleFactor;
            return value1;
        }

        public static Vector4 operator /(Vector4 value1, Vector4 value2)
        {
            value1.X /= value2.X;
            value1.Y /= value2.Y;
            value1.Z /= value2.Z;
            value1.W /= value2.W;
            return value1;
        }

        public static Vector4 operator /(Vector4 value1, float divider)
        {
            float factor = 1f / divider;
            value1.X *= factor;
            value1.Y *= factor;
            value1.Z *= factor;
            value1.W *= factor;
            return value1;
        }

        #endregion Operators
    }
}
