// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents an axis aligned bounding box.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{DebugDisplayString,nq}")]
    public struct BoundingBox : IEquatable<BoundingBox>
    {

        #region Public Fields

        /// <summary>
        /// Minimal edge of the box.
        /// </summary>
        [DataMember]
        public Vector3 Min;
      
        /// <summary>
        /// Maximum edge of the box.
        /// </summary>
        [DataMember]
        public Vector3 Max;

        /// <summary>
        /// Count of Corners, which will always be 8.
        /// </summary>
        public const int CornerCount = 8;

        #endregion Public Fields


        #region Public Constructors

        /// <summary>
        /// Creates an axis aligned bounding box defining minimum and maximum edge.
        /// </summary>
        /// <param name="min">Minimum edge.</param>
        /// <param name="max">Maximum edge.</param>
        public BoundingBox(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Checks for containing one bounding box in another.
        /// </summary>
        /// <param name="box">Box to test with.</param>
        /// <returns>Containment type.</returns>
        public ContainmentType Contains(BoundingBox box)
        {
            //test if all corner is in the same side of a face by just checking min and max
            if (box.Max.X < Min.X
                || box.Min.X > Max.X
                || box.Max.Y < Min.Y
                || box.Min.Y > Max.Y
                || box.Max.Z < Min.Z
                || box.Min.Z > Max.Z)
                return ContainmentType.Disjoint;


            if (box.Min.X >= Min.X
                && box.Max.X <= Max.X
                && box.Min.Y >= Min.Y
                && box.Max.Y <= Max.Y
                && box.Min.Z >= Min.Z
                && box.Max.Z <= Max.Z)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        /// <summary>
        /// Checks for containing one bounding box in another.
        /// </summary>
        /// <param name="box">Box to test with.</param>
        /// <param name="result">Containment type.</param>
        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = Contains(box);
        }

        /// <summary>
        /// Checks for containment in a bounding frustum.
        /// </summary>
        /// <param name="frustum">Bounding frustum to test with.</param>
        /// <returns>Containment type.</returns>
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
        /// Checks for containing a bounding sphere.
        /// </summary>
        /// <param name="sphere">Bounding sphere to test with.</param>
        /// <returns>Containment type.</returns>
        public ContainmentType Contains(BoundingSphere sphere)
        {
            if (sphere.Center.X - Min.X >= sphere.Radius
                && sphere.Center.Y - Min.Y >= sphere.Radius
                && sphere.Center.Z - Min.Z >= sphere.Radius
                && Max.X - sphere.Center.X >= sphere.Radius
                && Max.Y - sphere.Center.Y >= sphere.Radius
                && Max.Z - sphere.Center.Z >= sphere.Radius)
                return ContainmentType.Contains;

            double dmin = 0;

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
        /// Checks for containing a bounding sphere.
        /// </summary>
        /// <param name="sphere">Bounding sphere to test with.</param>
        /// <param name="result">Containment type.</param>
        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            result = this.Contains(sphere);
        }

        /// <summary>
        /// Checks for containing a Point.
        /// </summary>
        /// <param name="point">Point to test with.</param>
        /// <returns>Containment type.</returns>
        public ContainmentType Contains(Vector3 point)
        {
            ContainmentType result;
            this.Contains(ref point, out result);
            return result;
        }

        /// <summary>
        /// Checks for containing a Point.
        /// </summary>
        /// <param name="point">Point to test with.</param>
        /// <param name="result">Containment type.</param>
        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            //first we get if point is out of box
            if (point.X < this.Min.X
                || point.X > this.Max.X
                || point.Y < this.Min.Y
                || point.Y > this.Max.Y
                || point.Z < this.Min.Z
                || point.Z > this.Max.Z)
            {
                result = ContainmentType.Disjoint;
            }//or if point is on box because coordonate of point is lesser or equal
            else if (point.X == this.Min.X
                || point.X == this.Max.X
                || point.Y == this.Min.Y
                || point.Y == this.Max.Y
                || point.Z == this.Min.Z
                || point.Z == this.Max.Z)
                result = ContainmentType.Intersects;
            else
                result = ContainmentType.Contains;
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
        /// Creates a so called smallest enclosing box from a sphere.
        /// </summary>
        /// <param name="sphere">Sphere to enclose.</param>
        /// <returns>Enclosing bounding box.</returns>
        public static BoundingBox CreateFromSphere(BoundingSphere sphere)
        {
            BoundingBox result;
            CreateFromSphere(ref sphere, out result);
            return result;
        }

        /// <summary>
        /// Creates a so called smallest enclosing box from a sphere.
        /// </summary>
        /// <param name="sphere">Sphere to enclose.</param>
        /// <param name="result">Enclosing bounding box.</param>
        public static void CreateFromSphere(ref BoundingSphere sphere, out BoundingBox result)
        {
            var corner = new Vector3(sphere.Radius);
            result.Min = sphere.Center - corner;
            result.Max = sphere.Center + corner;
        }

        /// <summary>
        /// Merges two bounding boxes to a combined one.
        /// </summary>
        /// <param name="original">First bounding box.</param>
        /// <param name="additional">Second bounding box.</param>
        /// <returns>Merged bounding box.</returns>
        public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
        {
            BoundingBox result;
            CreateMerged(ref original, ref additional, out result);
            return result;
        }

        /// <summary>
        /// Merges two bounding boxes to a combined one.
        /// </summary>
        /// <param name="original">First bounding box.</param>
        /// <param name="additional">Second bounding box.</param>
        /// <param name="result">Merged bounding box.</param>
        public static void CreateMerged(ref BoundingBox original, ref BoundingBox additional, out BoundingBox result)
        {
            result.Min.X = Math.Min(original.Min.X, additional.Min.X);
            result.Min.Y = Math.Min(original.Min.Y, additional.Min.Y);
            result.Min.Z = Math.Min(original.Min.Z, additional.Min.Z);
            result.Max.X = Math.Max(original.Max.X, additional.Max.X);
            result.Max.Y = Math.Max(original.Max.Y, additional.Max.Y);
            result.Max.Z = Math.Max(original.Max.Z, additional.Max.Z);
        }

        /// <summary>
        /// Check for data equality of the bounding box to another.
        /// </summary>
        /// <param name="other">Bounding box comparing to.</param>
        /// <returns>Equality.</returns>
        public bool Equals(BoundingBox other)
        {
            return (this.Min == other.Min) && (this.Max == other.Max);
        }

        /// <summary>
        /// Object-equality falling back to data equality when <see cref="obj"/> is a bounding box as well.
        /// </summary>
        /// <param name="obj">Comparing object.</param>
        /// <returns>Equality</returns>
        public override bool Equals(object obj)
        {
            return (obj is BoundingBox) ? this.Equals((BoundingBox)obj) : false;
        }

        /// <summary>
        /// Gets all 8 Corners of the Bounding box as an array.
        /// </summary>
        /// <returns>All 8 Corners.</returns>
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
        /// Gets all 8 Corners of the Bounding box as an array.
        /// </summary>
        /// <param name="corners">All 8 Corners.</param>
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

        /// <summary>
        /// Calculates the Hashcode of the bounding box.
        /// </summary>
        /// <returns>Hashcode.</returns>
        public override int GetHashCode()
        {
            return this.Min.GetHashCode() + this.Max.GetHashCode();
        }

        /// <summary>
        /// Checks for intersection to another bounding box.
        /// </summary>
        /// <param name="box">Bounding box to test with.</param>
        /// <returns>If they intersects.</returns>
        public bool Intersects(BoundingBox box)
        {
            bool result;
            Intersects(ref box, out result);
            return result;
        }

        /// <summary>
        /// Checks for intersection to another bounding box.
        /// </summary>
        /// <param name="box">Bounding box to test with.</param>
        /// <param name="result">If they intersects.</param>
        public void Intersects(ref BoundingBox box, out bool result)
        {
            if ((this.Max.X >= box.Min.X) && (this.Min.X <= box.Max.X))
            {
                if ((this.Max.Y < box.Min.Y) || (this.Min.Y > box.Max.Y))
                {
                    result = false;
                    return;
                }

                result = (this.Max.Z >= box.Min.Z) && (this.Min.Z <= box.Max.Z);
                return;
            }

            result = false;
            return;
        }

        /// <summary>
        /// Checks for intersection to another bounding box and returning information about separtion.
        /// </summary>
        /// <param name="other">Bounding box to teset with.</param>
        /// <param name="result">Result containing intersection result.</param>
        public void IntersectsWithSat(BoundingBox other, out SatIntersectionResult result)
        {
            //as we have axis aligned colliders, there are just the three main axis to test.
            var axis = new[]
            {
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ
            };

            var shortestAxis = Vector3.Zero;
            var shortestOverlap = float.MaxValue;

            //go through all axis and get the overlapping size on the projected one-dimensional line.
            for (var i = 0; i < axis.Length; i++)
            {
                var currentAxis = axis[i];
                var p1 = GetProjectedPoints(this, currentAxis);
                var p2 = GetProjectedPoints(other, currentAxis);

                var overlapValue = p1.OverlapSize(p2);

                //if there is overlap, save axis and distance if the overlap is smaller than previous one.
                if (overlapValue > 0.0f)
                {
                    if (overlapValue < shortestOverlap)
                    {
                        shortestOverlap = overlapValue;
                        shortestAxis = p1.Min < p2.Min ? currentAxis : -currentAxis;
                    }
                }
                else
                {
                    //if on any axis is no overlap, then the collision shapes don't collide.
                    result = SatIntersectionResult.Empty;
                    return;
                }
            }

            // if all axis collide (overlap > 0) then the separting distance is the smallest overlap with
            result = new SatIntersectionResult
            {
                Intersects = true,
                Separtion = shortestAxis * shortestOverlap
            };
        }

        /// <summary>
        /// Checks for intersection to another bounding box and returning information about separtion.
        /// </summary>
        /// <param name="other">Bounding box to teset with.</param>
        /// <returns>Result containing intersection result.</returns>
        public SatIntersectionResult IntersectsWithSat(BoundingBox other)
        {
            SatIntersectionResult result;
            IntersectsWithSat(other, out result);
            return result;
        }
        
        /// <summary>
        /// Checks for intersection to a bounding frustum.
        /// </summary>
        /// <param name="frustum">Bounding frustum to check with.</param>
        /// <returns>If they intersect.</returns>
        public bool Intersects(BoundingFrustum frustum)
        {
            return frustum.Intersects(this);
        }

        /// <summary>
        /// Checks for intersection to a bounding sphere.
        /// </summary>
        /// <param name="sphere">Bounding sphere to check with.</param>
        /// <returns>If they intersect.</returns>
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

        /// <summary>
        /// Checks for intersection to a bounding sphere.
        /// </summary>
        /// <param name="sphere">Bounding sphere to check with.</param>
        /// <param name="result">If they intersect.</param>
        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
            result = Intersects(sphere);
        }

        /// <summary>
        /// Checks for an intersection to a plane.
        /// </summary>
        /// <param name="plane">Plane to check with.</param>
        /// <returns>Intersection type.</returns>
        public PlaneIntersectionType Intersects(Plane plane)
        {
            PlaneIntersectionType result;
            Intersects(ref plane, out result);
            return result;
        }

        /// <summary>
        /// Checks for an intersection to a plane.
        /// </summary>
        /// <param name="plane">Plane to check with.</param>
        /// <param name="result">Intersection type.</param>
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

        /// <summary>
        /// Checks for an intersection to a ray.
        /// </summary>
        /// <param name="ray">Plane to check with.</param>
        /// <returns>Null when no intersection, otherwise returning float.</returns>
        public Nullable<float> Intersects(Ray ray)
        {
            return ray.Intersects(this);
        }

        /// <summary>
        /// Checks for an intersection to a ray.
        /// </summary>
        /// <param name="ray">Plane to check with.</param>
        /// <param name="result">Null when no intersection, otherwise returning float.</param>
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

        #region Private Methods

        private static Line1D GetProjectedPoints(BoundingBox boundingBox, Vector3 axis)
        {
            var points = boundingBox.GetCorners();
            var axisNormalized = Vector3.Normalize(axis);

            var min = Vector3.Dot(axisNormalized, points[0]);
            var max = min;

            for (var i = 1; i < points.Length; i++)
            {
                var projectedPoint = Vector3.Dot(axisNormalized, points[i]);
                if (projectedPoint < min)
                {
                    min = projectedPoint;
                }
                else if (projectedPoint > max)
                {
                    max = projectedPoint;
                }
            }

            return new Line1D(min, max);
        }

        #endregion
    }
}
