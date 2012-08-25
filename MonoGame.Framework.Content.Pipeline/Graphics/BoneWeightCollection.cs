#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

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

            List<BoneWeight> weights = new List<BoneWeight>(this.Items);

            // Sort into descending order
            weights.Sort((b1, b2) => { return b2.Weight.CompareTo(b1.Weight); });

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
