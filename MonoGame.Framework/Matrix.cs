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
        private static Matrix identity = new Matrix(1f, 0f, 0f, 0f, 
		                                            0f, 1f, 0f, 0f, 
		                                            0f, 0f, 1f, 0f, 
		                                            0f, 0f, 0f, 1f);
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

		// made this static so we dont create a new
		// float array each time we use the matrix 
		private static float[] openglMatrix = { 1f, 0f, 0f, 0f, 
	                                            0f, 1f, 0f, 0f, 
	                                            0f, 0f, 1f, 0f, 
	                                            0f, 0f, 0f, 1f};
		
				
		
		// required for OpenGL 2.0 projection matrix stuff
		public static float[] ToFloatArray(Matrix mat)
        {
			openglMatrix[0]  = mat.M11; openglMatrix[1]  = mat.M12; openglMatrix[2]  = mat.M13; openglMatrix[3]  = mat.M14;
			openglMatrix[4]  = mat.M21; openglMatrix[5]  = mat.M22; openglMatrix[6]  = mat.M23; openglMatrix[7]  = mat.M24;
			openglMatrix[8]  = mat.M31; openglMatrix[9]  = mat.M32; openglMatrix[10] = mat.M33; openglMatrix[11] = mat.M34;
			openglMatrix[12] = mat.M41; openglMatrix[13] = mat.M42; openglMatrix[14] = mat.M43; openglMatrix[15] = mat.M44;
			return openglMatrix;
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
            matrix1.M11 += matrix2.M11;
            matrix1.M12 += matrix2.M12;
            matrix1.M13 += matrix2.M13;
            matrix1.M14 += matrix2.M14;
            matrix1.M21 += matrix2.M21;
            matrix1.M22 += matrix2.M22;
            matrix1.M23 += matrix2.M23;
            matrix1.M24 += matrix2.M24;
            matrix1.M31 += matrix2.M31;
            matrix1.M32 += matrix2.M32;
            matrix1.M33 += matrix2.M33;
            matrix1.M34 += matrix2.M34;
            matrix1.M41 += matrix2.M41;
            matrix1.M42 += matrix2.M42;
            matrix1.M43 += matrix2.M43;
            matrix1.M44 += matrix2.M44;
            return matrix1;
        }


        public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;

        }

        
        public static Matrix CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 cameraUpVector, Nullable<Vector3> cameraForwardVector)
        {
            Matrix matrix;
		    Vector3 vector;
		    Vector3 vector2;
		    Vector3 vector3;
		    vector.X = objectPosition.X - cameraPosition.X;
		    vector.Y = objectPosition.Y - cameraPosition.Y;
		    vector.Z = objectPosition.Z - cameraPosition.Z;
		    float num = vector.LengthSquared();
		    if (num < 0.0001f)
		    {
		        vector = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
		    }
		    else
		    {
		        Vector3.Multiply(ref vector, (float) (1f / ((float) Math.Sqrt((double) num))), out vector);
		    }
		    Vector3.Cross(ref cameraUpVector, ref vector, out vector3);
		    vector3.Normalize();
		    Vector3.Cross(ref vector, ref vector3, out vector2);
		    matrix.M11 = vector3.X;
		    matrix.M12 = vector3.Y;
		    matrix.M13 = vector3.Z;
		    matrix.M14 = 0f;
		    matrix.M21 = vector2.X;
		    matrix.M22 = vector2.Y;
		    matrix.M23 = vector2.Z;
		    matrix.M24 = 0f;
		    matrix.M31 = vector.X;
		    matrix.M32 = vector.Y;
		    matrix.M33 = vector.Z;
		    matrix.M34 = 0f;
		    matrix.M41 = objectPosition.X;
		    matrix.M42 = objectPosition.Y;
		    matrix.M43 = objectPosition.Z;
		    matrix.M44 = 1f;
		    return matrix;
        }

        
        public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix result)
        {
            Vector3 vector;
		    Vector3 vector2;
		    Vector3 vector3;
		    vector.X = objectPosition.X - cameraPosition.X;
		    vector.Y = objectPosition.Y - cameraPosition.Y;
		    vector.Z = objectPosition.Z - cameraPosition.Z;
		    float num = vector.LengthSquared();
		    if (num < 0.0001f)
		    {
		        vector = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
		    }
		    else
		    {
		        Vector3.Multiply(ref vector, (float) (1f / ((float) Math.Sqrt((double) num))), out vector);
		    }
		    Vector3.Cross(ref cameraUpVector, ref vector, out vector3);
		    vector3.Normalize();
		    Vector3.Cross(ref vector, ref vector3, out vector2);
		    result.M11 = vector3.X;
		    result.M12 = vector3.Y;
		    result.M13 = vector3.Z;
		    result.M14 = 0f;
		    result.M21 = vector2.X;
		    result.M22 = vector2.Y;
		    result.M23 = vector2.Z;
		    result.M24 = 0f;
		    result.M31 = vector.X;
		    result.M32 = vector.Y;
		    result.M33 = vector.Z;
		    result.M34 = 0f;
		    result.M41 = objectPosition.X;
		    result.M42 = objectPosition.Y;
		    result.M43 = objectPosition.Z;
		    result.M44 = 1f;
        }

        
        public static Matrix CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition,
            Vector3 rotateAxis, Nullable<Vector3> cameraForwardVector, Nullable<Vector3> objectForwardVector)
        {
            float num;
		    Vector3 vector;
		    Matrix matrix;
		    Vector3 vector2;
		    Vector3 vector3;
		    vector2.X = objectPosition.X - cameraPosition.X;
		    vector2.Y = objectPosition.Y - cameraPosition.Y;
		    vector2.Z = objectPosition.Z - cameraPosition.Z;
		    float num2 = vector2.LengthSquared();
		    if (num2 < 0.0001f)
		    {
		        vector2 = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
		    }
		    else
		    {
		        Vector3.Multiply(ref vector2, (float) (1f / ((float) Math.Sqrt((double) num2))), out vector2);
		    }
		    Vector3 vector4 = rotateAxis;
		    Vector3.Dot(ref rotateAxis, ref vector2, out num);
		    if (Math.Abs(num) > 0.9982547f)
		    {
		        if (objectForwardVector.HasValue)
		        {
		            vector = objectForwardVector.Value;
		            Vector3.Dot(ref rotateAxis, ref vector, out num);
		            if (Math.Abs(num) > 0.9982547f)
		            {
		                num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
		                vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
		            }
		        }
		        else
		        {
		            num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
		            vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
		        }
		        Vector3.Cross(ref rotateAxis, ref vector, out vector3);
		        vector3.Normalize();
		        Vector3.Cross(ref vector3, ref rotateAxis, out vector);
		        vector.Normalize();
		    }
		    else
		    {
		        Vector3.Cross(ref rotateAxis, ref vector2, out vector3);
		        vector3.Normalize();
		        Vector3.Cross(ref vector3, ref vector4, out vector);
		        vector.Normalize();
		    }
		    matrix.M11 = vector3.X;
		    matrix.M12 = vector3.Y;
		    matrix.M13 = vector3.Z;
		    matrix.M14 = 0f;
		    matrix.M21 = vector4.X;
		    matrix.M22 = vector4.Y;
		    matrix.M23 = vector4.Z;
		    matrix.M24 = 0f;
		    matrix.M31 = vector.X;
		    matrix.M32 = vector.Y;
		    matrix.M33 = vector.Z;
		    matrix.M34 = 0f;
		    matrix.M41 = objectPosition.X;
		    matrix.M42 = objectPosition.Y;
		    matrix.M43 = objectPosition.Z;
		    matrix.M44 = 1f;
		    return matrix;

        }

        
        public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition,
            ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix result)
        {
            float num;
		    Vector3 vector;
		    Vector3 vector2;
		    Vector3 vector3;
		    vector2.X = objectPosition.X - cameraPosition.X;
		    vector2.Y = objectPosition.Y - cameraPosition.Y;
		    vector2.Z = objectPosition.Z - cameraPosition.Z;
		    float num2 = vector2.LengthSquared();
		    if (num2 < 0.0001f)
		    {
		        vector2 = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
		    }
		    else
		    {
		        Vector3.Multiply(ref vector2, (float) (1f / ((float) Math.Sqrt((double) num2))), out vector2);
		    }
		    Vector3 vector4 = rotateAxis;
		    Vector3.Dot(ref rotateAxis, ref vector2, out num);
		    if (Math.Abs(num) > 0.9982547f)
		    {
		        if (objectForwardVector.HasValue)
		        {
		            vector = objectForwardVector.Value;
		            Vector3.Dot(ref rotateAxis, ref vector, out num);
		            if (Math.Abs(num) > 0.9982547f)
		            {
		                num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
		                vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
		            }
		        }
		        else
		        {
		            num = ((rotateAxis.X * Vector3.Forward.X) + (rotateAxis.Y * Vector3.Forward.Y)) + (rotateAxis.Z * Vector3.Forward.Z);
		            vector = (Math.Abs(num) > 0.9982547f) ? Vector3.Right : Vector3.Forward;
		        }
		        Vector3.Cross(ref rotateAxis, ref vector, out vector3);
		        vector3.Normalize();
		        Vector3.Cross(ref vector3, ref rotateAxis, out vector);
		        vector.Normalize();
		    }
		    else
		    {
		        Vector3.Cross(ref rotateAxis, ref vector2, out vector3);
		        vector3.Normalize();
		        Vector3.Cross(ref vector3, ref vector4, out vector);
		        vector.Normalize();
		    }
		    result.M11 = vector3.X;
		    result.M12 = vector3.Y;
		    result.M13 = vector3.Z;
		    result.M14 = 0f;
		    result.M21 = vector4.X;
		    result.M22 = vector4.Y;
		    result.M23 = vector4.Z;
		    result.M24 = 0f;
		    result.M31 = vector.X;
		    result.M32 = vector.Y;
		    result.M33 = vector.Z;
		    result.M34 = 0f;
		    result.M41 = objectPosition.X;
		    result.M42 = objectPosition.Y;
		    result.M43 = objectPosition.Z;
		    result.M44 = 1f;

        }


        public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
        {
            Matrix matrix;
		    float x = axis.X;
		    float y = axis.Y;
		    float z = axis.Z;
		    float num2 = (float) Math.Sin((double) angle);
		    float num = (float) Math.Cos((double) angle);
		    float num11 = x * x;
		    float num10 = y * y;
		    float num9 = z * z;
		    float num8 = x * y;
		    float num7 = x * z;
		    float num6 = y * z;
		    matrix.M11 = num11 + (num * (1f - num11));
		    matrix.M12 = (num8 - (num * num8)) + (num2 * z);
		    matrix.M13 = (num7 - (num * num7)) - (num2 * y);
		    matrix.M14 = 0f;
		    matrix.M21 = (num8 - (num * num8)) - (num2 * z);
		    matrix.M22 = num10 + (num * (1f - num10));
		    matrix.M23 = (num6 - (num * num6)) + (num2 * x);
		    matrix.M24 = 0f;
		    matrix.M31 = (num7 - (num * num7)) + (num2 * y);
		    matrix.M32 = (num6 - (num * num6)) - (num2 * x);
		    matrix.M33 = num9 + (num * (1f - num9));
		    matrix.M34 = 0f;
		    matrix.M41 = 0f;
		    matrix.M42 = 0f;
		    matrix.M43 = 0f;
		    matrix.M44 = 1f;
		    return matrix;

        }


        public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
        {
            float x = axis.X;
		    float y = axis.Y;
		    float z = axis.Z;
		    float num2 = (float) Math.Sin((double) angle);
		    float num = (float) Math.Cos((double) angle);
		    float num11 = x * x;
		    float num10 = y * y;
		    float num9 = z * z;
		    float num8 = x * y;
		    float num7 = x * z;
		    float num6 = y * z;
		    result.M11 = num11 + (num * (1f - num11));
		    result.M12 = (num8 - (num * num8)) + (num2 * z);
		    result.M13 = (num7 - (num * num7)) - (num2 * y);
		    result.M14 = 0f;
		    result.M21 = (num8 - (num * num8)) - (num2 * z);
		    result.M22 = num10 + (num * (1f - num10));
		    result.M23 = (num6 - (num * num6)) + (num2 * x);
		    result.M24 = 0f;
		    result.M31 = (num7 - (num * num7)) + (num2 * y);
		    result.M32 = (num6 - (num * num6)) - (num2 * x);
		    result.M33 = num9 + (num * (1f - num9));
		    result.M34 = 0f;
		    result.M41 = 0f;
		    result.M42 = 0f;
		    result.M43 = 0f;
		    result.M44 = 1f;
        }


        public static Matrix CreateFromQuaternion(Quaternion quaternion)
        {
            Matrix matrix;
		    float num9 = quaternion.X * quaternion.X;
		    float num8 = quaternion.Y * quaternion.Y;
		    float num7 = quaternion.Z * quaternion.Z;
		    float num6 = quaternion.X * quaternion.Y;
		    float num5 = quaternion.Z * quaternion.W;
		    float num4 = quaternion.Z * quaternion.X;
		    float num3 = quaternion.Y * quaternion.W;
		    float num2 = quaternion.Y * quaternion.Z;
		    float num = quaternion.X * quaternion.W;
		    matrix.M11 = 1f - (2f * (num8 + num7));
		    matrix.M12 = 2f * (num6 + num5);
		    matrix.M13 = 2f * (num4 - num3);
		    matrix.M14 = 0f;
		    matrix.M21 = 2f * (num6 - num5);
		    matrix.M22 = 1f - (2f * (num7 + num9));
		    matrix.M23 = 2f * (num2 + num);
		    matrix.M24 = 0f;
		    matrix.M31 = 2f * (num4 + num3);
		    matrix.M32 = 2f * (num2 - num);
		    matrix.M33 = 1f - (2f * (num8 + num9));
		    matrix.M34 = 0f;
		    matrix.M41 = 0f;
		    matrix.M42 = 0f;
		    matrix.M43 = 0f;
		    matrix.M44 = 1f;
		    return matrix;
        }


        public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
        {
            float num9 = quaternion.X * quaternion.X;
		    float num8 = quaternion.Y * quaternion.Y;
		    float num7 = quaternion.Z * quaternion.Z;
		    float num6 = quaternion.X * quaternion.Y;
		    float num5 = quaternion.Z * quaternion.W;
		    float num4 = quaternion.Z * quaternion.X;
		    float num3 = quaternion.Y * quaternion.W;
		    float num2 = quaternion.Y * quaternion.Z;
		    float num = quaternion.X * quaternion.W;
		    result.M11 = 1f - (2f * (num8 + num7));
		    result.M12 = 2f * (num6 + num5);
		    result.M13 = 2f * (num4 - num3);
		    result.M14 = 0f;
		    result.M21 = 2f * (num6 - num5);
		    result.M22 = 1f - (2f * (num7 + num9));
		    result.M23 = 2f * (num2 + num);
		    result.M24 = 0f;
		    result.M31 = 2f * (num4 + num3);
		    result.M32 = 2f * (num2 - num);
		    result.M33 = 1f - (2f * (num8 + num9));
		    result.M34 = 0f;
		    result.M41 = 0f;
		    result.M42 = 0f;
		    result.M43 = 0f;
		    result.M44 = 1f;
        }
		
		public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
		{
			Matrix matrix;
		    Quaternion quaternion;
		    Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
		    CreateFromQuaternion(ref quaternion, out matrix);
		    return matrix;
		}
		
		public static void CreateFromYawPitchRoll(
         float yaw,
         float pitch,
         float roll,
         out Matrix result)
		{
			Quaternion quaternion;
		    Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
		    CreateFromQuaternion(ref quaternion, out result);
		}

        public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
			
            Matrix m = identity;
		    float[] x = new float[3]; 
			float[] y = new float[3]; 
			float[] z = new float[3]; 
		    float mag;
		    
		    // Make rotation matrix
		    
		    // Z vector
		    z[0] = cameraPosition.X - cameraTarget.X;
		    z[1] = cameraPosition.Y - cameraTarget.Y;
		    z[2] = cameraPosition.Z - cameraTarget.Z;
		    mag = (float)System.Math.Sqrt((z[0] * z[0] + z[1] * z[1] + z[2] * z[2]));
		    if (mag > 0) {          // mpichler, 19950515
		        z[0] /= mag;
		        z[1] /= mag;
		        z[2] /= mag;
		    }
		    
		    // Y vector
		    y[0] = cameraUpVector.X;
		    y[1] = cameraUpVector.Y;
		    y[2] = cameraUpVector.Z;
		    
		    // X vector = Y cross Z
		    x[0] = y[1] * z[2] - y[2] * z[1];
		    x[1] = -y[0] * z[2] + y[2] * z[0];
		    x[2] = y[0] * z[1] - y[1] * z[0];
		    
		    // Recompute Y = Z cross X
		    y[0] = z[1] * x[2] - z[2] * x[1];
		    y[1] = -z[0] * x[2] + z[2] * x[0];
		    y[2] = z[0] * x[1] - z[1] * x[0];
		    
		    // mpichler, 19950515
		    // cross product gives area of parallelogram, which is < 1.0 for
		    // non-perpendicular unit-length vectors; so normalize x, y here
		    //
		    
		    mag = (float)System.Math.Sqrt(x[0] * x[0] + x[1] * x[1] + x[2] * x[2]);
		    if (mag>0) {
		        x[0] /= mag;
		        x[1] /= mag;
		        x[2] /= mag;
		    }
		    
		    mag = (float)System.Math.Sqrt(y[0] * y[0] + y[1] * y[1] + y[2] * y[2]);
		    if (mag>0) {
		        y[0] /= mag;
		        y[1] /= mag;
		        y[2] /= mag;
		    }
		    
		
		    m.M11 = x[0];
		    m.M12 = x[1];
		    m.M13 = x[2];
		    m.M14 = 0.0f;
		    m.M21 = y[0];
		    m.M22 = y[1];
		    m.M23 = y[2];
		    m.M24 = 0.0f;
		    m.M31 = z[0];
		    m.M32 = z[1];
		    m.M33 = z[2];
		    m.M34 = 0.0f;
		    m.M41 = 0.0f;
		    m.M42 = 0.0f;
		    m.M43 = 0.0f;
		    m.M44 = 1.0f;
			
			return m;
        }


        public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
        {
            Vector3 vector = Vector3.Normalize(cameraPosition - cameraTarget);
		    Vector3 vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
		    Vector3 vector3 = Vector3.Cross(vector, vector2);
		    result.M11 = vector2.X;
		    result.M12 = vector3.X;
		    result.M13 = vector.X;
		    result.M14 = 0f;
		    result.M21 = vector2.Y;
		    result.M22 = vector3.Y;
		    result.M23 = vector.Y;
		    result.M24 = 0f;
		    result.M31 = vector2.Z;
		    result.M32 = vector3.Z;
		    result.M33 = vector.Z;
		    result.M34 = 0f;
		    result.M41 = -Vector3.Dot(vector2, cameraPosition);
		    result.M42 = -Vector3.Dot(vector3, cameraPosition);
		    result.M43 = -Vector3.Dot(vector, cameraPosition);
		    result.M44 = 1f;
        }


        public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            Matrix matrix;
		    matrix.M11 = 2f / width;
		    matrix.M12 = matrix.M13 = matrix.M14 = 0f;
		    matrix.M22 = 2f / height;
		    matrix.M21 = matrix.M23 = matrix.M24 = 0f;
		    matrix.M33 = 1f / (zNearPlane - zFarPlane);
		    matrix.M31 = matrix.M32 = matrix.M34 = 0f;
		    matrix.M41 = matrix.M42 = 0f;
		    matrix.M43 = zNearPlane / (zNearPlane - zFarPlane);
		    matrix.M44 = 1f;
		    return matrix;
        }


        public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Matrix result)
        {
            result.M11 = 2f / width;
		    result.M12 = result.M13 = result.M14 = 0f;
		    result.M22 = 2f / height;
		    result.M21 = result.M23 = result.M24 = 0f;
		    result.M33 = 1f / (zNearPlane - zFarPlane);
		    result.M31 = result.M32 = result.M34 = 0f;
		    result.M41 = result.M42 = 0f;
		    result.M43 = zNearPlane / (zNearPlane - zFarPlane);
		    result.M44 = 1f;
        }


        public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            float tx = - (right + left)/(right - left);
			float ty = - (top + bottom)/(top - bottom);
			float tz = - (zFarPlane + zNearPlane)/(zFarPlane - zNearPlane);
			
			Matrix m = identity;
			
			m.M11 = 2.0f/(right-left);
			m.M12 = 0;
			m.M13 = 0;
			m.M14 = tx;
			
			m.M21 = 0;
			m.M22 = 2.0f/(top-bottom);
			m.M23 = 0;
			m.M24 = ty;
			
			m.M31 = 0;
			m.M32 = 0;
			m.M33 = -2.0f/(zFarPlane - zNearPlane);
			m.M34 = tz;
			
			m.M41 = 0;
			m.M42 = 0;
			m.M43 = 0;
			m.M44 = 1;
			
			return m;
        }

        
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top,
            float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            result = CreateOrthographicOffCenter( left, right, bottom, top, nearPlaneDistance, farPlaneDistance);
        }


        public static Matrix CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix matrix;
		    if (nearPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("nearPlaneDistance <= 0");
		    }
		    if (farPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("farPlaneDistance <= 0");
		    }
		    if (nearPlaneDistance >= farPlaneDistance)
		    {
		        throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		    }
		    matrix.M11 = (2f * nearPlaneDistance) / width;
		    matrix.M12 = matrix.M13 = matrix.M14 = 0f;
		    matrix.M22 = (2f * nearPlaneDistance) / height;
		    matrix.M21 = matrix.M23 = matrix.M24 = 0f;
		    matrix.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		    matrix.M31 = matrix.M32 = 0f;
		    matrix.M34 = -1f;
		    matrix.M41 = matrix.M42 = matrix.M44 = 0f;
		    matrix.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
		    return matrix;
        }


        public static void CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if (nearPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("nearPlaneDistance <= 0");
		    }
		    if (farPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("farPlaneDistance <= 0");
		    }
		    if (nearPlaneDistance >= farPlaneDistance)
		    {
		        throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		    }
		    result.M11 = (2f * nearPlaneDistance) / width;
		    result.M12 = result.M13 = result.M14 = 0f;
		    result.M22 = (2f * nearPlaneDistance) / height;
		    result.M21 = result.M23 = result.M24 = 0f;
		    result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		    result.M31 = result.M32 = 0f;
		    result.M34 = -1f;
		    result.M41 = result.M42 = result.M44 = 0f;
		    result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
        }


        public static Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix matrix;
		    if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f))
		    {
		        throw new ArgumentException("fieldOfView <= 0 O >= PI");
		    }
		    if (nearPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("nearPlaneDistance <= 0");
		    }
		    if (farPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("farPlaneDistance <= 0");
		    }
		    if (nearPlaneDistance >= farPlaneDistance)
		    {
		        throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		    }
		    float num = 1f / ((float) Math.Tan((double) (fieldOfView * 0.5f)));
		    float num9 = num / aspectRatio;
		    matrix.M11 = num9;
		    matrix.M12 = matrix.M13 = matrix.M14 = 0f;
		    matrix.M22 = num;
		    matrix.M21 = matrix.M23 = matrix.M24 = 0f;
		    matrix.M31 = matrix.M32 = 0f;
		    matrix.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		    matrix.M34 = -1f;
		    matrix.M41 = matrix.M42 = matrix.M44 = 0f;
		    matrix.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
		    return matrix;
        }


        public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if ((fieldOfView <= 0f) || (fieldOfView >= 3.141593f))
		    {
		        throw new ArgumentException("fieldOfView <= 0 or >= PI");
		    }
		    if (nearPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("nearPlaneDistance <= 0");
		    }
		    if (farPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("farPlaneDistance <= 0");
		    }
		    if (nearPlaneDistance >= farPlaneDistance)
		    {
		        throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		    }
		    float num = 1f / ((float) Math.Tan((double) (fieldOfView * 0.5f)));
		    float num9 = num / aspectRatio;
		    result.M11 = num9;
		    result.M12 = result.M13 = result.M14 = 0f;
		    result.M22 = num;
		    result.M21 = result.M23 = result.M24 = 0f;
		    result.M31 = result.M32 = 0f;
		    result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		    result.M34 = -1f;
		    result.M41 = result.M42 = result.M44 = 0f;
		    result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
        }


        public static Matrix CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
        {
            Matrix matrix;
		    if (nearPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("nearPlaneDistance <= 0");
		    }
		    if (farPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("farPlaneDistance <= 0");
		    }
		    if (nearPlaneDistance >= farPlaneDistance)
		    {
		        throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		    }
		    matrix.M11 = (2f * nearPlaneDistance) / (right - left);
		    matrix.M12 = matrix.M13 = matrix.M14 = 0f;
		    matrix.M22 = (2f * nearPlaneDistance) / (top - bottom);
		    matrix.M21 = matrix.M23 = matrix.M24 = 0f;
		    matrix.M31 = (left + right) / (right - left);
		    matrix.M32 = (top + bottom) / (top - bottom);
		    matrix.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		    matrix.M34 = -1f;
		    matrix.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
		    matrix.M41 = matrix.M42 = matrix.M44 = 0f;
		    return matrix;
        }


        public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if (nearPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("nearPlaneDistance <= 0");
		    }
		    if (farPlaneDistance <= 0f)
		    {
		        throw new ArgumentException("farPlaneDistance <= 0");
		    }
		    if (nearPlaneDistance >= farPlaneDistance)
		    {
		        throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		    }
		    result.M11 = (2f * nearPlaneDistance) / (right - left);
		    result.M12 = result.M13 = result.M14 = 0f;
		    result.M22 = (2f * nearPlaneDistance) / (top - bottom);
		    result.M21 = result.M23 = result.M24 = 0f;
		    result.M31 = (left + right) / (right - left);
		    result.M32 = (top + bottom) / (top - bottom);
		    result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		    result.M34 = -1f;
		    result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
		    result.M41 = result.M42 = result.M44 = 0f;
        }


        public static Matrix CreateRotationX(float radians)
        {
            Matrix returnMatrix = Matrix.Identity;

            returnMatrix.M22 = (float)Math.Cos(radians);
            returnMatrix.M23 = (float)Math.Sin(radians);
            returnMatrix.M32 = -returnMatrix.M23;
            returnMatrix.M33 = returnMatrix.M22;

            return returnMatrix;

        }


        public static void CreateRotationX(float radians, out Matrix result)
        {
            result = Matrix.Identity;

            result.M22 = (float)Math.Cos(radians);
            result.M23 = (float)Math.Sin(radians);
            result.M32 = -result.M23;
            result.M33 = result.M22;

        }


        public static Matrix CreateRotationY(float radians)
        {
            Matrix returnMatrix = Matrix.Identity;

            returnMatrix.M11 = (float)Math.Cos(radians);
            returnMatrix.M13 = (float)Math.Sin(radians);
            returnMatrix.M31 = -returnMatrix.M13;
            returnMatrix.M33 = returnMatrix.M11;

            return returnMatrix;
        }


        public static void CreateRotationY(float radians, out Matrix result)
        {
            result = Matrix.Identity;

            result.M11 = (float)Math.Cos(radians);
            result.M13 = (float)Math.Sin(radians);
            result.M31 = -result.M13;
            result.M33 = result.M11;
        }


        public static Matrix CreateRotationZ(float radians)
        {
            Matrix returnMatrix = Matrix.Identity;

            returnMatrix.M11 = (float)Math.Cos(radians);
            returnMatrix.M12 = (float)Math.Sin(radians);
            returnMatrix.M21 = -returnMatrix.M12;
            returnMatrix.M22 = returnMatrix.M11;

            return returnMatrix;
        }


        public static void CreateRotationZ(float radians, out Matrix result)
        {
            result = Matrix.Identity;

            result.M11 = (float)Math.Cos(radians);
            result.M12 = (float)Math.Sin(radians);
            result.M21 = -result.M12;
            result.M22 = result.M11;
        }


        public static Matrix CreateScale(float scale)
        {
            Matrix m = Matrix.Identity;
            m.M11 = m.M22 = m.M33 = scale;
            return m;
        }


        public static void CreateScale(float scale, out Matrix result)
        {
            result = CreateScale(scale);
        }


        public static Matrix CreateScale(float xScale, float yScale, float zScale)
        {
            Matrix returnMatrix;
			returnMatrix.M11 = xScale;
			returnMatrix.M12 = 0;
			returnMatrix.M13 = 0;
			returnMatrix.M14 = 0;
			returnMatrix.M21 = 0;
			returnMatrix.M22 = yScale;
			returnMatrix.M23 = 0;
			returnMatrix.M24 = 0;
			returnMatrix.M31 = 0;
			returnMatrix.M32 = 0;
			returnMatrix.M33 = zScale;
			returnMatrix.M34 = 0;
			returnMatrix.M41 = 0;
			returnMatrix.M42 = 0;
			returnMatrix.M43 = 0;
			returnMatrix.M44 = 1;
			return returnMatrix;
        }


        public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result)
        {
            result = CreateScale(xScale, yScale, zScale);
        }


        public static Matrix CreateScale(Vector3 scales)
        {
            return CreateScale(scales.X, scales.Y, scales.Z);
        }


        public static void CreateScale(ref Vector3 scales, out Matrix result)
        {
            result = CreateScale(scales.X, scales.Y, scales.Z);
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
            result = CreateTranslation(position.X, position.Y, position.Z);
        }


        public static Matrix CreateTranslation(Vector3 position)
        {
            return CreateTranslation(position.X, position.Y, position.Z);
        }


        public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
        {
            result = CreateTranslation(xPosition, yPosition, zPosition);
        }


        public float Determinant()
        {
            float num22 = this.M11;
		    float num21 = this.M12;
		    float num20 = this.M13;
		    float num19 = this.M14;
		    float num12 = this.M21;
		    float num11 = this.M22;
		    float num10 = this.M23;
		    float num9 = this.M24;
		    float num8 = this.M31;
		    float num7 = this.M32;
		    float num6 = this.M33;
		    float num5 = this.M34;
		    float num4 = this.M41;
		    float num3 = this.M42;
		    float num2 = this.M43;
		    float num = this.M44;
		    float num18 = (num6 * num) - (num5 * num2);
		    float num17 = (num7 * num) - (num5 * num3);
		    float num16 = (num7 * num2) - (num6 * num3);
		    float num15 = (num8 * num) - (num5 * num4);
		    float num14 = (num8 * num2) - (num6 * num4);
		    float num13 = (num8 * num3) - (num7 * num4);
		    return ((((num22 * (((num11 * num18) - (num10 * num17)) + (num9 * num16))) - (num21 * (((num12 * num18) - (num10 * num15)) + (num9 * num14)))) + (num20 * (((num12 * num17) - (num11 * num15)) + (num9 * num13)))) - (num19 * (((num12 * num16) - (num11 * num14)) + (num10 * num13))));
        }


        public static Matrix Divide(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
		    matrix.M11 = matrix1.M11 / matrix2.M11;
		    matrix.M12 = matrix1.M12 / matrix2.M12;
		    matrix.M13 = matrix1.M13 / matrix2.M13;
		    matrix.M14 = matrix1.M14 / matrix2.M14;
		    matrix.M21 = matrix1.M21 / matrix2.M21;
		    matrix.M22 = matrix1.M22 / matrix2.M22;
		    matrix.M23 = matrix1.M23 / matrix2.M23;
		    matrix.M24 = matrix1.M24 / matrix2.M24;
		    matrix.M31 = matrix1.M31 / matrix2.M31;
		    matrix.M32 = matrix1.M32 / matrix2.M32;
		    matrix.M33 = matrix1.M33 / matrix2.M33;
		    matrix.M34 = matrix1.M34 / matrix2.M34;
		    matrix.M41 = matrix1.M41 / matrix2.M41;
		    matrix.M42 = matrix1.M42 / matrix2.M42;
		    matrix.M43 = matrix1.M43 / matrix2.M43;
		    matrix.M44 = matrix1.M44 / matrix2.M44;
		    return matrix;
        }


        public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 / matrix2.M11;
		    result.M12 = matrix1.M12 / matrix2.M12;
		    result.M13 = matrix1.M13 / matrix2.M13;
		    result.M14 = matrix1.M14 / matrix2.M14;
		    result.M21 = matrix1.M21 / matrix2.M21;
		    result.M22 = matrix1.M22 / matrix2.M22;
		    result.M23 = matrix1.M23 / matrix2.M23;
		    result.M24 = matrix1.M24 / matrix2.M24;
		    result.M31 = matrix1.M31 / matrix2.M31;
		    result.M32 = matrix1.M32 / matrix2.M32;
		    result.M33 = matrix1.M33 / matrix2.M33;
		    result.M34 = matrix1.M34 / matrix2.M34;
		    result.M41 = matrix1.M41 / matrix2.M41;
		    result.M42 = matrix1.M42 / matrix2.M42;
		    result.M43 = matrix1.M43 / matrix2.M43;
		    result.M44 = matrix1.M44 / matrix2.M44;
        }


        public static Matrix Divide(Matrix matrix1, float divider)
        {
            Matrix matrix;
		    float num = 1f / divider;
		    matrix.M11 = matrix1.M11 * num;
		    matrix.M12 = matrix1.M12 * num;
		    matrix.M13 = matrix1.M13 * num;
		    matrix.M14 = matrix1.M14 * num;
		    matrix.M21 = matrix1.M21 * num;
		    matrix.M22 = matrix1.M22 * num;
		    matrix.M23 = matrix1.M23 * num;
		    matrix.M24 = matrix1.M24 * num;
		    matrix.M31 = matrix1.M31 * num;
		    matrix.M32 = matrix1.M32 * num;
		    matrix.M33 = matrix1.M33 * num;
		    matrix.M34 = matrix1.M34 * num;
		    matrix.M41 = matrix1.M41 * num;
		    matrix.M42 = matrix1.M42 * num;
		    matrix.M43 = matrix1.M43 * num;
		    matrix.M44 = matrix1.M44 * num;
		    return matrix;
        }


        public static void Divide(ref Matrix matrix1, float divider, out Matrix result)
        {
            float num = 1f / divider;
		    result.M11 = matrix1.M11 * num;
		    result.M12 = matrix1.M12 * num;
		    result.M13 = matrix1.M13 * num;
		    result.M14 = matrix1.M14 * num;
		    result.M21 = matrix1.M21 * num;
		    result.M22 = matrix1.M22 * num;
		    result.M23 = matrix1.M23 * num;
		    result.M24 = matrix1.M24 * num;
		    result.M31 = matrix1.M31 * num;
		    result.M32 = matrix1.M32 * num;
		    result.M33 = matrix1.M33 * num;
		    result.M34 = matrix1.M34 * num;
		    result.M41 = matrix1.M41 * num;
		    result.M42 = matrix1.M42 * num;
		    result.M43 = matrix1.M43 * num;
		    result.M44 = matrix1.M44 * num;
        }


        public bool Equals(Matrix other)
        {
            return ((((((this.M11 == other.M11) && (this.M22 == other.M22)) && ((this.M33 == other.M33) && (this.M44 == other.M44))) && (((this.M12 == other.M12) && (this.M13 == other.M13)) && ((this.M14 == other.M14) && (this.M21 == other.M21)))) && ((((this.M23 == other.M23) && (this.M24 == other.M24)) && ((this.M31 == other.M31) && (this.M32 == other.M32))) && (((this.M34 == other.M34) && (this.M41 == other.M41)) && (this.M42 == other.M42)))) && (this.M43 == other.M43));
        }


        public override bool Equals(object obj)
        {
            bool flag = false;
		    if (obj is Matrix)
		    {
		        flag = this.Equals((Matrix) obj);
		    }
		    return flag;
        }


        public override int GetHashCode()
        {
            return (((((((((((((((this.M11.GetHashCode() + this.M12.GetHashCode()) + this.M13.GetHashCode()) + this.M14.GetHashCode()) + this.M21.GetHashCode()) + this.M22.GetHashCode()) + this.M23.GetHashCode()) + this.M24.GetHashCode()) + this.M31.GetHashCode()) + this.M32.GetHashCode()) + this.M33.GetHashCode()) + this.M34.GetHashCode()) + this.M41.GetHashCode()) + this.M42.GetHashCode()) + this.M43.GetHashCode()) + this.M44.GetHashCode());
        }


        public static Matrix Invert(Matrix matrix)
        {
            Invert(ref matrix, out matrix);
            return matrix;

        }


        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            ///
            // Use Laplace expansion theorem to calculate the inverse of a 4x4 matrix
            // 
            // 1. Calculate the 2x2 determinants needed the 4x4 determinant based on the 2x2 determinants 
            // 3. Create the adjugate matrix, which satisfies: A * adj(A) = det(A) * I
            // 4. Divide adjugate matrix with the determinant to find the inverse
            
            float det1, det2, det3, det4, det5, det6, det7, det8, det9, det10, det11, det12;
            float detMatrix;
            findDeterminants(ref matrix, out detMatrix, out det1, out det2, out det3, out det4, out det5, out det6, 
                             out det7, out det8, out det9, out det10, out det11, out det12);
            
            float invDetMatrix = 1f / detMatrix;
            
            Matrix ret; // Allow for matrix and result to point to the same structure
            
            ret.M11 = (matrix.M22*det12 - matrix.M23*det11 + matrix.M24*det10) * invDetMatrix;
            ret.M12 = (-matrix.M12*det12 + matrix.M13*det11 - matrix.M14*det10) * invDetMatrix;
            ret.M13 = (matrix.M42*det6 - matrix.M43*det5 + matrix.M44*det4) * invDetMatrix;
            ret.M14 = (-matrix.M32*det6 + matrix.M33*det5 - matrix.M34*det4) * invDetMatrix;
            ret.M21 = (-matrix.M21*det12 + matrix.M23*det9 - matrix.M24*det8) * invDetMatrix;
            ret.M22 = (matrix.M11*det12 - matrix.M13*det9 + matrix.M14*det8) * invDetMatrix;
            ret.M23 = (-matrix.M41*det6 + matrix.M43*det3 - matrix.M44*det2) * invDetMatrix;
            ret.M24 = (matrix.M31*det6 - matrix.M33*det3 + matrix.M34*det2) * invDetMatrix;
            ret.M31 = (matrix.M21*det11 - matrix.M22*det9 + matrix.M24*det7) * invDetMatrix;
            ret.M32 = (-matrix.M11*det11 + matrix.M12*det9 - matrix.M14*det7) * invDetMatrix;
            ret.M33 = (matrix.M41*det5 - matrix.M42*det3 + matrix.M44*det1) * invDetMatrix;
            ret.M34 = (-matrix.M31*det5 + matrix.M32*det3 - matrix.M34*det1) * invDetMatrix;
            ret.M41 = (-matrix.M21*det10 + matrix.M22*det8 - matrix.M23*det7) * invDetMatrix;
            ret.M42 = (matrix.M11*det10 - matrix.M12*det8 + matrix.M13*det7) * invDetMatrix;
            ret.M43 = (-matrix.M41*det4 + matrix.M42*det2 - matrix.M43*det1) * invDetMatrix;
            ret.M44 = (matrix.M31*det4 - matrix.M32*det2 + matrix.M33*det1) * invDetMatrix;
            
            result = ret;
        }


        public static Matrix Lerp(Matrix matrix1, Matrix matrix2, float amount)
        {
            Matrix matrix;
		    matrix.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
		    matrix.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
		    matrix.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
		    matrix.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);
		    matrix.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
		    matrix.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
		    matrix.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
		    matrix.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);
		    matrix.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
		    matrix.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
		    matrix.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
		    matrix.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);
		    matrix.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
		    matrix.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
		    matrix.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
		    matrix.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);
		    return matrix;
        }


        public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, float amount, out Matrix result)
        {
            result.M11 = matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount);
		    result.M12 = matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount);
		    result.M13 = matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount);
		    result.M14 = matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount);
		    result.M21 = matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount);
		    result.M22 = matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount);
		    result.M23 = matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount);
		    result.M24 = matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount);
		    result.M31 = matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount);
		    result.M32 = matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount);
		    result.M33 = matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount);
		    result.M34 = matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount);
		    result.M41 = matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount);
		    result.M42 = matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount);
		    result.M43 = matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount);
		    result.M44 = matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount);
        }

        public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
        {
            Matrix ret;
            Multiply(ref matrix1, ref matrix2, out ret);
            return ret;
        }


        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
                        result.M12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
                        result.M13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
                        result.M14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
                        
            result.M21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
                        result.M22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
                        result.M23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
                        result.M24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
            
                        result.M31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
                        result.M32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
                        result.M33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
                        result.M34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
            
                        result.M41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
                        result.M42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
                        result.M43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
            result.M44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44; 
        }


        public static Matrix Multiply(Matrix matrix1, float factor)
        {
            matrix1.M11 *= factor;
            matrix1.M12 *= factor;
            matrix1.M13 *= factor;
            matrix1.M14 *= factor;
            matrix1.M21 *= factor;
            matrix1.M22 *= factor;
            matrix1.M23 *= factor;
            matrix1.M24 *= factor;
            matrix1.M31 *= factor;
            matrix1.M32 *= factor;
            matrix1.M33 *= factor;
            matrix1.M34 *= factor;
            matrix1.M41 *= factor;
            matrix1.M42 *= factor;
            matrix1.M43 *= factor;
            matrix1.M44 *= factor;
            return matrix1;
        }


        public static void Multiply(ref Matrix matrix1, float factor, out Matrix result)
        {
            result.M11 = matrix1.M11 * factor;
            result.M12 = matrix1.M12 * factor;
            result.M13 = matrix1.M13 * factor;
            result.M14 = matrix1.M14 * factor;
            result.M21 = matrix1.M21 * factor;
            result.M22 = matrix1.M22 * factor;
            result.M23 = matrix1.M23 * factor;
            result.M24 = matrix1.M24 * factor;
            result.M31 = matrix1.M31 * factor;
            result.M32 = matrix1.M32 * factor;
            result.M33 = matrix1.M33 * factor;
            result.M34 = matrix1.M34 * factor;
            result.M41 = matrix1.M41 * factor;
            result.M42 = matrix1.M42 * factor;
            result.M43 = matrix1.M43 * factor;
            result.M44 = matrix1.M44 * factor;

        }


        public static Matrix Negate(Matrix matrix)
        {
            Matrix matrix2;
		    matrix2.M11 = -matrix.M11;
		    matrix2.M12 = -matrix.M12;
		    matrix2.M13 = -matrix.M13;
		    matrix2.M14 = -matrix.M14;
		    matrix2.M21 = -matrix.M21;
		    matrix2.M22 = -matrix.M22;
		    matrix2.M23 = -matrix.M23;
		    matrix2.M24 = -matrix.M24;
		    matrix2.M31 = -matrix.M31;
		    matrix2.M32 = -matrix.M32;
		    matrix2.M33 = -matrix.M33;
		    matrix2.M34 = -matrix.M34;
		    matrix2.M41 = -matrix.M41;
		    matrix2.M42 = -matrix.M42;
		    matrix2.M43 = -matrix.M43;
		    matrix2.M44 = -matrix.M44;
		    return matrix2;
        }


        public static void Negate(ref Matrix matrix, out Matrix result)
        {
            result.M11 = -matrix.M11;
		    result.M12 = -matrix.M12;
		    result.M13 = -matrix.M13;
		    result.M14 = -matrix.M14;
		    result.M21 = -matrix.M21;
		    result.M22 = -matrix.M22;
		    result.M23 = -matrix.M23;
		    result.M24 = -matrix.M24;
		    result.M31 = -matrix.M31;
		    result.M32 = -matrix.M32;
		    result.M33 = -matrix.M33;
		    result.M34 = -matrix.M34;
		    result.M41 = -matrix.M41;
		    result.M42 = -matrix.M42;
		    result.M43 = -matrix.M43;
		    result.M44 = -matrix.M44;
        }


        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            Matrix.Add(ref matrix1, ref matrix2, out matrix1);
            return matrix1;
        }


        public static Matrix operator /(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
		    matrix.M11 = matrix1.M11 / matrix2.M11;
		    matrix.M12 = matrix1.M12 / matrix2.M12;
		    matrix.M13 = matrix1.M13 / matrix2.M13;
		    matrix.M14 = matrix1.M14 / matrix2.M14;
		    matrix.M21 = matrix1.M21 / matrix2.M21;
		    matrix.M22 = matrix1.M22 / matrix2.M22;
		    matrix.M23 = matrix1.M23 / matrix2.M23;
		    matrix.M24 = matrix1.M24 / matrix2.M24;
		    matrix.M31 = matrix1.M31 / matrix2.M31;
		    matrix.M32 = matrix1.M32 / matrix2.M32;
		    matrix.M33 = matrix1.M33 / matrix2.M33;
		    matrix.M34 = matrix1.M34 / matrix2.M34;
		    matrix.M41 = matrix1.M41 / matrix2.M41;
		    matrix.M42 = matrix1.M42 / matrix2.M42;
		    matrix.M43 = matrix1.M43 / matrix2.M43;
		    matrix.M44 = matrix1.M44 / matrix2.M44;
		    return matrix;
        }


        public static Matrix operator /(Matrix matrix1, float divider)
        {
            Matrix matrix;
		    float num = 1f / divider;
		    matrix.M11 = matrix1.M11 * num;
		    matrix.M12 = matrix1.M12 * num;
		    matrix.M13 = matrix1.M13 * num;
		    matrix.M14 = matrix1.M14 * num;
		    matrix.M21 = matrix1.M21 * num;
		    matrix.M22 = matrix1.M22 * num;
		    matrix.M23 = matrix1.M23 * num;
		    matrix.M24 = matrix1.M24 * num;
		    matrix.M31 = matrix1.M31 * num;
		    matrix.M32 = matrix1.M32 * num;
		    matrix.M33 = matrix1.M33 * num;
		    matrix.M34 = matrix1.M34 * num;
		    matrix.M41 = matrix1.M41 * num;
		    matrix.M42 = matrix1.M42 * num;
		    matrix.M43 = matrix1.M43 * num;
		    matrix.M44 = matrix1.M44 * num;
		    return matrix;
        }


        public static bool operator ==(Matrix matrix1, Matrix matrix2)
        {
            return (
                matrix1.M11 == matrix2.M11 &&
                matrix1.M12 == matrix2.M12 &&
                matrix1.M13 == matrix2.M13 &&
                matrix1.M14 == matrix2.M14 &&
                matrix1.M21 == matrix2.M21 &&
                matrix1.M22 == matrix2.M22 &&
                matrix1.M23 == matrix2.M23 &&
                matrix1.M24 == matrix2.M24 &&
                matrix1.M31 == matrix2.M31 &&
                matrix1.M32 == matrix2.M32 &&
                matrix1.M33 == matrix2.M33 &&
                matrix1.M34 == matrix2.M34 &&
                matrix1.M41 == matrix2.M41 &&
                matrix1.M42 == matrix2.M42 &&
                matrix1.M43 == matrix2.M43 &&
                matrix1.M44 == matrix2.M44                  
                );
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
            Matrix matrix2;
		    float num = scaleFactor;
		    matrix2.M11 = matrix.M11 * num;
		    matrix2.M12 = matrix.M12 * num;
		    matrix2.M13 = matrix.M13 * num;
		    matrix2.M14 = matrix.M14 * num;
		    matrix2.M21 = matrix.M21 * num;
		    matrix2.M22 = matrix.M22 * num;
		    matrix2.M23 = matrix.M23 * num;
		    matrix2.M24 = matrix.M24 * num;
		    matrix2.M31 = matrix.M31 * num;
		    matrix2.M32 = matrix.M32 * num;
		    matrix2.M33 = matrix.M33 * num;
		    matrix2.M34 = matrix.M34 * num;
		    matrix2.M41 = matrix.M41 * num;
		    matrix2.M42 = matrix.M42 * num;
		    matrix2.M43 = matrix.M43 * num;
		    matrix2.M44 = matrix.M44 * num;
		    return matrix2;
        }


        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
		    matrix.M11 = matrix1.M11 - matrix2.M11;
		    matrix.M12 = matrix1.M12 - matrix2.M12;
		    matrix.M13 = matrix1.M13 - matrix2.M13;
		    matrix.M14 = matrix1.M14 - matrix2.M14;
		    matrix.M21 = matrix1.M21 - matrix2.M21;
		    matrix.M22 = matrix1.M22 - matrix2.M22;
		    matrix.M23 = matrix1.M23 - matrix2.M23;
		    matrix.M24 = matrix1.M24 - matrix2.M24;
		    matrix.M31 = matrix1.M31 - matrix2.M31;
		    matrix.M32 = matrix1.M32 - matrix2.M32;
		    matrix.M33 = matrix1.M33 - matrix2.M33;
		    matrix.M34 = matrix1.M34 - matrix2.M34;
		    matrix.M41 = matrix1.M41 - matrix2.M41;
		    matrix.M42 = matrix1.M42 - matrix2.M42;
		    matrix.M43 = matrix1.M43 - matrix2.M43;
		    matrix.M44 = matrix1.M44 - matrix2.M44;
		    return matrix;
        }


        public static Matrix operator -(Matrix matrix1)
        {
            Matrix matrix;
		    matrix.M11 = -matrix1.M11;
		    matrix.M12 = -matrix1.M12;
		    matrix.M13 = -matrix1.M13;
		    matrix.M14 = -matrix1.M14;
		    matrix.M21 = -matrix1.M21;
		    matrix.M22 = -matrix1.M22;
		    matrix.M23 = -matrix1.M23;
		    matrix.M24 = -matrix1.M24;
		    matrix.M31 = -matrix1.M31;
		    matrix.M32 = -matrix1.M32;
		    matrix.M33 = -matrix1.M33;
		    matrix.M34 = -matrix1.M34;
		    matrix.M41 = -matrix1.M41;
		    matrix.M42 = -matrix1.M42;
		    matrix.M43 = -matrix1.M43;
		    matrix.M44 = -matrix1.M44;
		    return matrix;
        }


        public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
		    matrix.M11 = matrix1.M11 - matrix2.M11;
		    matrix.M12 = matrix1.M12 - matrix2.M12;
		    matrix.M13 = matrix1.M13 - matrix2.M13;
		    matrix.M14 = matrix1.M14 - matrix2.M14;
		    matrix.M21 = matrix1.M21 - matrix2.M21;
		    matrix.M22 = matrix1.M22 - matrix2.M22;
		    matrix.M23 = matrix1.M23 - matrix2.M23;
		    matrix.M24 = matrix1.M24 - matrix2.M24;
		    matrix.M31 = matrix1.M31 - matrix2.M31;
		    matrix.M32 = matrix1.M32 - matrix2.M32;
		    matrix.M33 = matrix1.M33 - matrix2.M33;
		    matrix.M34 = matrix1.M34 - matrix2.M34;
		    matrix.M41 = matrix1.M41 - matrix2.M41;
		    matrix.M42 = matrix1.M42 - matrix2.M42;
		    matrix.M43 = matrix1.M43 - matrix2.M43;
		    matrix.M44 = matrix1.M44 - matrix2.M44;
		    return matrix;
        }


        public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
		    result.M12 = matrix1.M12 - matrix2.M12;
		    result.M13 = matrix1.M13 - matrix2.M13;
		    result.M14 = matrix1.M14 - matrix2.M14;
		    result.M21 = matrix1.M21 - matrix2.M21;
		    result.M22 = matrix1.M22 - matrix2.M22;
		    result.M23 = matrix1.M23 - matrix2.M23;
		    result.M24 = matrix1.M24 - matrix2.M24;
		    result.M31 = matrix1.M31 - matrix2.M31;
		    result.M32 = matrix1.M32 - matrix2.M32;
		    result.M33 = matrix1.M33 - matrix2.M33;
		    result.M34 = matrix1.M34 - matrix2.M34;
		    result.M41 = matrix1.M41 - matrix2.M41;
		    result.M42 = matrix1.M42 - matrix2.M42;
		    result.M43 = matrix1.M43 - matrix2.M43;
		    result.M44 = matrix1.M44 - matrix2.M44;
        }


        public override string ToString()
        {
            throw new NotImplementedException();
        }


        public static Matrix Transpose(Matrix matrix)
        {
            Matrix ret;
            Transpose(ref matrix, out ret);
            return ret;
        }

        
        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;

            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;

            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;

            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;
        }
        #endregion Public Methods
		
		#region Private Static Methods
        
        /// <summary>
        /// Helper method for using the Laplace expansion theorem using two rows expansions to calculate major and 
        /// minor determinants of a 4x4 matrix. This method is used for inverting a matrix.
        /// </summary>
        private static void findDeterminants(ref Matrix matrix, out float major, 
                                             out float minor1, out float minor2, out float minor3, out float minor4, out float minor5, out float minor6,
                                             out float minor7, out float minor8, out float minor9, out float minor10, out float minor11, out float minor12)
        {
                double det1 = (double)matrix.M11 * (double)matrix.M22 - (double)matrix.M12 * (double)matrix.M21;
                double det2 = (double)matrix.M11 * (double)matrix.M23 - (double)matrix.M13 * (double)matrix.M21;
                double det3 = (double)matrix.M11 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M21;
                double det4 = (double)matrix.M12 * (double)matrix.M23 - (double)matrix.M13 * (double)matrix.M22;
                double det5 = (double)matrix.M12 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M22;
                double det6 = (double)matrix.M13 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M23;
                double det7 = (double)matrix.M31 * (double)matrix.M42 - (double)matrix.M32 * (double)matrix.M41;
                double det8 = (double)matrix.M31 * (double)matrix.M43 - (double)matrix.M33 * (double)matrix.M41;
                double det9 = (double)matrix.M31 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M41;
                double det10 = (double)matrix.M32 * (double)matrix.M43 - (double)matrix.M33 * (double)matrix.M42;
                double det11 = (double)matrix.M32 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M42;
                double det12 = (double)matrix.M33 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M43;
                
                major = (float)(det1*det12 - det2*det11 + det3*det10 + det4*det9 - det5*det8 + det6*det7);
                minor1 = (float)det1;
                minor2 = (float)det2;
                minor3 = (float)det3;
                minor4 = (float)det4;
                minor5 = (float)det5;
                minor6 = (float)det6;
                minor7 = (float)det7;
                minor8 = (float)det8;
                minor9 = (float)det9;
                minor10 = (float)det10;
                minor11 = (float)det11;
                minor12 = (float)det12;
        }
        
        #endregion Private Static Methods

    }
}
