#region File Description
//-----------------------------------------------------------------------------
// StatisticsRange.cs
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
    /// A range of character statistics values.
    /// </summary>
    /// <remarks>Typically used for constrained random modifiers.</remarks>
#if WINDOWS
    [Serializable]
#endif
    public class StatisticsRange
    {
        [ContentSerializer(Optional = true)]
        public Int32Range HealthPointsRange;

        [ContentSerializer(Optional = true)]
        public Int32Range MagicPointsRange;

        [ContentSerializer(Optional = true)]
        public Int32Range PhysicalOffenseRange;

        [ContentSerializer(Optional = true)]
        public Int32Range PhysicalDefenseRange;

        [ContentSerializer(Optional = true)]
        public Int32Range MagicalOffenseRange;

        [ContentSerializer(Optional = true)]
        public Int32Range MagicalDefenseRange;


        #region Value Generation


        /// <summary>
        /// Generate a random value between the minimum and maximum, inclusively.
        /// </summary>
        /// <param name="random">The Random object used to generate the value.</param>
        public StatisticsValue GenerateValue(Random random)
        {
            // check the parameters
            Random usedRandom = random;
            if (usedRandom == null)
            {
                usedRandom = new Random();
            }

            // generate the new value
            StatisticsValue outputValue = new StatisticsValue();
            outputValue.HealthPoints = HealthPointsRange.GenerateValue(usedRandom);
            outputValue.MagicPoints = MagicPointsRange.GenerateValue(usedRandom);
            outputValue.PhysicalOffense = PhysicalOffenseRange.GenerateValue(usedRandom);
            outputValue.PhysicalDefense = PhysicalDefenseRange.GenerateValue(usedRandom);
            outputValue.MagicalOffense = MagicalOffenseRange.GenerateValue(usedRandom);
            outputValue.MagicalDefense = MagicalDefenseRange.GenerateValue(usedRandom);

            return outputValue;
        }


        #endregion


        #region String Output


        /// <summary>
        /// Builds a string that describes this object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("HP:");
            sb.Append(HealthPointsRange.ToString());

            sb.Append("; MP:");
            sb.Append(MagicPointsRange.ToString());

            sb.Append("; PO:");
            sb.Append(PhysicalOffenseRange.ToString());

            sb.Append("; PD:");
            sb.Append(PhysicalDefenseRange.ToString());

            sb.Append("; MO:");
            sb.Append(MagicalOffenseRange.ToString());

            sb.Append("; MD:");
            sb.Append(MagicalDefenseRange.ToString());

            return sb.ToString();
        }


        /// <summary>
        /// Builds a string that describes a modifier, where non-zero stats are skipped.
        /// </summary>
        public string GetModifierString()
        {
            StringBuilder sb = new StringBuilder();
            bool firstStatistic = true;

            // add the health points value, if any
            if ((HealthPointsRange.Minimum != 0) || (HealthPointsRange.Maximum != 0))
            {
                if (firstStatistic)
                {
                    firstStatistic = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append("HP:");
                sb.Append(HealthPointsRange.ToString());
            }

            // add the magic points value, if any
            if ((MagicPointsRange.Minimum != 0) || (MagicPointsRange.Maximum != 0))
            {
                if (firstStatistic)
                {
                    firstStatistic = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append("MP:");
                sb.Append(MagicPointsRange.ToString());
            }

            // add the physical offense value, if any
            if ((PhysicalOffenseRange.Minimum != 0) || 
                (PhysicalOffenseRange.Maximum != 0))
            {
                if (firstStatistic)
                {
                    firstStatistic = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append("PO:");
                sb.Append(PhysicalOffenseRange.ToString());
            }

            // add the physical defense value, if any
            if ((PhysicalDefenseRange.Minimum != 0) || 
                (PhysicalDefenseRange.Maximum != 0))
            {
                if (firstStatistic)
                {
                    firstStatistic = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append("PD:");
                sb.Append(PhysicalDefenseRange.ToString());
            }

            // add the magical offense value, if any
            if ((MagicalOffenseRange.Minimum != 0) || 
                (MagicalOffenseRange.Maximum != 0))
            {
                if (firstStatistic)
                {
                    firstStatistic = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append("MO:");
                sb.Append(MagicalOffenseRange.ToString());
            }

            // add the magical defense value, if any
            if ((MagicalDefenseRange.Minimum != 0) || 
                (MagicalDefenseRange.Maximum != 0))
            {
                if (firstStatistic)
                {
                    firstStatistic = false;
                }
                else
                {
                    sb.Append("; ");
                }
                sb.Append("MD:");
                sb.Append(MagicalDefenseRange.ToString());
            }

            return sb.ToString();
        }


        #endregion


        #region Operator: StatisticsRange + StatisticsValue


        /// <summary>
        /// Add one value to another, piecewise, and return the result.
        /// </summary>
        public static StatisticsRange Add(StatisticsRange value1,
            StatisticsValue value2)
        {
            StatisticsRange outputRange = new StatisticsRange();
            outputRange.HealthPointsRange = 
                value1.HealthPointsRange + value2.HealthPoints;
            outputRange.MagicPointsRange = 
                value1.MagicPointsRange + value2.MagicPoints;
            outputRange.PhysicalOffenseRange = 
                value1.PhysicalOffenseRange + value2.PhysicalOffense;
            outputRange.PhysicalDefenseRange = 
                value1.PhysicalDefenseRange + value2.PhysicalDefense;
            outputRange.MagicalOffenseRange = 
                value1.MagicalOffenseRange + value2.MagicalOffense;
            outputRange.MagicalDefenseRange =
                value1.MagicalDefenseRange + value2.MagicalDefense;
            return outputRange;
        }

        /// <summary>
        /// Add one value to another, piecewise, and return the result.
        /// </summary>
        public static StatisticsRange operator +(StatisticsRange value1,
            StatisticsValue value2)
        {
            return Add(value1, value2);
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Reads a StatisticsRange object from the content pipeline.
        /// </summary>
        public class StatisticsRangeReader : ContentTypeReader<StatisticsRange>
        {
            protected override StatisticsRange Read(ContentReader input, 
                StatisticsRange existingInstance)
            {
                StatisticsRange output = new StatisticsRange();

                output.HealthPointsRange = input.ReadObject<Int32Range>();
                output.MagicPointsRange = input.ReadObject<Int32Range>();
                output.PhysicalOffenseRange = input.ReadObject<Int32Range>();
                output.PhysicalDefenseRange = input.ReadObject<Int32Range>();
                output.MagicalOffenseRange = input.ReadObject<Int32Range>();
                output.MagicalDefenseRange = input.ReadObject<Int32Range>();

                return output;
            }
        }


        #endregion
    }
}
