// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Contains a collection of <see cref="CurveKey"/> points in 2D space and provides methods for evaluating features of the curve they define.
    /// </summary>
    // TODO : [TypeConverter(typeof(ExpandableObjectConverter))]
    [DataContract]
    public class Curve
    {
        #region Private Fields

        private CurveLoopType _preLoop;
        private CurveLoopType _postLoop;
        private CurveKeyCollection _keys;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns <c>true</c> if this curve is constant (has zero or one points); <c>false</c> otherwise.
        /// </summary>
        [DataMember]
        public bool IsConstant
        {
            get { return this._keys.Count <= 1; }
        }

        /// <summary>
        /// Defines how to handle weighting values that are less than the first control point in the curve.
        /// </summary>
        [DataMember]
        public CurveLoopType PreLoop
        {
            get { return this._preLoop; }
            set { this._preLoop = value; }
        }

        /// <summary>
        /// Defines how to handle weighting values that are greater than the last control point in the curve.
        /// </summary>
        [DataMember]
        public CurveLoopType PostLoop
        {
            get { return this._postLoop; }
            set { this._postLoop = value; }
        }

        /// <summary>
        /// The collection of curve keys.
        /// </summary>
        [DataMember]
        public CurveKeyCollection Keys
        {
            get { return this._keys; }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Constructs a curve.
        /// </summary>
        public Curve()
        {
            this._keys = new CurveKeyCollection();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a copy of this curve.
        /// </summary>
        /// <returns>A copy of this curve.</returns>
        public Curve Clone()
        {
            Curve curve = new Curve();

            curve._keys = this._keys.Clone();
            curve._preLoop = this._preLoop;
            curve._postLoop = this._postLoop;

            return curve;
        }

        /// <summary>
        /// Evaluate the value at a position of this <see cref="Curve"/>.
        /// </summary>
        /// <param name="position">The position on this <see cref="Curve"/>.</param>
        /// <returns>Value at the position on this <see cref="Curve"/>.</returns>
        public float Evaluate(float position)
        {
            if (_keys.Count == 0)
            {
            	return 0f;
            }
						
            if (_keys.Count == 1)
            {
            	return _keys[0].Value;
            }
			
            CurveKey first = _keys[0];
            CurveKey last = _keys[_keys.Count - 1];

            if (position < first.Position)
            {
                switch (this.PreLoop)
                {
                    case CurveLoopType.Constant:
                        //constant
                        return first.Value;

                    case CurveLoopType.Linear:
                        // linear y = a*x +b with a tangeant of last point
                        return first.Value - first.TangentIn * (first.Position - position);

                    case CurveLoopType.Cycle:
                        //start -> end / start -> end
                        int cycle = GetNumberOfCycle(position);
                        float virtualPos = position - (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);

                    case CurveLoopType.CycleOffset:
                        //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle * (last.Position - first.Position));
                        return (GetCurvePosition(virtualPos) + cycle * (last.Value - first.Value));

                    case CurveLoopType.Oscillate:
                        //go back on curve from end and target start 
                        // start-> end / end -> start
                        cycle = GetNumberOfCycle(position);
                        if (0 == cycle % 2f)//if pair
                            virtualPos = position - (cycle * (last.Position - first.Position));
                        else
                            virtualPos = last.Position - position + first.Position + (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);
                }
            }
            else if (position > last.Position)
            {
                int cycle;
                switch (this.PostLoop)
                {
                    case CurveLoopType.Constant:
                        //constant
                        return last.Value;

                    case CurveLoopType.Linear:
                        // linear y = a*x +b with a tangeant of last point
                        return last.Value + first.TangentOut * (position - last.Position);

                    case CurveLoopType.Cycle:
                        //start -> end / start -> end
                        cycle = GetNumberOfCycle(position);
                        float virtualPos = position - (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);

                    case CurveLoopType.CycleOffset:
                        //make the curve continue (with no step) so must up the curve each cycle of delta(value)
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle * (last.Position - first.Position));
                        return (GetCurvePosition(virtualPos) + cycle * (last.Value - first.Value));

                    case CurveLoopType.Oscillate:
                        //go back on curve from end and target start 
                        // start-> end / end -> start
                        cycle = GetNumberOfCycle(position);
                        virtualPos = position - (cycle * (last.Position - first.Position));
                        if (0 == cycle % 2f)//if pair
                            virtualPos = position - (cycle * (last.Position - first.Position));
                        else
                            virtualPos = last.Position - position + first.Position + (cycle * (last.Position - first.Position));
                        return GetCurvePosition(virtualPos);
                }
            }

            //in curve
            return GetCurvePosition(position);
        }

        /// <summary>
        /// Computes tangents for all keys in the collection.
        /// </summary>
        /// <param name="tangentType">The tangent type for both in and out.</param>
		public void ComputeTangents (CurveTangent tangentType)
		{
		    ComputeTangents(tangentType, tangentType);
		}
		
        /// <summary>
        /// Computes tangents for all keys in the collection.
        /// </summary>
        /// <param name="tangentInType">The tangent in-type. <see cref="CurveKey.TangentIn"/> for more details.</param>
        /// <param name="tangentOutType">The tangent out-type. <see cref="CurveKey.TangentOut"/> for more details.</param>
		public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
		{
            for (var i = 0; i < Keys.Count; ++i)
            {
                ComputeTangent(i, tangentInType, tangentOutType);
            }
		}

        /// <summary>
        /// Computes tangent for the specific key in the collection.
        /// </summary>
        /// <param name="keyIndex">The index of a key in the collection.</param>
        /// <param name="tangentType">The tangent type for both in and out.</param>
        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            ComputeTangent(keyIndex, tangentType, tangentType);
        }

        /// <summary>
        /// Computes tangent for the specific key in the collection.
        /// </summary>
        /// <param name="keyIndex">The index of key in the collection.</param>
        /// <param name="tangentInType">The tangent in-type. <see cref="CurveKey.TangentIn"/> for more details.</param>
        /// <param name="tangentOutType">The tangent out-type. <see cref="CurveKey.TangentOut"/> for more details.</param>
        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            // See http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.curvetangent.aspx

            var key = _keys[keyIndex];

            float p0, p, p1;
            p0 = p = p1 = key.Position;

            float v0, v, v1;
            v0 = v = v1 = key.Value;

            if ( keyIndex > 0 )
            {
                p0 = _keys[keyIndex - 1].Position;
                v0 = _keys[keyIndex - 1].Value;
            }

            if (keyIndex < _keys.Count-1)
            {
                p1 = _keys[keyIndex + 1].Position;
                v1 = _keys[keyIndex + 1].Value;
            }

            switch (tangentInType)
            {
                case CurveTangent.Flat:
                    key.TangentIn = 0;
                    break;
                case CurveTangent.Linear:
                    key.TangentIn = v - v0;
                    break;
                case CurveTangent.Smooth:
                    var pn = p1 - p0;
                    if (Math.Abs(pn) < float.Epsilon)
                        key.TangentIn = 0;
                    else
                        key.TangentIn = (v1 - v0) * ((p - p0) / pn);
                    break;
            }

            switch (tangentOutType)
            {
                case CurveTangent.Flat:
                    key.TangentOut = 0;
                    break;
                case CurveTangent.Linear:
                    key.TangentOut = v1 - v;
                    break;
                case CurveTangent.Smooth:
                    var pn = p1 - p0;
                    if (Math.Abs(pn) < float.Epsilon)
                        key.TangentOut = 0;
                    else
                        key.TangentOut = (v1 - v0) * ((p1 - p) / pn);
                    break;
            }
        }

	    #endregion

        #region Private Methods

        private int GetNumberOfCycle(float position)
        {
            float cycle = (position - _keys[0].Position) / (_keys[_keys.Count - 1].Position - _keys[0].Position);
            if (cycle < 0f)
                cycle--;
            return (int)cycle;
        }

        private float GetCurvePosition(float position)
        {
            //only for position in curve
            CurveKey prev = this._keys[0];
            CurveKey next;
            for (int i = 1; i < this._keys.Count; ++i)
            {
                next = this.Keys[i];
                if (next.Position >= position)
                {
                    if (prev.Continuity == CurveContinuity.Step)
                    {
                        if (position >= 1f)
                        {
                            return next.Value;
                        }
                        return prev.Value;
                    }
                    float t = (position - prev.Position) / (next.Position - prev.Position);//to have t in [0,1]
                    float ts = t * t;
                    float tss = ts * t;
                    //After a lot of search on internet I have found all about spline function
                    // and bezier (phi'sss ancien) but finaly use hermite curve 
                    //http://en.wikipedia.org/wiki/Cubic_Hermite_spline
                    //P(t) = (2*t^3 - 3t^2 + 1)*P0 + (t^3 - 2t^2 + t)m0 + (-2t^3 + 3t^2)P1 + (t^3-t^2)m1
                    //with P0.value = prev.value , m0 = prev.tangentOut, P1= next.value, m1 = next.TangentIn
                    return (2 * tss - 3 * ts + 1f) * prev.Value + (tss - 2 * ts + t) * prev.TangentOut + (3 * ts - 2 * tss) * next.Value + (tss - ts) * next.TangentIn;
                }
                prev = next;
            }
            return 0f;
        }

        #endregion
    }
}
