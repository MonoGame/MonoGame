// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
 [DataContract]
    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        #region Private Fields

        private CurveContinuity continuity;
        private float position;
        private float tangentIn;
        private float tangentOut;
        private float value;

        #endregion Private Fields


        #region Properties

        [DataMember]
        public CurveContinuity Continuity
        {
            get { return this.continuity; }
            set { this.continuity = value; }
        }

        [DataMember]
        public float Position
        {
            get { return this.position; }
        }

        [DataMember]
        public float TangentIn
        {
            get { return this.tangentIn; }
            set { this.tangentIn = value; }
        }

        [DataMember]
        public float TangentOut
        {
            get { return this.tangentOut; }
            set { this.tangentOut = value; }
        }

        [DataMember]
        public float Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #endregion


        #region Constructors

        public CurveKey(float position, float value)
            : this(position, value, 0, 0, CurveContinuity.Smooth)
        {

        }

        public CurveKey(float position, float value, float tangentIn, float tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
        {

        }

        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            this.position = position;
            this.value = value;
            this.tangentIn = tangentIn;
            this.tangentOut = tangentOut;
            this.continuity = continuity;
        }

        #endregion Constructors


        #region Public Methods

        public static bool operator !=(CurveKey a, CurveKey b)
        {
            return !(a == b);
        }

        public static bool operator ==(CurveKey a, CurveKey b)
        {
            if (object.Equals(a, null))
                return object.Equals(b, null);

            if (object.Equals(b, null))
                return object.Equals(a, null);

            return (a.position == b.position)
                && (a.value == b.value)
                && (a.tangentIn == b.tangentIn)
                && (a.tangentOut == b.tangentOut)
                && (a.continuity == b.continuity);
        }

        public CurveKey Clone()
        {
            return new CurveKey(this.position, this.value, this.tangentIn, this.tangentOut, this.continuity);
        }

        public int CompareTo(CurveKey other)
        {
            return this.position.CompareTo(other.position);
        }

        public bool Equals(CurveKey other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            return (obj is CurveKey) ? ((CurveKey)obj) == this : false;
        }

        public override int GetHashCode()
        {
            return this.position.GetHashCode() ^ this.value.GetHashCode() ^ this.tangentIn.GetHashCode() ^
                this.tangentOut.GetHashCode() ^ this.continuity.GetHashCode();
       } 

        #endregion
    }
}
