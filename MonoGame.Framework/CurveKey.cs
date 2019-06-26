// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Key point on the <see cref="Curve"/>.
    /// </summary>
    // TODO : [TypeConverter(typeof(ExpandableObjectConverter))]
    [DataContract]
    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        #region Private Fields

        private CurveContinuity _continuity;
        private readonly float _position;
        private float _tangentIn;
        private float _tangentOut;
        private float _value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete or continuous.
        /// </summary>
        [DataMember]
        public CurveContinuity Continuity
        {
            get { return this._continuity; }
            set { this._continuity = value; }
        }

        /// <summary>
        /// Gets a position of the key on the curve.
        /// </summary>
        [DataMember]
        public float Position
        {
            get { return this._position; }
        }

        /// <summary>
        /// Gets or sets a tangent when approaching this point from the previous point on the curve.
        /// </summary>
        [DataMember]
        public float TangentIn
        {
            get { return this._tangentIn; }
            set { this._tangentIn = value; }
        }

        /// <summary>
        /// Gets or sets a tangent when leaving this point to the next point on the curve.
        /// </summary>
        [DataMember]
        public float TangentOut
        {
            get { return this._tangentOut; }
            set { this._tangentOut = value; }
        }

        /// <summary>
        /// Gets a value of this point.
        /// </summary>
        [DataMember]
        public float Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class with position: 0 and value: 0.
        /// </summary>
        public CurveKey() : this(0, 0)
        {
            // This parameterless constructor is needed for correct serialization of CurveKeyCollection and CurveKey.
        }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        public CurveKey(float position, float value)
            : this(position, value, 0, 0, CurveContinuity.Smooth)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut)
            : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="CurveKey"/> class.
        /// </summary>
        /// <param name="position">Position on the curve.</param>
        /// <param name="value">Value of the control point.</param>
        /// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
        /// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
        /// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
        public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
        {
            this._position = position;
            this._value = value;
            this._tangentIn = tangentIn;
            this._tangentOut = tangentOut;
            this._continuity = continuity;
        }

        #endregion

        /// <summary>
        /// 
        /// Compares whether two <see cref="CurveKey"/> instances are not equal.
        /// </summary>
        /// <param name="value1"><see cref="CurveKey"/> instance on the left of the not equal sign.</param>
        /// <param name="value2"><see cref="CurveKey"/> instance on the right of the not equal sign.</param>
        /// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
        public static bool operator !=(CurveKey value1, CurveKey value2)
        {
            return !(value1 == value2);
        }

        /// <summary>
        /// Compares whether two <see cref="CurveKey"/> instances are equal.
        /// </summary>
        /// <param name="value1"><see cref="CurveKey"/> instance on the left of the equal sign.</param>
        /// <param name="value2"><see cref="CurveKey"/> instance on the right of the equal sign.</param>
        /// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(CurveKey value1, CurveKey value2)
        {
            if (object.Equals(value1, null))
                return object.Equals(value2, null);

            if (object.Equals(value2, null))
                return object.Equals(value1, null);

            return (value1._position == value2._position)
                && (value1._value == value2._value)
                && (value1._tangentIn == value2._tangentIn)
                && (value1._tangentOut == value2._tangentOut)
                && (value1._continuity == value2._continuity);
        }

        /// <summary>
        /// Creates a copy of this key.
        /// </summary>
        /// <returns>A copy of this key.</returns>
        public CurveKey Clone()
        {
            return new CurveKey(this._position, this._value, this._tangentIn, this._tangentOut, this._continuity);
        }

        #region Inherited Methods

        public int CompareTo(CurveKey other)
        {
            return this._position.CompareTo(other._position);
        }

        public bool Equals(CurveKey other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            return (obj as CurveKey) != null && Equals((CurveKey)obj);
        }

        public override int GetHashCode()
        {
            return this._position.GetHashCode() ^ this._value.GetHashCode() ^ this._tangentIn.GetHashCode() ^
                this._tangentOut.GetHashCode() ^ this._continuity.GetHashCode();
        } 

        #endregion
    }
}
