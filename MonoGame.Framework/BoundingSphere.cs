// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
 
    [DataContract]
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        #region Public Fields

        [DataMember]
        public Vector3 Center;

        [DataMember]
        public float Radius;

        #endregion Public Fields


        #region Constructors

        public BoundingSphere(Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        #endregion Constructors


        #region Public Methods

        public BoundingSphere Transform(Matrix matrix)
        {
            BoundingSphere sphere = new BoundingSphere();
            sphere.Center = Vector3.Transform(this.Center, matrix);
            sphere.Radius = this.Radius * ((float)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
            return sphere;
        }

        public void Transform(ref Matrix matrix, out BoundingSphere result)
        {
            result.Center = Vector3.Transform(this.Center, matrix);
            result.Radius = this.Radius * ((float)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
        }

        public ContainmentType Contains(BoundingBox box)
        {
            //check if all corner is in sphere
            bool inside = true;
            foreach (Vector3 corner in box.GetCorners())
            {
                if (this.Contains(corner) == ContainmentType.Disjoint)
                {
                    inside = false;
                    break;
                }
            }

            if (inside)
                return ContainmentType.Contains;

            //check if the distance from sphere center to cube face < radius
            double dmin = 0;

            if (Center.X < box.Min.X)
				dmin += (Center.X - box.Min.X) * (Center.X - box.Min.X);

			else if (Center.X > box.Max.X)
					dmin += (Center.X - box.Max.X) * (Center.X - box.Max.X);

			if (Center.Y < box.Min.Y)
				dmin += (Center.Y - box.Min.Y) * (Center.Y - box.Min.Y);

			else if (Center.Y > box.Max.Y)
				dmin += (Center.Y - box.Max.Y) * (Center.Y - box.Max.Y);

			if (Center.Z < box.Min.Z)
				dmin += (Center.Z - box.Min.Z) * (Center.Z - box.Min.Z);

			else if (Center.Z > box.Max.Z)
				dmin += (Center.Z - box.Max.Z) * (Center.Z - box.Max.Z);

			if (dmin <= Radius * Radius) 
				return ContainmentType.Intersects;
            
            //else disjoint
            return ContainmentType.Disjoint;

        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = this.Contains(box);
        }

        public ContainmentType Contains(BoundingFrustum frustum)
        {
            //check if all corner is in sphere
            bool inside = true;

            Vector3[] corners = frustum.GetCorners();
            foreach (Vector3 corner in corners)
            {
                if (this.Contains(corner) == ContainmentType.Disjoint)
                {
                    inside = false;
                    break;
                }
            }
            if (inside)
                return ContainmentType.Contains;

            //check if the distance from sphere center to frustrum face < radius
            double dmin = 0;
            //TODO : calcul dmin

            if (dmin <= Radius * Radius)
                return ContainmentType.Intersects;

            //else disjoint
            return ContainmentType.Disjoint;
        }

        public ContainmentType Contains(BoundingSphere sphere)
        {
            float val = Vector3.Distance(sphere.Center, Center);

            if (val > sphere.Radius + Radius)
                return ContainmentType.Disjoint;

            else if (val <= Radius - sphere.Radius)
                return ContainmentType.Contains;

            else
                return ContainmentType.Intersects;
        }

        public void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            result = Contains(sphere);
        }

        public ContainmentType Contains(Vector3 point)
        {
            float distance = Vector3.Distance(point, Center);

            if (distance > this.Radius)
                return ContainmentType.Disjoint;

            else if (distance < this.Radius)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        public void Contains(ref Vector3 point, out ContainmentType result)
        {
            result = Contains(point);
        }

        public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
        {
            // Find the center of the box.
            Vector3 center = new Vector3((box.Min.X + box.Max.X) / 2.0f,
                                         (box.Min.Y + box.Max.Y) / 2.0f,
                                         (box.Min.Z + box.Max.Z) / 2.0f);

            // Find the distance between the center and one of the corners of the box.
            float radius = Vector3.Distance(center, box.Max);

            return new BoundingSphere(center, radius);
        }

        public static void CreateFromBoundingBox(ref BoundingBox box, out BoundingSphere result)
        {
            result = CreateFromBoundingBox(box);
        }

        public static BoundingSphere CreateFromFrustum(BoundingFrustum frustum)
        {
            return BoundingSphere.CreateFromPoints(frustum.GetCorners());
        }

        public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
        {
            if (points == null )
                throw new ArgumentNullException("points");

            // From "Real-Time Collision Detection" (Page 89)

            var minx = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var maxx = -minx;
            var miny = minx;
            var maxy = -minx;
            var minz = minx;
            var maxz = -minx;

            // Find the most extreme points along the principle axis.
            var numPoints = 0;           
            foreach (var pt in points)
            {
                ++numPoints;

                if (pt.X < minx.X) 
                    minx = pt;
                if (pt.X > maxx.X) 
                    maxx = pt;
                if (pt.Y < miny.Y) 
                    miny = pt;
                if (pt.Y > maxy.Y) 
                    maxy = pt;
                if (pt.Z < minz.Z) 
                    minz = pt;
                if (pt.Z > maxz.Z) 
                    maxz = pt;
            }

            if (numPoints == 0)
                throw new ArgumentException("You should have at least one point in points.");

            var sqDistX = Vector3.DistanceSquared(maxx, minx);
            var sqDistY = Vector3.DistanceSquared(maxy, miny);
            var sqDistZ = Vector3.DistanceSquared(maxz, minz);

            // Pick the pair of most distant points.
            var min = minx;
            var max = maxx;
            if (sqDistY > sqDistX && sqDistY > sqDistZ) 
            {
                max = maxy;
                min = miny;
            }
            if (sqDistZ > sqDistX && sqDistZ > sqDistY) 
            {
                max = maxz;
                min = minz;
            }
            
            var center = (min + max) * 0.5f;
            var radius = Vector3.Distance(max, center);
            return new BoundingSphere(center, radius);
        }

        public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
        {
            Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
            float distance = ocenterToaCenter.Length();
            if (distance <= original.Radius + additional.Radius)//intersect
            {
                if (distance <= original.Radius - additional.Radius)//original contain additional
                    return original;
                if (distance <= additional.Radius - original.Radius)//additional contain original
                    return additional;
            }

            //else find center of new sphere and radius
            float leftRadius = Math.Max(original.Radius - distance, additional.Radius);
            float Rightradius = Math.Max(original.Radius + distance, additional.Radius);
            ocenterToaCenter = ocenterToaCenter + (((leftRadius - Rightradius) / (2 * ocenterToaCenter.Length())) * ocenterToaCenter);//oCenterToResultCenter
            
            BoundingSphere result = new BoundingSphere();
            result.Center = original.Center + ocenterToaCenter;
            result.Radius = (leftRadius + Rightradius) / 2;
            return result;
        }

        public static void CreateMerged(ref BoundingSphere original, ref BoundingSphere additional, out BoundingSphere result)
        {
            result = BoundingSphere.CreateMerged(original, additional);
        }

        public bool Equals(BoundingSphere other)
        {
            return this.Center == other.Center && this.Radius == other.Radius;
        }

        public override bool Equals(object obj)
        {
            if (obj is BoundingSphere)
                return this.Equals((BoundingSphere)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return this.Center.GetHashCode() + this.Radius.GetHashCode();
        }

        public bool Intersects(BoundingBox box)
        {
			return box.Intersects(this);
        }

        public void Intersects(ref BoundingBox box, out bool result)
        {
			result = Intersects(box);
        }

        /*
        public bool Intersects(BoundingFrustum frustum)
        {
            if (frustum == null)
                throw new NullReferenceException();

            throw new NotImplementedException();
        }
        */

        public bool Intersects(BoundingSphere sphere)
        {
            float val = Vector3.Distance(sphere.Center, Center);
			if (val > sphere.Radius + Radius)
				return false;
			return true;
        }

        public void Intersects(ref BoundingSphere sphere, out bool result)
        {
			result = Intersects(sphere);
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            var result = default(PlaneIntersectionType);
            // TODO: we might want to inline this for performance reasons
            this.Intersects(ref plane, out result);
            return result;
        }

        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            var distance = default(float);
            // TODO: we might want to inline this for performance reasons
            Vector3.Dot(ref plane.Normal, ref this.Center, out distance);
            distance += plane.D;
            if (distance > this.Radius)
                result = PlaneIntersectionType.Front;
            else if (distance < -this.Radius)
                result = PlaneIntersectionType.Back;
            else
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

        public static bool operator == (BoundingSphere a, BoundingSphere b)
        {
            return a.Equals(b);
        }

        public static bool operator != (BoundingSphere a, BoundingSphere b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Center:{0} Radius:{1}}}", this.Center.ToString(), this.Radius.ToString());
        }

        #endregion Public Methods
    }
}
