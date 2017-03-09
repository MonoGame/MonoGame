// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;


/// <summary>
/// Defines a box in 3D-space
/// </summary>

namespace Microsoft.Xna.Framework
{
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct BoundingBox : IEquatable<BoundingBox>
    {

        #region Public Fields

        [DataMember]
        public Vector3 Min;
      
        [DataMember]
        public Vector3 Max;

        public const int CornerCount = 8;

        #endregion Public Fields


        #region Public Constructors

        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        #endregion Public Constructors


        #region Public Methods


        /// <summary>
        /// Determines the <see cref="ContainmentType"/> between this <see cref="BoundingBox"/> and another.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to compare to.</param>
        /// <returns>The <see cref="ContainmentType"/> relationship between the two objects.</returns>
        public ContainmentType Contains(BoundingBox box)
        {
            // If, in any dimension: the other box's MAX is less than this box's MIN, OR
            //                       the other box's MIN is is more than this box's MAX
            //                    then the two must be disjoint
            if (box.Max.X < Min.X
                || box.Min.X > Max.X
                || box.Max.Y < Min.Y
                || box.Min.Y > Max.Y
                || box.Max.Z < Min.Z
                || box.Min.Z > Max.Z)
                return ContainmentType.Disjoint;

            // Apply DeMorgan's property to the "disjoint" condition above to arrive at the "contains" condition:
            if (box.Min.X >= Min.X
                && box.Max.X <= Max.X
                && box.Min.Y >= Min.Y
                && box.Max.Y <= Max.Y
                && box.Min.Z >= Min.Z
                && box.Max.Z <= Max.Z)
                return ContainmentType.Contains;

            // Not disjoint and not containing -> intersecting
            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Determines the <see cref="ContainmentType"/> between this <see cref="BoundingBox"/> and another, putting the result in the <see cref="ContainmentType"/> parameter.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to compare to.</param>
        /// <param name="result">The <see cref="ContainmentType"/> to store the result in.</param>
        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = Contains(box);
        }

        /// <summary>
        /// Determines the <see cref="ContainmentType"/> between this <see cref="BoundingBox"/> and a given <see cref="BoundingFrustum"/>.
        /// </summary>
        /// <param name="frustum">The <see cref="BoundingFrustum"/> to compare to.</param>
        /// <returns>The <see cref="ContainmentType"/> relationship between the two objects.</returns>
        public ContainmentType Contains(BoundingFrustum frustum)
        {
            //TODO: bad done here need a fix. 
            //Because question is not frustum contain box but reverse and this is not the same
            int i;
            ContainmentType contained;
            Vector3[] corners = frustum.GetCorners();

            // First we check if frustum is in box
            for (i = 0; i < corners.Length; i++)
            {
                this.Contains(ref corners[i], out contained);
                if (contained == ContainmentType.Disjoint)
                    break;
            }

            if (i == corners.Length) // This means we checked all the corners and they were all contain or instersect
                return ContainmentType.Contains;

            if (i != 0)             // if i is not equal to zero, we can fastpath and say that this box intersects
                return ContainmentType.Intersects;


            // If we get here, it means the first (and only) point we checked was actually contained in the frustum.
            // So we assume that all other points will also be contained. If one of the points is disjoint, we can
            // exit immediately saying that the result is Intersects
            i++;
            for (; i < corners.Length; i++)
            {
                this.Contains(ref corners[i], out contained);
                if (contained != ContainmentType.Contains)
                    return ContainmentType.Intersects;

            }

            // If we get here, then we know all the points were actually contained, therefore result is Contains
            return ContainmentType.Contains;
        }

        /// <summary>
        /// Determines the <see cref="ContainmentType"/> between this <see cref="BoundingBox"/> and a <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to compare to.</param>
        /// <returns>The <see cref="ContainmentType"/> relationship between the two objects.</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            // A box contains a sphere if:
            //     The box's minimum, in every dimension, is less than the sphere's center minus its radius, AND
            //     The box's maximum, in every dimension, is more than the sphere's center plus it radius
            if (sphere.Center.X - Min.X >= sphere.Radius
                && sphere.Center.Y - Min.Y >= sphere.Radius
                && sphere.Center.Z - Min.Z >= sphere.Radius
                && Max.X - sphere.Center.X >= sphere.Radius
                && Max.Y - sphere.Center.Y >= sphere.Radius
                && Max.Z - sphere.Center.Z >= sphere.Radius)
                return ContainmentType.Contains;

            // If we get here, we know the sphere is not entirely contained within the box

            // For in-depth explanation, see: https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection
            //                           and: www.mrtc.mdh.se/projects/3Dgraphics/paperF.pdf

            double dmin = 0;

            // Dimension X
            double e = sphere.Center.X - Min.X;
            if (e < 0)
            {
                if (e < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                dmin += e * e;
            }
            else
            {
                e = sphere.Center.X - Max.X;
                if (e > 0)
                {
                    if (e > sphere.Radius)
                    {
                        return ContainmentType.Disjoint;
                    }
                    dmin += e * e;
                }
            }

            // Dimension Y
            e = sphere.Center.Y - Min.Y;
            if (e < 0)
            {
                if (e < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                dmin += e * e;
            }
            else
            {
                e = sphere.Center.Y - Max.Y;
                if (e > 0)
                {
                    if (e > sphere.Radius)
                    {
                        return ContainmentType.Disjoint;
                    }
                    dmin += e * e;
                }
            }

            // Dimension Z
            e = sphere.Center.Z - Min.Z;
            if (e < 0)
            {
                if (e < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                dmin += e * e;
            }
            else
            {
                e = sphere.Center.Z - Max.Z;
                if (e > 0)
                {
                    if (e > sphere.Radius)
                    {
                        return ContainmentType.Disjoint;
                    }
                    dmin += e * e;
                }
            }

            if (dmin <= sphere.Radius * sphere.Radius)
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }

        /// <summary>
        /// Determines the <see cref="ContainmentType"/> between this <see cref="BoundingBox"/> and a <see cref="BoundingSphere"/>, putting the result in the <see cref="ContainmentType"/> parameter.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to compare to.</param>
        /// <param name="result">The <see cref="ContainmentType"/> to store the result in.</param>
        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            result = this.Contains(sphere);
        }

        /// <summary>
        /// Determines if a given <see cref="Vector3"/> is contained by the <see cref="ContainmentType"/>.
        /// </summary>
        /// <param name="point">The <see cref="Vector3"/> to compare to.</param>
        /// <returns>The <see cref="ContainmentType"/> relationship between the two objects.</returns>
        public ContainmentType Contains(Vector3 point)
        {
            ContainmentType result;
            this.Contains(ref point, out result);
            return result;
        }


        /// <summary>
        /// Determines if a given <see cref="Vector3"/> is contained by this <see cref="BoundingBox"/>, putting the result in the <see cref="ContainmentType"/> parameter.
        /// </summary>
        /// <param name="point">The <see cref="Vector3"/> to compare to.</param>
        /// <param name="result">The <see cref="ContainmentType"/> to store the result in.</param>
        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            // First we get if point is out of box
            if (point.X < this.Min.X
                || point.X > this.Max.X
                || point.Y < this.Min.Y
                || point.Y > this.Max.Y
                || point.Z < this.Min.Z
                || point.Z > this.Max.Z)
            {
                result = ContainmentType.Disjoint;
            }
            else
            {
                result = ContainmentType.Contains;
            }
        }

        private static readonly Vector3 MaxVector3 = new Vector3(float.MaxValue);
        private static readonly Vector3 MinVector3 = new Vector3(float.MinValue);

        /// <summary>
        /// Create a bounding box from the given list of points.
        /// </summary>
        /// <param name="points">The list of Vector3 instances defining the point cloud to bound</param>
        /// <returns>A bounding box that encapsulates the given point cloud.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the given list has no points.</exception>
        public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
        {
            if (points == null)
                throw new ArgumentNullException();

            var empty = true;
            var minVec = MaxVector3;
            var maxVec = MinVector3;

            // Global min/max must contain all points within the cloud
            foreach (var ptVector in points)
            {
                minVec.X = (minVec.X < ptVector.X) ? minVec.X : ptVector.X;
                minVec.Y = (minVec.Y < ptVector.Y) ? minVec.Y : ptVector.Y;
                minVec.Z = (minVec.Z < ptVector.Z) ? minVec.Z : ptVector.Z;

                maxVec.X = (maxVec.X > ptVector.X) ? maxVec.X : ptVector.X;
                maxVec.Y = (maxVec.Y > ptVector.Y) ? maxVec.Y : ptVector.Y;
                maxVec.Z = (maxVec.Z > ptVector.Z) ? maxVec.Z : ptVector.Z;

                empty = false;
            }
            if (empty)
                throw new ArgumentException();

            return new BoundingBox(minVec, maxVec);
        }

        /// <summary>
        /// Create a bounding box that perfectly contains a given sphere.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to base the box upon.</param>
        /// <returns>A bounding box that encapsulates the given sphere</returns>
        public static BoundingBox CreateFromSphere(BoundingSphere sphere)
        {
            BoundingBox result;
            CreateFromSphere(ref sphere, out result);
            return result;
        }

        /// <summary>
        /// Create a bounding box given a sphere, putting resulting bounding box in the given <see cref="BoundingBox"/> parameter.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to base the box upon.</param>
        /// <param name="BoundingBox">The <see cref="BoundingBox"/> to put the result in.</param>
        public static void CreateFromSphere(ref BoundingSphere sphere, out BoundingBox result)
        {
            var corner = new Vector3(sphere.Radius);
            result.Min = sphere.Center - corner;
            result.Max = sphere.Center + corner;
        }

        /// <summary>
        /// Create a bounding box that contains two given bounding boxes.
        /// </summary>
        /// <param name="original">The first <see cref="BoundingBox"/>.</param>
        /// <param name="additional">The second <see cref="BoundingBox"/>.</param>
        /// <returns>A bounding box that contains both given boxes</returns>
        public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
        {
            BoundingBox result;
            CreateMerged(ref original, ref additional, out result);
            return result;
        }

        /// <summary>
        /// Create a bounding box that contains two given bounding boxes, putting the result in the result <see cref="BoundingBox"/> parameter.
        /// </summary>
        /// <param name="original">The first <see cref="BoundingBox"/>.</param>
        /// <param name="additional">The second <see cref="BoundingBox"/>.</param>
        /// <param name="result">The <see cref="BoundingBox"/> to store the result in.</param>
        public static void CreateMerged(ref BoundingBox original, ref BoundingBox additional, out BoundingBox result)
        {
            // For each dimension, set the min/max to the extreme of either boxes
            result.Min.X = Math.Min(original.Min.X, additional.Min.X);
            result.Min.Y = Math.Min(original.Min.Y, additional.Min.Y);
            result.Min.Z = Math.Min(original.Min.Z, additional.Min.Z);
            result.Max.X = Math.Max(original.Max.X, additional.Max.X);
            result.Max.Y = Math.Max(original.Max.Y, additional.Max.Y);
            result.Max.Z = Math.Max(original.Max.Z, additional.Max.Z);
        }

        /// <summary>
        /// Evaluates equality between two boxes
        /// </summary>
        /// <param name="original">The bounding box to compare to.</param>
        /// <returns>True is boxes are equal, false otherwise.</returns>
        public bool Equals(BoundingBox other)
        {
            return (this.Min == other.Min) && (this.Max == other.Max);
        }

        public override bool Equals(object obj)
        {
            return (obj is BoundingBox) ? this.Equals((BoundingBox)obj) : false;
        }

        /// <summary>
        /// Gets the box's corners.
        /// </summary>
        /// <returns>A 1-dimensional array of <see cref="Vector3"/>.</returns>
        public Vector3[] GetCorners()
        {
            return new Vector3[] {
                new Vector3(this.Min.X, this.Max.Y, this.Max.Z), 
                new Vector3(this.Max.X, this.Max.Y, this.Max.Z),
                new Vector3(this.Max.X, this.Min.Y, this.Max.Z), 
                new Vector3(this.Min.X, this.Min.Y, this.Max.Z), 
                new Vector3(this.Min.X, this.Max.Y, this.Min.Z),
                new Vector3(this.Max.X, this.Max.Y, this.Min.Z),
                new Vector3(this.Max.X, this.Min.Y, this.Min.Z),
                new Vector3(this.Min.X, this.Min.Y, this.Min.Z)
            };
        }

        /// <summary>
        /// Gets the box's corners, and stores them in the given array.
        /// </summary>
        /// <param name="corners">The array of 3-dimensional vectors (<see cref="Vector3"/>) to store the corners in</param>
        /// <exception cref="ArgumentNullException">Thrown if the input array was not instantiated.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the input array has less than 8 elements (Box has 8 corners)</exception>
        public void GetCorners(Vector3[] corners)
        {
            if (corners == null)
            {
                throw new ArgumentNullException("corners");
            }
            if (corners.Length < 8)
            {
                throw new ArgumentOutOfRangeException("corners", "Not Enought Corners");
            }
            corners[0].X = this.Min.X;
            corners[0].Y = this.Max.Y;
            corners[0].Z = this.Max.Z;
            corners[1].X = this.Max.X;
            corners[1].Y = this.Max.Y;
            corners[1].Z = this.Max.Z;
            corners[2].X = this.Max.X;
            corners[2].Y = this.Min.Y;
            corners[2].Z = this.Max.Z;
            corners[3].X = this.Min.X;
            corners[3].Y = this.Min.Y;
            corners[3].Z = this.Max.Z;
            corners[4].X = this.Min.X;
            corners[4].Y = this.Max.Y;
            corners[4].Z = this.Min.Z;
            corners[5].X = this.Max.X;
            corners[5].Y = this.Max.Y;
            corners[5].Z = this.Min.Z;
            corners[6].X = this.Max.X;
            corners[6].Y = this.Min.Y;
            corners[6].Z = this.Min.Z;
            corners[7].X = this.Min.X;
            corners[7].Y = this.Min.Y;
            corners[7].Z = this.Min.Z;
        }

        public override int GetHashCode()
        {
            return this.Min.GetHashCode() + this.Max.GetHashCode();
        }

        /// <summary>
        /// Determines if this <see cref="BoundingBox"> intersects with another.
        /// </summary>
        /// <param name="box">The box to test against</param>
        /// <returns>True if the boxes intersect, false otherwise</returns>
        public bool Intersects(BoundingBox box)
        {
            bool result;
            Intersects(ref box, out result);
            return result;
        }

        /// <summary>
        /// Determines if this <see cref="BoundingBox"> intersects with another, placing the result in the "result" parameter
        /// </summary>
        /// <param name="box">The box to test against</param>
        /// <param name="result">The boolean in which to place the result</param>
        public void Intersects(ref BoundingBox box, out bool result)
        {
            if ((this.Max.X >= box.Min.X) && (this.Min.X <= box.Max.X))
            {
                if ((this.Max.Y >= box.Min.Y) && (this.Min.Y <= box.Max.Y))
                {
                    result = (this.Max.Z >= box.Min.Z) && (this.Min.Z <= box.Max.Z);
                    return;
                }

                result = false;
                return;
            }
            
            result = false;
            return;
        }

        public bool Intersects(BoundingFrustum frustum)
        {
            return frustum.Intersects(this);
        }

        public bool Intersects(BoundingSphere sphere)
        {
            if (sphere.Center.X - Min.X > sphere.Radius
                && sphere.Center.Y - Min.Y > sphere.Radius
                && sphere.Center.Z - Min.Z > sphere.Radius
                && Max.X - sphere.Center.X > sphere.Radius
                && Max.Y - sphere.Center.Y > sphere.Radius
                && Max.Z - sphere.Center.Z > sphere.Radius)
                return true;

            double dmin = 0;

            if (sphere.Center.X - Min.X <= sphere.Radius)
                dmin += (sphere.Center.X - Min.X) * (sphere.Center.X - Min.X);
            else if (Max.X - sphere.Center.X <= sphere.Radius)
                dmin += (sphere.Center.X - Max.X) * (sphere.Center.X - Max.X);

            if (sphere.Center.Y - Min.Y <= sphere.Radius)
                dmin += (sphere.Center.Y - Min.Y) * (sphere.Center.Y - Min.Y);
            else if (Max.Y - sphere.Center.Y <= sphere.Radius)
                dmin += (sphere.Center.Y - Max.Y) * (sphere.Center.Y - Max.Y);

            if (sphere.Center.Z - Min.Z <= sphere.Radius)
                dmin += (sphere.Center.Z - Min.Z) * (sphere.Center.Z - Min.Z);
            else if (Max.Z - sphere.Center.Z <= sphere.Radius)
                dmin += (sphere.Center.Z - Max.Z) * (sphere.Center.Z - Max.Z);

            if (dmin <= sphere.Radius * sphere.Radius)
                return true;

            return false;
        }

        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            result = Intersects(sphere);
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            PlaneIntersectionType result;
            Intersects(ref plane, out result);
            return result;
        }

        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            // See http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

            Vector3 positiveVertex;
            Vector3 negativeVertex;

            if (plane.Normal.X >= 0)
            {
                positiveVertex.X = Max.X;
                negativeVertex.X = Min.X;
            }
            else
            {
                positiveVertex.X = Min.X;
                negativeVertex.X = Max.X;
            }

            if (plane.Normal.Y >= 0)
            {
                positiveVertex.Y = Max.Y;
                negativeVertex.Y = Min.Y;
            }
            else
            {
                positiveVertex.Y = Min.Y;
                negativeVertex.Y = Max.Y;
            }

            if (plane.Normal.Z >= 0)
            {
                positiveVertex.Z = Max.Z;
                negativeVertex.Z = Min.Z;
            }
            else
            {
                positiveVertex.Z = Min.Z;
                negativeVertex.Z = Max.Z;
            }

            // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
            var distance = plane.Normal.X * negativeVertex.X + plane.Normal.Y * negativeVertex.Y + plane.Normal.Z * negativeVertex.Z + plane.D;
            if (distance > 0)
            {
                result = PlaneIntersectionType.Front;
                return;
            }

            // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
            distance = plane.Normal.X * positiveVertex.X + plane.Normal.Y * positiveVertex.Y + plane.Normal.Z * positiveVertex.Z + plane.D;
            if (distance < 0)
            {
                result = PlaneIntersectionType.Back;
                return;
            }

            result = PlaneIntersectionType.Intersecting;
        }

        public Nullable<float> Intersects(Ray ray)
        {
            return ray.Intersects(this);
        }

        public void Intersects(ref Ray ray, out Nullable<float> result)
        {
            result = Intersects(ray);
        }

        public static bool operator ==(BoundingBox a, BoundingBox b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BoundingBox a, BoundingBox b)
        {
            return !a.Equals(b);
        }

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Min( ", this.Min.DebugDisplayString, " )  \r\n",
                    "Max( ",this.Max.DebugDisplayString, " )"
                    );
            }
        }

        public override string ToString()
        {
            return "{{Min:" + this.Min.ToString() + " Max:" + this.Max.ToString() + "}}";
        }

        #endregion Public Methods
    }
}
