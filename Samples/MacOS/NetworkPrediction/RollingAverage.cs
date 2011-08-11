#region File Description
//-----------------------------------------------------------------------------
// RollingAverage.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace NetworkPrediction
{
    /// <summary>
    /// To compensate for network latency, we need to know exactly how late each
    /// packet is. Trouble is, there is no guarantee that the clock will be set the
    /// same on every machine! The sender can include packet data indicating what
    /// time their clock showed when they sent the packet, but this is meaningless
    /// unless our local clock is in sync with theirs. To compensate for any clock
    /// skew, we maintain a rolling average of the send times from the last 100
    /// incoming packets. If this average is, say, 50 milliseconds, but one specific
    /// packet arrives with a time difference of 70 milliseconds, we can deduce this
    /// particular packet was delivered 20 milliseconds later than usual.
    /// </summary>
    class RollingAverage
    {
        #region Fields

        // Array holding the N most recent sample values.
        float[] sampleValues;

        // Counter indicating how many of the sampleValues have been filled up.
        int sampleCount;

        // Cached sum of all the valid sampleValues.
        float valueSum;

        // Write position in the sampleValues array. When this reaches the end,
        // it wraps around, so we overwrite the oldest samples with newer data.
        int currentPosition;

        #endregion


        /// <summary>
        /// Constructs a new rolling average object that will track
        /// the specified number of sample values.
        /// </summary>
        public RollingAverage(int sampleCount)
        {
            sampleValues = new float[sampleCount];
        }


        /// <summary>
        /// Adds a new value to the rolling average, automatically
        /// replacing the oldest existing entry.
        /// </summary>
        public void AddValue(float newValue)
        {
            // To avoid having to recompute the sum from scratch every time
            // we add a new sample value, we just subtract out the value that
            // we are replacing, then add in the new value.
            valueSum -= sampleValues[currentPosition];
            valueSum += newValue;

            // Store the new sample value.
            sampleValues[currentPosition] = newValue;

            // Increment the write position.
            currentPosition++;

            // Track how many of the sampleValues elements are filled with valid data.
            if (currentPosition > sampleCount)
                sampleCount = currentPosition;

            // If we reached the end of the array, wrap back to the beginning.
            if (currentPosition >= sampleValues.Length)
            {
                currentPosition = 0;

                // The trick we used at the top of this method to update the sum
                // without having to recompute it from scratch works pretty well to
                // keep the average efficient, but over time, floating point rounding
                // errors could accumulate enough to cause problems. To prevent that,
                // we recalculate from scratch each time the counter wraps.
                valueSum = 0;

                foreach (float value in sampleValues)
                {
                    valueSum += value;
                }
            }
        }


        /// <summary>
        /// Gets the current value of the rolling average.
        /// </summary>
        public float AverageValue
        {
            get
            {
                if (sampleCount == 0)
                    return 0;
                
                return valueSum / sampleCount;
            }
        }
    }
}
