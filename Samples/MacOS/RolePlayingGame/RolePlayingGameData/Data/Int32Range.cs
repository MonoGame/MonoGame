#region File Description
//-----------------------------------------------------------------------------
// Int32Range.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Text;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// Defines a range of values, useful for generating values in that range.
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public struct Int32Range
    {
        /// <summary>
        /// The minimum value for the range, inclusively.
        /// </summary>
        public int Minimum;

        /// <summary>
        /// The maximum value for the range, inclusively.
        /// </summary>
        public int Maximum;


        /// <summary>
        /// Calculate the average value returned by this range.
        /// </summary>
        public Int32 Average
        {
            get { return Minimum + Range / 2; }
        }


        /// <summary>
        /// Calculate the size of this range.
        /// </summary>
        public Int32 Range
        {
            get { return (Maximum - Minimum); }
        }


        #region Initialization


        /// <summary>
        /// Construct a new Int32Range object, from the given minimum and maximums.
        /// </summary>
        public Int32Range(Int32 minimum, Int32 maximum)
        {
            // check the parameters
            if (maximum > minimum)
            {
                throw new ArgumentException(
                    "The minimum must be less than or equal to the maximum.");
            }

            // assign the parameters
            this.Minimum = minimum;
            this.Maximum = maximum;
        }


        #endregion


        #region Value Generation


        /// <summary>
        /// Generate a random value between the minimum and maximum, inclusively.
        /// </summary>
        /// <param name="random">The Random object used to generate the value.</param>
        public Int32 GenerateValue(Random random)
        {
            // check the parameters
            Random usedRandom = random;
            if (usedRandom == null)
            {
                usedRandom = new Random();
            }

            return usedRandom.Next(Minimum, Maximum);
        }


        #endregion


        #region Operator: Int32Range + Int32Range


        /// <summary>
        /// Add one range to another, piecewise, and return the result.
        /// </summary>
        public static Int32Range Add(Int32Range range1, Int32Range range2)
        {
            Int32Range outputRange;
            outputRange.Minimum = range1.Minimum + range2.Minimum;
            outputRange.Maximum = range1.Maximum + range2.Maximum;
            return outputRange;
        }

        /// <summary>
        /// Add one range to another, piecewise, and return the result.
        /// </summary>
        public static Int32Range operator +(Int32Range range1, Int32Range range2)
        {
            return Add(range1, range2);
        }


        #endregion


        #region Operator:  Int32Range + Int32


        /// <summary>
        /// Add an Int32 to both the minimum and maximum values of the range.
        /// </summary>
        public static Int32Range Add(Int32Range range, Int32 amount)
        {
            Int32Range outputRange = range;
            outputRange.Minimum += amount;
            outputRange.Maximum += amount;
            return outputRange;
        }

        /// <summary>
        /// Add an Int32 to both the minimum and maximum values of the range.
        /// </summary>
        public static Int32Range operator +(Int32Range range, Int32 amount)
        {
            return Add(range, amount);
        }


        #endregion


        #region Operator: Int32Range - Int32Range


        /// <summary>
        /// Subtract one range from another, piecewise, and return the result.
        /// </summary>
        public static Int32Range Subtract(Int32Range range1, Int32Range range2)
        {
            Int32Range outputRange;
            outputRange.Minimum = range1.Minimum - range2.Minimum;
            outputRange.Maximum = range1.Maximum - range2.Maximum;
            return outputRange;
        }

        /// <summary>
        /// Subtract one range from another, piecewise, and return the result.
        /// </summary>
        public static Int32Range operator -(Int32Range range1, Int32Range range2)
        {
            return Subtract(range1, range2);
        }


        #endregion


        #region Operator:  Int32Range - Int32


        /// <summary>
        /// Subtract an Int32 from both the minimum and maximum values of the range.
        /// </summary>
        public static Int32Range Subtract(Int32Range range, Int32 amount)
        {
            Int32Range outputRange = range;
            outputRange.Minimum -= amount;
            outputRange.Maximum -= amount;
            return outputRange;
        }

        /// <summary>
        /// Subtract an Int32 from both the minimum and maximum values of the range.
        /// </summary>
        public static Int32Range operator -(Int32Range range, Int32 amount)
        {
            return Subtract(range, amount);
        }


        #endregion


        // Compound assignment (+=, etc.) operators use the overloaded binary operators,
        // so there is no need in this case to override them explicitly


        #region String Output


        /// <summary>
        /// Builds a string that describes this object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(");

            sb.Append(Minimum);
            sb.Append(',');
            sb.Append(Maximum);
            sb.Append(')');

            return sb.ToString();
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read an Int32Range object from the content pipeline.
        /// </summary>
        public class Int32RangeReader : ContentTypeReader<Int32Range>
        {
            protected override Int32Range Read(ContentReader input, 
                Int32Range existingInstance)
            {
                Int32Range output = existingInstance;

                output.Minimum = input.ReadInt32();
                output.Maximum = input.ReadInt32();

                return output;
            }
        }


        #endregion
    }
}
