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

namespace XnaTouch.Framework
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        static Quaternion identity = new Quaternion(0, 0, 0, 1);

        
        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
        
        
        public Quaternion(Vector3 vectorPart, float scalarPart)
        {
            this.X = vectorPart.X;
            this.Y = vectorPart.Y;
            this.Z = vectorPart.Z;
            this.W = scalarPart;
        }

        public static Quaternion Identity
        {
            get{ return identity; }
        }


        public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
        {
            throw new NotImplementedException();
        }


        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion CreateFromRotationMatrix(Matrix matrix)
        {
            throw new NotImplementedException();
        }


        public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
        {
            throw new NotImplementedException();
        }


        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }


        public bool Equals(Quaternion other)
        {
            throw new NotImplementedException();
        }


        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }


        public static Quaternion Inverse(Quaternion quaternion)
        {
            throw new NotImplementedException();
        }


        public static void Inverse(ref Quaternion quaternion, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public float Length()
        {
            throw new NotImplementedException();
        }


        public float LengthSquared()
        {
            throw new NotImplementedException();
        }


        public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            throw new NotImplementedException();
        }


        public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
        {
            throw new NotImplementedException();
        }


        public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor)
        {
            throw new NotImplementedException();
        }


        public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion Negate(Quaternion quaternion)
        {
            throw new NotImplementedException();
        }


        public static void Negate(ref Quaternion quaternion, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public void Normalize()
        {
            throw new NotImplementedException();
        }


        public static Quaternion Normalize(Quaternion quaternion)
        {
            throw new NotImplementedException();
        }


        public static void Normalize(ref Quaternion quaternion, out Quaternion result)
        {
            throw new NotImplementedException();
        }


        public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static Quaternion operator *(Quaternion quaternion1, float scaleFactor)
        {
            throw new NotImplementedException();
        }


        public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
        {
            throw new NotImplementedException();
        }


        public static Quaternion operator -(Quaternion quaternion)
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
