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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework
{
	// TODO [TypeConverter(ExpandableObjectConverter)]
#if WINRT
    [DataContract]
#else
    [Serializable]
#endif
    public class Curve
    {
        #region Private Fields

        private CurveKeyCollection keys;
        private CurveLoopType postLoop;
        private CurveLoopType preLoop;

        #endregion Private Fields


        #region Public Properties

        public bool IsConstant
        {
            get { return this.keys.Count <= 1; }
        }

        public CurveKeyCollection Keys
        {
            get { return this.keys; }
        }

        public CurveLoopType PostLoop
        {
            get { return this.postLoop; }
            set { this.postLoop = value; }
        }

        public CurveLoopType PreLoop
        {
            get { return this.preLoop; }
            set { this.preLoop = value; }
        }

        #endregion Public Properties


        #region Public Constructors

        public Curve()
        {
            this.keys = new CurveKeyCollection();
        }

        #endregion Public Constructors


        #region Public Methods

        public Curve Clone()
        {
            Curve curve = new Curve();

            curve.keys = this.keys.Clone();
            curve.preLoop = this.preLoop;
            curve.postLoop = this.postLoop;

            return curve;
        }

        public float Evaluate(float position)
        {
            CurveKey first = keys[0];
            CurveKey last = keys[keys.Count - 1];

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

		public void ComputeTangents (CurveTangent tangentType )
		{
		    ComputeTangents(tangentType, tangentType);
		}
		
		public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
		{
            for (var i = 0; i < Keys.Count; i++)
                ComputeTangent(i, tangentInType, tangentOutType);
		}

        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            ComputeTangent(keyIndex, tangentType, tangentType);
        }

        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            // See http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.curvetangent.aspx

            var key = keys[keyIndex];

            float p0, p, p1;
            p0 = p = p1 = key.Position;

            float v0, v, v1;
            v0 = v = v1 = key.Value;

            if ( keyIndex > 0 )
            {
                p0 = keys[keyIndex - 1].Position;
                v0 = keys[keyIndex - 1].Value;
            }

            if (keyIndex < keys.Count-1)
            {
                p1 = keys[keyIndex + 1].Position;
                v1 = keys[keyIndex + 1].Value;
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

	    #endregion Public Methods


        #region Private Methods

        private int GetNumberOfCycle(float position)
        {
            float cycle = (position - keys[0].Position) / (keys[keys.Count - 1].Position - keys[0].Position);
            if (cycle < 0f)
                cycle--;
            return (int)cycle;
        }

        private float GetCurvePosition(float position)
        {
            //only for position in curve
            CurveKey prev = this.keys[0];
            CurveKey next;
            for (int i = 1; i < this.keys.Count; i++)
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
