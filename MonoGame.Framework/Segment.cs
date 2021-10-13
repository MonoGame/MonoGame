using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    class Segment
    {
        #region Public Fields

        /// <summary>
        /// The direction of this <see cref="Ray"/>.
        /// </summary>
        [DataMember]
        public Vector3 Start;

        /// <summary>
        /// The origin of this <see cref="Ray"/>.
        /// </summary>
        [DataMember]
        public Vector3 End;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Create a <see cref="Segment"/>.
        /// </summary>
        /// <param name="start">The starting point of the <see cref="Segment"/>.</param>
        /// <param name="end">The ending point of the <see cref="Ray"/>.</param>
        public Segment(Vector3 start, Vector3 end)
        {
            this.Start = start;
            this.End = end;
        }

        #endregion

        #region Public Properties

        public float Distance { get { return (End - Start).Length(); } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if the specified <see cref="Object"/> is equal to this <see cref="Segment"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to test for equality with this <see cref="Segment"/>.</param>
        /// <returns>
        /// <code>true</code> if the specified <see cref="Object"/> is equal to this <see cref="Segment"/>,
        /// <code>false</code> if it is not.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is Segment) && this.Equals((Segment)obj);
        }

        /// <summary>
        /// Check if the specified <see cref="Segment"/> is equal to this <see cref="Segment"/>.
        /// </summary>
        /// <param name="other">The <see cref="Segment"/> to test for equality with this <see cref="Segment"/>.</param>
        /// <returns>
        /// <code>true</code> if the specified <see cref="Segment"/> is equal to this <see cref="Segment"/>,
        /// <code>false</code> if it is not.
        /// </returns>
        public bool Equals(Segment other)
        {
            return this.Start.Equals(other.Start) && this.End.Equals(other.End);
        }

        /// <summary>
        /// Get a hash code for this <see cref="Ray"/>.
        /// </summary>
        /// <returns>A hash code for this <see cref="Ray"/>.</returns>
        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        /// <summary>
        /// Check if this <see cref="Segment"/> intersects the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <returns>
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Segment"/> does not intersect the <see cref="BoundingBox"/>.
        /// </returns>
        public float? Intersects(BoundingBox box)
        {
            float distance = this.Distance;
            var dif = End - Start;
            dif.Normalize();
            float? d1 = new Ray(Start, dif).Intersects(box);
            if (d1.HasValue && d1 < distance) return d1;

            return null;
        }

        /// <summary>
        /// Check if this <see cref="Segment"/> intersects the specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="box">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <param name="result">
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Segment"/> does not intersect the <see cref="BoundingBox"/>.
        /// </param>
        public void Intersects(ref BoundingBox box, out float? result)
        {
            result = Intersects(box);
        }
        /// <summary>
        /// Check if this <see cref="Segment"/> intersects the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <returns>
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Segment"/> does not intersect the <see cref="BoundingSphere"/>.
        /// </returns>
        public float? Intersects(BoundingSphere sphere)
        {
            float distance = this.Distance;
            float? d1 = new Ray(Start, End - Start).Intersects(sphere);
            if (d1.HasValue && d1 < distance) return d1;

            return null;
        }

        /// <summary>
        /// Check if this <see cref="Segment"/> intersects the specified <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to test for intersection.</param>
        /// <returns>
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Segment"/> does not intersect the <see cref="Plane"/>.
        /// </returns>
        public float? Intersects(Plane plane)
        {
            float distance = this.Distance;

            float? d1 = new Ray(Start, (End - Start)/distance).Intersects(plane);
            if (d1.HasValue && d1 < distance) return d1;

            return null;
        }

        /// <summary>
        /// Check if this <see cref="Segment"/> intersects the specified <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane">The <see cref="Plane"/> to test for intersection.</param>
        /// <param name="result">
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Segment"/> does not intersect the <see cref="Plane"/>.
        /// </param>
        public void Intersects(ref Plane plane, out float? result)
        {
            result = Intersects(plane);
        }

        /// <summary>
        /// Check if this <see cref="Segment"/> intersects the specified <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingBox"/> to test for intersection.</param>
        /// <param name="result">
        /// The distance along the ray of the intersection or <code>null</code> if this
        /// <see cref="Segment"/> does not intersect the <see cref="BoundingSphere"/>.
        /// </param>
        public void Intersects(ref BoundingSphere sphere, out float? result)
        {
            result = Intersects(sphere);
        }


        /// <summary>
        /// Check if two segments are not equal.
        /// </summary>
        /// <param name="a">A ray to check for inequality.</param>
        /// <param name="b">A ray to check for inequality.</param>
        /// <returns><code>true</code> if the two rays are not equal, <code>false</code> if they are.</returns>
        public static bool operator != (Segment a, Segment b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Check if two segments are equal.
        /// </summary>
        /// <param name="a">A ray to check for equality.</param>
        /// <param name="b">A ray to check for equality.</param>
        /// <returns><code>true</code> if the two rays are equals, <code>false</code> if they are not.</returns>
        public static bool operator == (Segment a, Segment b)
        {
            return a.Equals(b);
        }

        internal string DebugDisplayString
        {
            get
            {
                return string.Concat(
                    "Start( ", this.Start.DebugDisplayString, " )  \r\n",
                    "End( ", this.End.DebugDisplayString, " )"
                );
            }
        }

        /// <summary>
        /// Get a <see cref="String"/> representation of this <see cref="Segment"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Segment"/>.</returns>
        public override string ToString()
        {
            return "{{Start:" + Start.ToString() + " End:" + End.ToString() + "}}";
        }

        /// <summary>
        /// Deconstruction method for <see cref="Segment"/>.
        /// </summary>
        /// <param name="start">Receives the start position of the segment.</param>
        /// <param name="end">Receives the start position of the segment.</param>
        public void Deconstruct(out Vector3 start, out Vector3 end)
        {
            start = Start;
            end = End;
        }
        #endregion
    }
}
