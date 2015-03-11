// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines a frustum and helps determine whether forms intersect with it.
    /// </summary>
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public class BoundingFrustum : IEquatable<BoundingFrustum>
    {
        #region Private Fields

        private Matrix matrix;
        private readonly Vector3[] corners = new Vector3[CornerCount];
        private readonly Plane[] planes = new Plane[PlaneCount];

        private const int PlaneCount = 6;

        #endregion Private Fields

        #region Public Fields
        /// <summary>
        /// Specifies the total number of corners (8) in the <see cref="BoundingFrustum"/>.
        /// </summary>
        public const int CornerCount = 8;
        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new instance of <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="value">Combined matrix that usually takes view × projection matrix.</param>
        public BoundingFrustum(Matrix value)
        {
            this.matrix = value;
            this.CreatePlanes();
            this.CreateCorners();
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets the <see cref="Matrix"/> that describes this bounding frustum.
        /// </summary>
        /// <value>The <see cref="Matrix"/> that describes this bounding frustum.</value>
        /// <remarks>This property can be used to reset an existing <see cref="BoundingFrustum"/> to new values, instead of creating a new one.</remarks>
        public Matrix Matrix
        {
            get { return this.matrix; }
            set
            {
                this.matrix = value;
                this.CreatePlanes();    // FIXME: The odds are the planes will be used a lot more often than the matrix
            	this.CreateCorners();   // is updated, so this should help performance. I hope ;)
			}
        }

        /// <summary>
        /// Gets the near plane of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <value>Returns the near plane of the <see cref="BoundingFrustum"/>.</value>
        public Plane Near
        {
            get { return this.planes[0]; }
        }

        /// <summary>
        /// Gets the far plane of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <value>Returns the far plane of the <see cref="BoundingFrustum"/>.</value>
        public Plane Far
        {
            get { return this.planes[1]; }
        }

        /// <summary>
        /// Gets the left plane of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <value>Returns the left plane of the <see cref="BoundingFrustum"/>.</value>
        public Plane Left
        {
            get { return this.planes[2]; }
        }

        /// <summary>
        /// Gets the right plane of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <value>Returns the right plane of the <see cref="BoundingFrustum"/>.</value>
        public Plane Right
        {
            get { return this.planes[3]; }
        }

        /// <summary>
        /// Gets the top plane of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <value>Returns the top plane of the <see cref="BoundingFrustum"/>.</value>
        public Plane Top
        {
            get { return this.planes[4]; }
        }

        /// <summary>
        /// Gets the bottom plane of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <value>Returns the bottom plane of the <see cref="BoundingFrustum"/>.</value>
        public Plane Bottom
        {
            get { return this.planes[5]; }
        }

        #endregion Public Properties


        #region Public Methods

        public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
        {
            if (object.Equals(a, null))
                return (object.Equals(b, null));

            if (object.Equals(b, null))
                return (object.Equals(a, null));

            return a.matrix == (b.matrix);
        }

        public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to check against the current <see cref="BoundingFrustum"/>.</param>
        /// <returns>Enumeration indicating the relationship of the current <see cref="BoundingFrustum"/> to the specified <see cref="BoundingBox"/>.</returns>
        public ContainmentType Contains(BoundingBox box)
        {
            var result = default(ContainmentType);
            this.Contains(ref box, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for overlap.</param>
        /// <param name="result">Enumeration indicating the extent of overlap.</param>
        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            var intersects = false;
            for (var i = 0; i < PlaneCount; ++i)
            {
                var planeIntersectionType = default(PlaneIntersectionType);
                box.Intersects(ref this.planes[i], out planeIntersectionType);
                switch (planeIntersectionType)
                {
                case PlaneIntersectionType.Front:
                    result = ContainmentType.Disjoint; 
                    return;
                case PlaneIntersectionType.Intersecting:
                    intersects = true;
                    break;
                }
            }
            result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="frustum">The <see cref="BoundingFrustum"/> to check against the current <see cref="BoundingFrustum"/>.</param>
        /// <returns>Enumeration indicating the relationship of the current <see cref="BoundingFrustum"/> to the specified <see cref="BoundingFrustum"/>.</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            if (this == frustum)                // We check to see if the two frustums are equal
                return ContainmentType.Contains;// If they are, there's no need to go any further.

            var intersects = false;
            for (var i = 0; i < PlaneCount; ++i)
            {
                PlaneIntersectionType planeIntersectionType;
                frustum.Intersects(ref planes[i], out planeIntersectionType);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        return ContainmentType.Disjoint;
                    case PlaneIntersectionType.Intersecting:
                        intersects = true;
                        break;
                }
            }
            return intersects ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to check against the current <see cref="BoundingFrustum"/>.</param>
        /// <returns>Enumeration indicating the relationship of the current <see cref="BoundingFrustum"/> to the specified <see cref="BoundingSphere"/>.</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            var result = default(ContainmentType);
            this.Contains(ref sphere, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to test for overlap.</param>
        /// <param name="result">Enumeration indicating the extent of overlap.</param>
        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            var intersects = false;
            for (var i = 0; i < PlaneCount; ++i) 
            {
                var planeIntersectionType = default(PlaneIntersectionType);

                // TODO: we might want to inline this for performance reasons
                sphere.Intersects(ref this.planes[i], out planeIntersectionType);
                switch (planeIntersectionType)
                {
                case PlaneIntersectionType.Front:
                    result = ContainmentType.Disjoint; 
                    return;
                case PlaneIntersectionType.Intersecting:
                    intersects = true;
                    break;
                }
            }
            result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified point.
        /// </summary>
        /// <param name="point">The point to check against the current <see cref="BoundingFrustum"/>.</param>
        /// <returns>Enumeration indicating the relationship of the current <see cref="BoundingFrustum"/> to the specified point.</returns>
        public ContainmentType Contains(Vector3 point)
        {
            var result = default(ContainmentType);
            this.Contains(ref point, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> contains the specified point.
        /// </summary>
        /// <param name="point">The point to test for overlap.</param>
        /// <param name="result">Enumeration indicating the extent of overlap.</param>
        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            for (var i = 0; i < PlaneCount; ++i)
            {
                // TODO: we might want to inline this for performance reasons
                if (PlaneHelper.ClassifyPoint(ref point, ref this.planes[i]) > 0)
                {
                    result = ContainmentType.Disjoint;
                    return;
                }
            }
            result = ContainmentType.Contains;
        }

        /// <summary>
        /// Determines whether the specified <see cref="BoundingFrustum"/> is equal to the current <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="other">The <see cref="BoundingFrustum"/> to compare with the current <see cref="BoundingFrustum"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="BoundingFrustum"/> is equal to the current <see cref="BoundingFrustum"/>; <c>false</c> otherwise.</returns>
        public bool Equals(BoundingFrustum other)
        {
            return (this == other);
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="obj">The Object to compare with the current <see cref="BoundingFrustum"/>.</param>
        /// <returns><c>true</c> if the specified Object is equal to the current <see cref="BoundingFrustum"/>; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            BoundingFrustum f = obj as BoundingFrustum;
            return (object.Equals(f, null)) ? false : (this == f);
        }

        /// <summary>
        /// Gets an array of points that make up the corners of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <returns>Array of Vector3 points that make up the corners of the <see cref="BoundingFrustum"/>.</returns>
        /// <remarks>The points returned correspond to the corners of the <see cref="BoundingFrustum"/> faces that are perpendicular to the z-axis. The near face is the face with the larger z value, and the far face is the face with the smaller z value. Points 0 to 3 correspond to the near face in a clockwise order starting at its upper-left corner when looking toward the origin from the positive z direction. Points 4 to 7 correspond to the far face in a clockwise order starting at its upper-left corner when looking toward the origin from the positive z direction.</remarks>
        public Vector3[] GetCorners()
        {
            return (Vector3[])this.corners.Clone();
        }
		
        /// <summary>
        /// Gets an array of points that make up the corners of the <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="corners">An existing array of at least 8 <see cref="Vector3"/> points where the corners of the <see cref="BoundingFrustum"/> are written.</param>
        /// <exception cref="ArgumentNullException">corners is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">You have to have at least 8 elements to copy corners.</exception>
        /// <remarks>The points returned correspond to the corners of the <see cref="BoundingFrustum"/> faces that are perpendicular to the z-axis. The near face is the face with the larger z value, and the far face is the face with the smaller z value. Points 0 to 3 correspond to the near face in a clockwise order starting at its upper-left corner when looking toward the origin from the positive z direction. Points 4 to 7 correspond to the far face in a clockwise order starting at its upper-left corner when looking toward the origin from the positive z direction.</remarks>
		public void GetCorners(Vector3[] corners)
        {
			if (corners == null) throw new ArgumentNullException("corners");
		    if (corners.Length < CornerCount) throw new ArgumentOutOfRangeException("corners");

            this.corners.CopyTo(corners, 0);
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="BoundingFrustum"/>.</returns>
        public override int GetHashCode()
        {
            return this.matrix.GetHashCode();
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to check for intersection.</param>
        /// <returns><c>true</c> if the <see cref="BoundingFrustum"/> intersects the <see cref="BoundingBox"/>; <c>false</c> otherwise.</returns>
        public bool Intersects(BoundingBox box)
        {
			var result = false;
			this.Intersects(ref box, out result);
			return result;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects a <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to check for intersection with.</param>
        /// <param name="result"><c>true</c> if the <see cref="BoundingFrustum"/> and <see cref="BoundingBox"/> intersect; <c>false</c> otherwise.</param>
        public void Intersects(ref BoundingBox box, out bool result)
        {
			var containment = default(ContainmentType);
			this.Contains(ref box, out containment);
			result = containment != ContainmentType.Disjoint;
		}

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects the specified <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="frustum">The <see cref="BoundingFrustum"/> to check for intersection.</param>
        /// <returns><c>true</c> if the current <see cref="BoundingFrustum"/> intersects the specified <see cref="BoundingFrustum"/>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">frustum is <c>null</c>.</exception>
        public bool Intersects(BoundingFrustum frustum)
        {
            return Contains(frustum) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to check for intersection.</param>
        /// <returns><c>true</c> if the <see cref="BoundingFrustum"/> intersects the <see cref="BoundingSphere"/>; <c>false</c> otherwise.</returns>
        public bool Intersects(BoundingSphere sphere)
        {
            var result = default(bool);
            this.Intersects(ref sphere, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to check for intersection with.</param>
        /// <param name="result"><c>true</c> if the <see cref="BoundingFrustum"/> and <see cref="BoundingSphere"/> intersect; <c>false</c> otherwise.</param>
        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            var containment = default(ContainmentType);
            this.Contains(ref sphere, out containment);
            result = containment != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects the specified <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to check for intersection.</param>
        /// <returns>An enumeration indicating whether <see cref="BoundingFrustum"/> intersects the specified <see cref="Plane"/>.</returns>
        public PlaneIntersectionType Intersects(Plane plane)
        {
            PlaneIntersectionType result;
            Intersects(ref plane, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to check for intersection with.</param>
        /// <param name="result">An enumeration indicating whether the <see cref="BoundingFrustum"/> intersects the <see cref="Plane"/>.</param>
        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            result = plane.Intersects(ref corners[0]);
            for (int i = 1; i < corners.Length; i++)
                if (plane.Intersects(ref corners[i]) != result)
                    result = PlaneIntersectionType.Intersecting;
        }

        /*
        /// <summary>
        /// Checks whether the current <see cref="BoundingFrustum"/> intersects the specified <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The <see cref="Ray"/> to check for intersection.</param>
        /// <returns>Distance at which the ray intersects the <see cref="BoundingFrustum"/> or <c>null</c> if there is no intersection.</returns>
        public Nullable<float> Intersects(Ray ray)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a <see cref="Ray"/>.
        /// </summary>
        /// <param name="ray">The <see cref="Ray"/> to check for intersection with.</param>
        /// <param name="result">Distance at which the ray intersects the <see cref="BoundingFrustum"/> or <c>null</c> if there is no intersection.</param>
        public void Intersects(ref Ray ray, out Nullable<float> result)
        {
            throw new NotImplementedException();
        }
        */

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Near( ", this.planes[0].DebugDisplayString, " )  \r\n",
                    "Far( ", this.planes[1].DebugDisplayString, " )  \r\n",
                    "Left( ", this.planes[2].DebugDisplayString, " )  \r\n",
                    "Right( ", this.planes[3].DebugDisplayString, " )  \r\n",
                    "Top( ", this.planes[4].DebugDisplayString, " )  \r\n",
                    "Bottom( ", this.planes[5].DebugDisplayString, " )  "                  
                    );
            }
        }

        /// <summary>
        /// Returns a String that represents the current <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <returns>String representation of the current <see cref="BoundingFrustum"/>.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("{Near:");
            sb.Append(this.planes[0].ToString());
            sb.Append(" Far:");
            sb.Append(this.planes[1].ToString());
            sb.Append(" Left:");
            sb.Append(this.planes[2].ToString());
            sb.Append(" Right:");
            sb.Append(this.planes[3].ToString());
            sb.Append(" Top:");
            sb.Append(this.planes[4].ToString());
            sb.Append(" Bottom:");
            sb.Append(this.planes[5].ToString());
            sb.Append("}");
            return sb.ToString();
        }

        #endregion Public Methods


        #region Private Methods

        private void CreateCorners()
        {
            IntersectionPoint(ref this.planes[0], ref this.planes[2], ref this.planes[4], out this.corners[0]);
            IntersectionPoint(ref this.planes[0], ref this.planes[3], ref this.planes[4], out this.corners[1]);
            IntersectionPoint(ref this.planes[0], ref this.planes[3], ref this.planes[5], out this.corners[2]);
            IntersectionPoint(ref this.planes[0], ref this.planes[2], ref this.planes[5], out this.corners[3]);
            IntersectionPoint(ref this.planes[1], ref this.planes[2], ref this.planes[4], out this.corners[4]);
            IntersectionPoint(ref this.planes[1], ref this.planes[3], ref this.planes[4], out this.corners[5]);
            IntersectionPoint(ref this.planes[1], ref this.planes[3], ref this.planes[5], out this.corners[6]);
            IntersectionPoint(ref this.planes[1], ref this.planes[2], ref this.planes[5], out this.corners[7]);
        }

        private void CreatePlanes()
        {            
            this.planes[0] = new Plane(-this.matrix.M13, -this.matrix.M23, -this.matrix.M33, -this.matrix.M43);
            this.planes[1] = new Plane(this.matrix.M13 - this.matrix.M14, this.matrix.M23 - this.matrix.M24, this.matrix.M33 - this.matrix.M34, this.matrix.M43 - this.matrix.M44);
            this.planes[2] = new Plane(-this.matrix.M14 - this.matrix.M11, -this.matrix.M24 - this.matrix.M21, -this.matrix.M34 - this.matrix.M31, -this.matrix.M44 - this.matrix.M41);
            this.planes[3] = new Plane(this.matrix.M11 - this.matrix.M14, this.matrix.M21 - this.matrix.M24, this.matrix.M31 - this.matrix.M34, this.matrix.M41 - this.matrix.M44);
            this.planes[4] = new Plane(this.matrix.M12 - this.matrix.M14, this.matrix.M22 - this.matrix.M24, this.matrix.M32 - this.matrix.M34, this.matrix.M42 - this.matrix.M44);
            this.planes[5] = new Plane(-this.matrix.M14 - this.matrix.M12, -this.matrix.M24 - this.matrix.M22, -this.matrix.M34 - this.matrix.M32, -this.matrix.M44 - this.matrix.M42);
            
            this.NormalizePlane(ref this.planes[0]);
            this.NormalizePlane(ref this.planes[1]);
            this.NormalizePlane(ref this.planes[2]);
            this.NormalizePlane(ref this.planes[3]);
            this.NormalizePlane(ref this.planes[4]);
            this.NormalizePlane(ref this.planes[5]);
        }

        private static void IntersectionPoint(ref Plane a, ref Plane b, ref Plane c, out Vector3 result)
        {
            // Formula used
            //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
            //P =   -------------------------------------------------------------------------
            //                             N1 . ( N2 * N3 )
            //
            // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product
            
            Vector3 v1, v2, v3;
            Vector3 cross;
            
            Vector3.Cross(ref b.Normal, ref c.Normal, out cross);
            
            float f;
            Vector3.Dot(ref a.Normal, ref cross, out f);
            f *= -1.0f;
            
            Vector3.Cross(ref b.Normal, ref c.Normal, out cross);
            Vector3.Multiply(ref cross, a.D, out v1);
            //v1 = (a.D * (Vector3.Cross(b.Normal, c.Normal)));
            
            
            Vector3.Cross(ref c.Normal, ref a.Normal, out cross);
            Vector3.Multiply(ref cross, b.D, out v2);
            //v2 = (b.D * (Vector3.Cross(c.Normal, a.Normal)));
            
            
            Vector3.Cross(ref a.Normal, ref b.Normal, out cross);
            Vector3.Multiply(ref cross, c.D, out v3);
            //v3 = (c.D * (Vector3.Cross(a.Normal, b.Normal)));
            
            result.X = (v1.X + v2.X + v3.X) / f;
            result.Y = (v1.Y + v2.Y + v3.Y) / f;
            result.Z = (v1.Z + v2.Z + v3.Z) / f;
        }
        
        private void NormalizePlane(ref Plane p)
        {
            float factor = 1f / p.Normal.Length();
            p.Normal.X *= factor;
            p.Normal.Y *= factor;
            p.Normal.Z *= factor;
            p.D *= factor;
        }

        #endregion
    }
}

