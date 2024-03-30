// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Collection of bone weights of a vertex.
    /// </summary>
    public sealed class BoneWeightCollection : Collection<BoneWeight>
    {
        /// <summary>
        /// Initializes a new instance of BoneWeightCollection.
        /// </summary>
        public BoneWeightCollection()
        {
        }

        /// <summary>
        /// Normalizes the contents of the weights list.
        /// </summary>
        public void NormalizeWeights()
        {
            // Normalization does the following:
            //
            // - Sorts weights such that the most significant weight is first.
            // - Removes zero-value entries.
            // - Adjusts values so the sum equals one.
            //
            // Throws InvalidContentException if all weights are zero.
            NormalizeWeights(int.MaxValue);
        }

        /// <summary>
        /// Normalizes the contents of the bone weights list.
        /// </summary>
        /// <param name="maxWeights">Maximum number of weights allowed.</param>
        public void NormalizeWeights(int maxWeights)
        {
            // Normalization does the following:
            //
            // - Sorts weights such that the most significant weight is first.
            // - Removes zero-value entries.
            // - Discards weights with the smallest value until there are maxWeights or less in the list.
            // - Adjusts values so the sum equals one.
            //
            // Throws InvalidContentException if all weights are zero.

            var weights = (List<BoneWeight>)Items;

            // Sort into descending order
            weights.Sort((b1, b2) => b2.Weight.CompareTo(b1.Weight));

            // Find the sum to validate we have weights and to normalize the weights
            float sum = 0.0f;
            int index = 0;
            // Cannot use a foreach or for because the index may not always increment and the length of the list may change.
            while (index < weights.Count)
            {
                float weight = weights[index].Weight;
                if ((weight > 0.0f) && (index < maxWeights))
                {
                    sum += weight;
                    ++index;
                }
                else
                {
                    // Discard any zero weights or if we have exceeded the maximum number of weights
                    weights.RemoveAt(index);
                }
            }

            if (sum == 0.0f)
                throw new InvalidContentException("Total bone weights in a collection must not be zero");

            // Normalize each weight
            int count = weights.Count();
            // Old-school trick. Multiplication is faster than division, so multiply by the inverse.
            float invSum = 1.0f / sum;
            for (index = 0; index < count; ++index)
            {
                BoneWeight bw = weights[index];
                bw.Weight *= invSum;
                weights[index] = bw;
            }
        }
    }
}
