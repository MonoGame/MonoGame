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
    public struct Matrix : IEquatable<Matrix>
    {
        #region Public Constructors
        
        public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31,
                      float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        #endregion Public Constructors


        #region Public Fields

        public float M11;
        public float M12;
        public float M13;
        public float M14;
        public float M21;
        public float M22;
        public float M23;
        public float M24;
        public float M31;
        public float M32;
        public float M33;
        public float M34;
        public float M41;
        public float M42;
        public float M43;
        public float M44;

        #endregion Public Fields


        #region Private Members
        private static Matrix identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        #endregion Private Members


        #region Public Properties
        
        public Vector3 Backward
        {
            get
            {
                return new Vector3(this.M31, this.M32, this.M33);
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }

        
        public Vector3 Down
        {
            get
            {
                return new Vector3(-this.M21, -this.M22, -this.M23);
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }

        
        public Vector3 Forward
        {
            get
            {
                return new Vector3(-this.M31, -this.M32, -this.M33);
            }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }
        }

        
        public static Matrix Identity
        {
            get { return identity; }
        }

        
        public Vector3 Left
        {
            get
            {
                return new Vector3(-this.M11, -this.M12, -this.M13);
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }

        
        public Vector3 Right
        {
            get
            {
                return new Vector3(this.M11, this.M12, this.M13);
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }

        
        public Vector3 Translation
        {
            get
            {
                return new Vector3(this.M41, this.M42, this.M43);
            }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }

        
        public Vector3 Up
        {
            get
            {
                return new Vector3(this.M21, this.M22, this.M23);
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }
        #endregion Public Properties


        #region Public Methods

        public static Matrix Add(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            throw new NotImplementedException();
        }

        
        public static Matrix CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 cameraUpVector, Nullable<Vector3> cameraForwardVector)
        {
            throw new NotImplementedException();
        }

        
        public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix result)
        {
            throw new NotImplementedException();
        }

        
        public static Matrix CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 rotateAxis, Nullable<Vector3> cameraForwardVector, Nullable<Vector3> objectForwardVector)
        {
            throw new NotImplementedException();
        }

        
        public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
        {
            throw new NotImplementedException();
        }


        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateFromQuaternion(Quaternion quaternion)
        {
            throw new NotImplementedException();
        }


        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            throw new NotImplementedException();
        }


        public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            throw new NotImplementedException();
        }


        public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            throw new NotImplementedException();
        }

        
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top,
            float zNearPlane, float zFarPlane, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreatePerspective(float width, float height, float zNearPlane, float zFarPlane)
        {
            throw new NotImplementedException();
        }


        public static void CreatePerspective(float width, float height, float zNearPlane, float zFarPlane, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float zNearPlane, float zFarPlane)
        {
            throw new NotImplementedException();
        }


        public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            throw new NotImplementedException();
        }


        public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateRotationX(float radians)
        {
            throw new NotImplementedException();
        }


        public static void CreateRotationX(float radians, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateRotationY(float radians)
        {
            throw new NotImplementedException();
        }


        public static void CreateRotationY(float radians, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateRotationZ(float radians)
        {
            Matrix matrix = new Matrix();
            matrix.M11 = (float)Math.Cos((double)radians);
            matrix.M12 = (float)Math.Sin((double)radians);
            matrix.M21 = -matrix.M12;
            matrix.M22 = matrix.M11;
            matrix.M33 = 1f;
            matrix.M44 = 1f;
            return matrix;
        }


        public static void CreateRotationZ(float radians, out Matrix result)
        {
            result = CreateRotationZ(radians);
        }


        public static Matrix CreateScale(float scale)
        {
            Matrix m = Matrix.Identity;
            m.M11 = m.M22 = m.M33 = scale;
            return m;
        }


        public static void CreateScale(float scale, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateScale(float xScale, float yScale, float zScale)
        {
            throw new NotImplementedException();
        }


        public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateScale(Vector3 scales)
        {
            throw new NotImplementedException();
        }


        public static void CreateScale(ref Vector3 scales, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateTranslation(float xPosition, float yPosition, float zPosition)
        {
            Matrix m = Matrix.Identity;
            m.M41 = xPosition;
            m.M42 = yPosition;
            m.M43 = zPosition;
            return m;
        }


        public static void CreateTranslation(ref Vector3 position, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix CreateTranslation(Vector3 position)
        {
            return CreateTranslation(position.X, position.Y, position.Z);
        }


        public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public float Determinant()
        {
            throw new NotImplementedException();
        }


        public static Matrix Divide(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix Divide(Matrix matrix1, float divider)
        {
            throw new NotImplementedException();
        }


        public static void Divide(ref Matrix matrix1, float divider, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public bool Equals(Matrix other)
        {
            throw new NotImplementedException();
        }


        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }


        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }


        public static Matrix Invert(Matrix m)
        {
            throw new NotImplementedException();
        }


        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix Lerp(Matrix matrix1, Matrix matrix2, float amount)
        {
            throw new NotImplementedException();
        }


        public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, float amount, out Matrix result)
        {
            throw new NotImplementedException();
        }

        public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix Multiply(Matrix matrix1, float factor)
        {
            throw new NotImplementedException();
        }


        public static void Multiply(ref Matrix matrix1, float factor, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix Negate(Matrix matrix)
        {
            throw new NotImplementedException();
        }


        public static void Negate(ref Matrix matrix, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static Matrix operator /(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static Matrix operator /(Matrix matrix1, float divider)
        {
            throw new NotImplementedException();
        }


        public static bool operator ==(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static bool operator !=(Matrix matrix1, Matrix matrix2)
        {
            return (
                matrix1.M11 != matrix2.M11 ||
                matrix1.M12 != matrix2.M12 ||
                matrix1.M13 != matrix2.M13 ||
                matrix1.M14 != matrix2.M14 ||
                matrix1.M21 != matrix2.M21 ||
                matrix1.M22 != matrix2.M22 ||
                matrix1.M23 != matrix2.M23 ||
                matrix1.M24 != matrix2.M24 ||
                matrix1.M31 != matrix2.M31 ||
                matrix1.M32 != matrix2.M32 ||
                matrix1.M33 != matrix2.M33 ||
                matrix1.M34 != matrix2.M34 || 
                matrix1.M41 != matrix2.M41 ||
                matrix1.M42 != matrix2.M42 ||
                matrix1.M43 != matrix2.M43 ||
                matrix1.M44 != matrix2.M44                  
                );
        }


        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            matrix.M11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            matrix.M12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            matrix.M13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            matrix.M14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            matrix.M21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            matrix.M22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            matrix.M23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            matrix.M24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            matrix.M31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            matrix.M32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            matrix.M33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            matrix.M34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            matrix.M41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            matrix.M42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            matrix.M43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            matrix.M44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            return matrix;
        }


        public static Matrix operator *(Matrix matrix, float scaleFactor)
        {
            throw new NotImplementedException();
        }


        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static Matrix operator -(Matrix matrix1)
        {
            throw new NotImplementedException();
        }


        public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
        {
            throw new NotImplementedException();
        }


        public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            throw new NotImplementedException();
        }


        public static Matrix Transpose(Matrix matrix)
        {
            throw new NotImplementedException();
        }

        
        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            throw new NotImplementedException();
        }
        #endregion Public Methods
    }
}
