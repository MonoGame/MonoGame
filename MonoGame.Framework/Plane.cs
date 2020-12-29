// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
	internal class PlaneHelper
    {
        /// <summary>
        /// Returns a value indicating what side (positive/negative) of a plane a point is
        /// </summary>
        /// <param name="point">The point to check with</param>
        /// <param name="plane">The plane to check against</param>
        /// <returns>Greater than zero if on the positive side, less than zero if on the negative size, 0 otherwise</returns>
        public static float ClassifyPoint(ref Vector3 point, ref Plane plane)
        {
            return point.X * plane.Normal.X + point.Y * plane.Normal.Y + point.Z * plane.Normal.Z + plane.D;
        }

        /// <summary>
        /// Returns the perpendicular distance from a point to a plane
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="plane">The place to check</param>
        /// <returns>The perpendicular distance from the point to the plane</returns>
        public static float PerpendicularDistance(ref Vector3 point, ref Plane plane)
        {
            // dist = (ax + by + cz + d) / sqrt(a*a + b*b + c*c)
            return (float)Math.Abs((plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z)
                                    / Math.Sqrt(plane.Normal.X * plane.Normal.X + plane.Normal.Y * plane.Normal.Y + plane.Normal.Z * plane.Normal.Z));
        }
    }
	
    /// <summary>
    /// A plane in 3d space, represented by its normal away from the origin and its distance from the origin, D.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct Plane : IEquatable<Plane>
    {
        #region Public Fields

        /// <summary>
        /// The distance of the <see cref="Plane"/> to the origin.
        /// </summary>
        [DataMember]
        public float D;

        /// <summary>
        /// The normal of the <see cref="Plane"/>.
        /// </summary>
        [DataMember]
        public Vector3 Normal;

        #endregion Public Fields


        #region Constructors

        /// <summary>
        /// Create a <see cref="Plane"/> with the first three components of the specified <see cref="Vector4"/>
        /// as the normal and the last component as the distance to the origin.
        /// </summary>
        /// <param name="value">A vector holding the normal and distance to origin.</param>
        public Plane(Vector4 value)
            : this(new Vector3(value.X, value.Y, value.Z), value.W)
        {

        }

        /// <summary>
        /// Create a <see cref="Plane"/> with the specified normal and distance to the origin.
        /// </summary>
        /// <param name="normal">The normal of the plane.</param>
        /// <param name="d">The distance to the origin.</param>
        public Plane(Vector3 normal, float d)
        {
            Normal = normal;
            D = d;
        }

        /// <summary>
        /// Create the <see cref="Plane"/> that contains the three specified points.
        /// </summary>
        /// <param name="a">A point the created <see cref="Plane"/> should contain.</param>
        /// <param name="b">A point the created <see cref="Plane"/> should contain.</param>
        /// <param name="c">A point the created <see cref="Plane"/> should contain.</param>
        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;

            Vector3 cross = Vector3.Cross(ab, ac);
            Vector3.Normalize(ref cross, out Normal);
            D = -(Vector3.Dot(Normal, a));
        }

        /// <summary>
        /// Create a <see cref="Plane"/> with the first three values as the X, Y and Z
        /// components of the normal and the last value as the distance to the origin.
        /// </summary>
        /// <param name="a">The X component of the normal.</param>
        /// <param name="b">The Y component of the normal.</param>
        /// <param name="c">The Z component of the normal.</param>
        /// <param name="d">The distance to the origin.</param>
        public Plane(float a, float b, float c, float d)
            : this(new Vector3(a, b, c), d)
        {

        }

        /// <summary>
        /// Create a <see cref="Plane"/> that contains the specified point and has the specified <see cref="Normal"/> vector.
        /// </summary>
        /// <param name="pointOnPlane">A point the created <see cref="Plane"/> should contain.</param>
        /// <param name="normal">The normal of the plane.</param>
        public Plane(Vector3 pointOnPlane, Vector3 normal)
        {
            Normal = normal;
            D = -(
                pointOnPlane.X * normal.X +
                pointOnPlane.Y * normal.Y +
                pointOnPlane.Z * normal.Z
            );
        }

        #endregion Constructors


        #region Public Methods

        /// <summary>
        /// Get the dot product of a <see cref="Vector4"/> with this <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4"/> to calculate the dot product with.</param>
        /// <returns>The dot product of the specified <see cref="Vector4"/> and this <see cref="Plane"/>.</returns>
        public float Dot(Vector4 value)
        {
            return ((((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + (this.D * value.W));
        }

        /// <summary>
        /// Get the dot product of a <see cref="Vector4"/> with this <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4"/> to calculate the dot product with.</param>
        /// <param name="result">
        /// The dot product of the specified <see cref="Vector4"/> and this <see cref="Plane"/>.
        /// </param>
        public void Dot(ref Vector4 value, out float result)
        {
            result = (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + (this.D * value.W);
        }

        /// <summary>
        /// Get the dot product of a <see cref="Vector3"/> with
        /// the <see cref="Normal"/> vector of this <see cref="Plane"/>
        /// plus the <see cref="D"/> value of this <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3"/> to calculate the dot product with.</param>
        /// <returns>
        /// The dot product of the specified <see cref="Vector3"/> and the normal of this <see cref="Plane"/>
        /// plus the <see cref="D"/> value of this <see cref="Plane"/>.
        /// </returns>
        public float DotCoordinate(Vector3 value)
        {
            return ((((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + this.D);
        }

        /// <summary>
        /// Get the dot product of a <see cref="Vector3"/> with
        /// the <see cref="Normal"/> vector of this <see cref="Plane"/>
        /// plus the <see cref="D"/> value of this <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3"/> to calculate the dot product with.</param>
        /// <param name="result">
        /// The dot product of the specified <see cref="Vector3"/> and the normal of this <see cref="Plane"/>
        /// plus the <see cref="D"/> value of this <see cref="Plane"/>.
        /// </param>
        public void DotCoordinate(ref Vector3 value, out float result)
        {
            result = (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z)) + this.D;
        }

        /// <summary>
        /// Get the dot product of a <see cref="Vector3"/> with
        /// the <see cref="Normal"/> vector of this <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3"/> to calculate the dot product with.</param>
        /// <returns>
        /// The dot product of the specified <see cref="Vector3"/> and the normal of this <see cref="Plane"/>.
        /// </returns>
        public float DotNormal(Vector3 value)
        {
            return (((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z));
        }

        /// <summary>
        /// Get the dot product of a <see cref="Vector3"/> with
        /// the <see cref="Normal"/> vector of this <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3"/> to calculate the dot product with.</param>
        /// <param name="result">
        /// The dot product of the specified <see cref="Vector3"/> and the normal of this <see cref="Plane"/>.
        /// </param>
        public void DotNormal(ref Vector3 value, out float result)
        {
            result = ((this.Normal.X * value.X) + (this.Normal.Y * value.Y)) + (this.Normal.Z * value.Z);
        }

        /// <summary>
        /// Transforms a normalized plane by a matrix.
        /// </summary>
        /// <param name="plane">The normalized plane to transform.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed plane.</returns>
        public static Plane Transform(Plane plane, Matrix matrix)
        {
            Plane result;
            Transform(ref plane, ref matrix, out result);
            return result;
        }

        /// <summary>
        /// Transforms a normalized plane by a matrix.
        /// </summary>
        /// <param name="plane">The normalized plane to transform.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <param name="result">The transformed plane.</param>
        public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
        {
            // See "Transforming Normals" in http://www.glprogramming.com/red/appendixf.html
            // for an explanation of how this works.

            Matrix transformedMatrix;
            Matrix.Invert(ref matrix, out transformedMatrix);
            Matrix.Transpose(ref transformedMatrix, out transformedMatrix);

            var vector = new Vector4(plane.Normal, plane.D);

            Vector4 transformedVector;
            Vector4.Transform(ref vector, ref transformedMatrix, out transformedVector);

            result = new Plane(transformedVector);
        }

        /// <summary>
        /// Transforms a normalized plane by a quaternion rotation.
        /// </summary>
        /// <param name="plane">The normalized plane to transform.</param>
        /// <param name="rotation">The quaternion rotation.</param>
        /// <returns>The transformed plane.</returns>
        public static Plane Transform(Plane plane, Quaternion rotation)
        {
            Plane result;
            Transform(ref plane, ref rotation, out result);
            return result;
        }

        /// <summary>
        /// Transforms a normalized plane by a quaternion rotation.
        /// </summary>
        /// <param name="plane">The normalized plane to transform.</param>
        /// <param name="rotation">The quaternion rotation.</param>
        /// <param name="result">The transformed plane.</param>
        public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
        {
            Vector3.Transform(ref plane.Normal, ref rotation, out result.Normal);
            result.D = plane.D;
        }

        /// <summary>
        /// Normalize the normal vector of this plane.
        /// </summary>
        public void Normalize()
        {
            float length = Normal.Length();
            float factor =  1f / length;            
            Vector3.Multiply(ref Normal, factor, out Normal);
            D = D * factor;
        }

        /// <summary>
        /// Get a normalized version of the specified plane.
        /// </summary>
        /// <param name="value">The <see cref="Plane"/> to normalize.</param>
        /// <returns>A normalized version of the specified <see cref="Plane"/>.</returns>
        public static Plane Normalize(Plane value)
        {
			Plane ret;
			Normalize(ref value, out ret);
			return ret;
        }

        /// <summary>
        /// Get a normalized version of the specified plane.
        /// </summary>
        /// <param name="value">The <see cref="Plane"/> to normalize.</param>
        /// <param name="result">A normalized version of the specified <see cref="Plane"/>.</param>
        public static void Normalize(ref Plane value, out Plane result)
        {
            float length = value.Normal.Length();
            float factor =  1f / length;            
            Vector3.Multiply(ref value.Normal, factor, out result.Normal);
            result.D = value.D * factor;
        }

        /// <summary>
        /// Check if two planes are not equal.
        /// </summary>
        /// <param name="plane1">A <see cref="Plane"/> to check for inequality.</param>
        /// <param name="plane2">A <see cref="Plane"/> to check for inequality.</param>
        /// <returns><code>true</code> if the two planes are not equal, <code>false</code> if they are.</returns>
        public static bool operator !=(Plane plane1, Plane plane2)
        {
            return !plane1.Equals(plane2);
        }

        /// <summary>
        /// Check if two planes are equal.
        /// </summary>
        /// <param name="plane1">A <see cref="Plane"/> to check for equality.</param>
        /// <param name="plane2">A <see cref="Plane"/> to check for equality.</param>
        /// <returns><code>true</code> if the two planes are equal, <code>false</code> if they are not.</returns>
        public static bool operator ==(Plane plane1, Plane plane2)
        {
            return plane1.Equals(plane2);
        }

        /// <summary>
        /// Check if this <see cref="Plane"/> is equal to another <see cref="Plane"/>.
        /// </summary>
        /// <param name="other">An <see cref="Object"/> to check for equality with this <see cref="Plane"/>.</param>
        /// <returns>
        /// <code>true</code> if the specified <see cref="object"/> is equal to this <see cref="Plane"/>,
        /// <code>false</code> if it is not.
        /// </returns>
        public override bool Equals(object other)
        {
            return (other is Plane) ? this.Equals((Plane)other) : false;
        }

        /// <summary>
        /// Check if this <see cref="Plane"/> is equal to another <see cref="Plane"/>.
        /// </summary>
        /// <param name="other">A <see cref="Plane"/> to check for equality with this <see cref="Plane"/>.</param>
        /// <returns>
        /// <code>true</code> if the specified <see cref="Plane"/> is equal to this one,
        /// <code>false</code> if it is not.
        /// </returns>
        public bool Equals(Plane other)
        {
            return ((Normal == other.Normal) && (D == other.D));
        }

        /// <summary>
        /// Get a hash code for this <see cref="Plane"/>.
        /// </summary>
        /// <returns>A hash code for this <see cref="Plane"/>.</returns>
        public override int GetHashCode()
        {
            return Normal.GetHashCode() ^ D.GetHashCode();
        }


        /// <summary>
        /// Check if this <see cref="Plane"/> intersects a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <returns>
        /// The type of intersection of this <see cref="Plane"/> with the specified <see cref="BoundingBox"/>.
        /// </returns>
        public PlaneIntersectionType Intersects(BoundingBox box)
        {
            return box.Intersects(this);
        }

        /// <summary>
        /// Check if this <see cref="Plane"/> intersects a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <param name="result">
        /// The type of intersection of this <see cref="Plane"/> with the specified <see cref="BoundingBox"/>.
        /// </param>
        public void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
        {
            box.Intersects (ref this, out result);
        }

        /// <summary>
        /// Check if this <see cref="Plane"/> intersects a <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="frustum">The <see cref="BoundingFrustum"/> to test for intersection.</param>
        /// <returns>
        /// The type of intersection of this <see cref="Plane"/> with the specified <see cref="BoundingFrustum"/>.
        /// </returns>
        public PlaneIntersectionType Intersects(BoundingFrustum frustum)
        {
            return frustum.Intersects(this);
        }

        /// <summary>
        /// Check if this <see cref="Plane"/> intersects a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to test for intersection.</param>
        /// <returns>
        /// The type of intersection of this <see cref="Plane"/> with the specified <see cref="BoundingSphere"/>.
        /// </returns>
        public PlaneIntersectionType Intersects(BoundingSphere sphere)
        {
            return sphere.Intersects(this);
        }

        /// <summary>
        /// Check if this <see cref="Plane"/> intersects a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to test for intersection.</param>
        /// <param name="result">
        /// The type of intersection of this <see cref="Plane"/> with the specified <see cref="BoundingSphere"/>.
        /// </param>
        public void Intersects(ref BoundingSphere sphere, out PlaneIntersectionType result)
        {
            sphere.Intersects(ref this, out result);
        }

        internal PlaneIntersectionType Intersects(ref Vector3 point)
        {
            float distance;
            DotCoordinate(ref point, out distance);

            if (distance > 0)
                return PlaneIntersectionType.Front;

            if (distance < 0)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    this.Normal.DebugDisplayString, "  ",
                    this.D.ToString()
                    );
            }
        }

        /// <summary>
        /// Get a <see cref="String"/> representation of this <see cref="Plane"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Plane"/>.</returns>
        public override string ToString()
        {
            return "{Normal:" + Normal + " D:" + D + "}";
        }

        /// <summary>
        /// Deconstruction method for <see cref="Plane"/>.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="d"></param>
        public void Deconstruct(out Vector3 normal, out float d)
        {
            normal = Normal;
            d = D;
        }

        /// <summary>
        /// Returns a <see cref="System.Numerics.Plane"/>.
        /// </summary>
        public System.Numerics.Plane ToNumerics()
        {
            return new System.Numerics.Plane(this.Normal.X, this.Normal.Y, this.Normal.Z, this.D);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Converts a <see cref="System.Numerics.Plane"/> to a <see cref="Plane"/>.
        /// </summary>
        /// <param name="value">The converted value.</param>
        public static implicit operator Plane(System.Numerics.Plane value)
        {
            return new Plane(value.Normal, value.D);
        }

        #endregion
    }
}

