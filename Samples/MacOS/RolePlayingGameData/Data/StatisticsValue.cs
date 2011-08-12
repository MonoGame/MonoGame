#region File Description
//-----------------------------------------------------------------------------
// StatisticsValue.cs
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
    /// The set of relevant statistics for characters.
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public struct StatisticsValue
    {
        [ContentSerializer(Optional = true)]
        public Int32 HealthPoints;

        [ContentSerializer(Optional = true)]
        public Int32 MagicPoints;

        [ContentSerializer(Optional = true)]
        public Int32 PhysicalOffense;

        [ContentSerializer(Optional = true)]
        public Int32 PhysicalDefense;

        [ContentSerializer(Optional = true)]
        public Int32 MagicalOffense;

        [ContentSerializer(Optional = true)]
        public Int32 MagicalDefense;


        /// <summary>
        /// Returns true if this object is trivial - all values at zero.
        /// </summary>
        public bool IsZero
        {
            get
            {
                return ((HealthPoints == 0) && (MagicPoints == 0) &&
                    (PhysicalOffense == 0) && (PhysicalDefense == 0) &&
                    (MagicalOffense == 0) && (MagicalDefense == 0));
            }
        }


        #region Initialization


        /// <summary>
        /// Create a new StatisticsValue object, fully specified by the parameters.
        /// </summary>
        public StatisticsValue(int healthPoints, int magicPoints, int physicalOffense,
            int physicalDefense, int magicalOffense, int magicalDefense)
        {
            HealthPoints = healthPoints;
            MagicPoints = magicPoints;
            PhysicalOffense = physicalOffense;
            PhysicalDefense = physicalDefense;
            MagicalOffense = magicalOffense;
            MagicalDefense = magicalDefense;
        }


        #endregion


        #region Operator: StatisticsValue + StatisticsValue


        /// <summary>
        /// Add one value to another, piecewise, and return the result.
        /// </summary>
        public static StatisticsValue Add(StatisticsValue value1, 
            StatisticsValue value2)
        {
            StatisticsValue outputValue = new StatisticsValue();
            outputValue.HealthPoints = 
                value1.HealthPoints + value2.HealthPoints;
            outputValue.MagicPoints = 
                value1.MagicPoints + value2.MagicPoints;
            outputValue.PhysicalOffense = 
                value1.PhysicalOffense + value2.PhysicalOffense;
            outputValue.PhysicalDefense = 
                value1.PhysicalDefense + value2.PhysicalDefense;
            outputValue.MagicalOffense = 
                value1.MagicalOffense + value2.MagicalOffense;
            outputValue.MagicalDefense = 
                value1.MagicalDefense + value2.MagicalDefense;
            return outputValue;
        }

        /// <summary>
        /// Add one value to another, piecewise, and return the result.
        /// </summary>
        public static StatisticsValue operator +(StatisticsValue value1, 
            StatisticsValue value2)
        {
            return Add(value1, value2);
        }


        #endregion


        #region Operator: StatisticsValue - StatisticsValue


        /// <summary>
        /// Subtract one value from another, piecewise, and return the result.
        /// </summary>
        public static StatisticsValue Subtract(StatisticsValue value1, 
            StatisticsValue value2)
        {
            StatisticsValue outputValue = new StatisticsValue();
            outputValue.HealthPoints =
                value1.HealthPoints - value2.HealthPoints;
            outputValue.MagicPoints =
                value1.MagicPoints - value2.MagicPoints;
            outputValue.PhysicalOffense =
                value1.PhysicalOffense - value2.PhysicalOffense;
            outputValue.PhysicalDefense =
                value1.PhysicalDefense - value2.PhysicalDefense;
            outputValue.MagicalOffense =
                value1.MagicalOffense - value2.MagicalOffense;
            outputValue.MagicalDefense =
                value1.MagicalDefense - value2.MagicalDefense;
            return outputValue;
        }

        /// <summary>
        /// Subtract one value from another, piecewise, and return the result.
        /// </summary>
        public static StatisticsValue operator -(StatisticsValue value1, 
            StatisticsValue value2)
        {
            return Subtract(value1, value2);
        }


        #endregion


        // Compound assignment (+=, etc.) operators use the overloaded binary operators,
        // so there is no need in this case to override them explicitly


        #region Limiting


        /// <summary>
        /// Clamp all values piecewise with the provided minimum values.
        /// </summary>
        public void ApplyMinimum(StatisticsValue minimumValue)
        {
            HealthPoints = Math.Max(HealthPoints, minimumValue.HealthPoints);
            MagicPoints = Math.Max(MagicPoints, minimumValue.MagicPoints);
            PhysicalOffense = Math.Max(PhysicalOffense, minimumValue.PhysicalOffense);
            PhysicalDefense = Math.Max(PhysicalDefense, minimumValue.PhysicalDefense);
            MagicalOffense = Math.Max(MagicalOffense, minimumValue.MagicalOffense);
            MagicalDefense = Math.Max(MagicalDefense, minimumValue.MagicalDefense);
        }


        /// <summary>
        /// Clamp all values piecewise with the provided maximum values.
        /// </summary>
        public void ApplyMaximum(StatisticsValue maximumValue)
        {
            HealthPoints = Math.Min(HealthPoints, maximumValue.HealthPoints);
            MagicPoints = Math.Min(MagicPoints, maximumValue.MagicPoints);
            PhysicalOffense = Math.Min(PhysicalOffense, maximumValue.PhysicalOffense);
            PhysicalDefense = Math.Min(PhysicalDefense, maximumValue.PhysicalDefense);
            MagicalOffense = Math.Min(MagicalOffense, maximumValue.MagicalOffense);
            MagicalDefense = Math.Min(MagicalDefense, maximumValue.MagicalDefense);
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
            sb.Append(HealthPoints.ToString());

            sb.Append("; MP:");
            sb.Append(MagicPoints.ToString());

            sb.Append("; PO:");
            sb.Append(PhysicalOffense.ToString());

            sb.Append("; PD:");
            sb.Append(PhysicalDefense.ToString());

            sb.Append("; MO:");
            sb.Append(MagicalOffense.ToString());

            sb.Append("; MD:");
            sb.Append(MagicalDefense.ToString());

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
            if (HealthPoints != 0)
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
                sb.Append(HealthPoints.ToString());
            }

            // add the magic points value, if any
            if (MagicPoints != 0)
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
                sb.Append(MagicPoints.ToString());
            }

            // add the physical offense value, if any
            if (PhysicalOffense != 0)
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
                sb.Append(PhysicalOffense.ToString());
            }

            // add the physical defense value, if any
            if (PhysicalDefense != 0)
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
                sb.Append(PhysicalDefense.ToString());
            }

            // add the magical offense value, if any
            if (MagicalOffense != 0)
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
                sb.Append(MagicalOffense.ToString());
            }

            // add the magical defense value, if any
            if (MagicalDefense != 0)
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
                sb.Append(MagicalDefense.ToString());
            } 
            
            return sb.ToString();
        }


        #endregion


        #region Content Type Reader


        public class StatisticsValueReader : ContentTypeReader<StatisticsValue>
        {
            protected override StatisticsValue Read(ContentReader input, 
                StatisticsValue existingInstance)
            {
                StatisticsValue output = new StatisticsValue();

                output.HealthPoints = input.ReadInt32();
                output.MagicPoints = input.ReadInt32();
                output.PhysicalOffense = input.ReadInt32();
                output.PhysicalDefense = input.ReadInt32();
                output.MagicalOffense = input.ReadInt32();
                output.MagicalDefense = input.ReadInt32();

                return output;
            }
        }


        #endregion
    }
}
