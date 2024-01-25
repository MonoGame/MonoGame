// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties for managing a collection of vertex channel names.
    /// </summary>
    public static class VertexChannelNames
    {
        /// <summary>
        /// A lookup for the TryDecodeUsage method.
        /// </summary>
        static Dictionary<string, VertexElementUsage> usages;

        static VertexChannelNames()
        {
            // Populate the lookup for TryDecodeUsage
            usages = new Dictionary<string, VertexElementUsage>();
            string[] names = Enum.GetNames(typeof(VertexElementUsage));
            Array values = Enum.GetValues(typeof(VertexElementUsage));
            for (int i = 0; i < names.Length; ++i)
                usages.Add(names[i], (VertexElementUsage)values.GetValue(i));
        }

        /// <summary>
        /// Gets the name of a binormal vector channel with the specified index.
        /// This will typically contain Vector3 data.
        /// </summary>
        /// <param name="usageIndex">Zero-based index of the vector channel being retrieved.</param>
        /// <returns>Name of the retrieved vector channel.</returns>
        public static string Binormal(int usageIndex)
        {
            return EncodeName(VertexElementUsage.Binormal, usageIndex);
        }

        /// <summary>
        /// Gets the name of a color channel with the specified index.
        /// This will typically contain Vector3 data.
        /// </summary>
        /// <param name="usageIndex">Zero-based index of the color channel being retrieved.</param>
        /// <returns>Name of the retrieved color channel.</returns>
        public static string Color(int usageIndex)
        {
            return EncodeName(VertexElementUsage.Color, usageIndex);
        }

        /// <summary>
        /// Gets a channel base name stub from the encoded string format.
        /// </summary>
        /// <param name="encodedName">Encoded string to be decoded.</param>
        /// <returns>Extracted base name.</returns>
        public static string DecodeBaseName(string encodedName)
        {
            if (string.IsNullOrEmpty(encodedName))
                throw new ArgumentNullException("encodedName");
            return encodedName.TrimEnd("0123456789".ToCharArray());
        }

        /// <summary>
        /// Gets a channel usage index from the encoded format.
        /// </summary>
        /// <param name="encodedName">Encoded name to be decoded.</param>
        /// <returns>Resulting channel usage index.</returns>
        public static int DecodeUsageIndex(string encodedName)
        {
            if (string.IsNullOrEmpty(encodedName))
                throw new ArgumentNullException("encodedName");
            // Extract the base name
            string baseName = DecodeBaseName(encodedName);
            if (string.IsNullOrEmpty(baseName))
                throw new InvalidOperationException("encodedName");

            // Subtract the base name from the string and convert the remainder to an integer.
            // TryParse solves the problem when name is just 'BlendIndicies' for example, in 
            // which case we default to index 0, assuming only 1 index.
            int index = 0;
            int.TryParse(encodedName.Substring(baseName.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out index);

            return index;
        }

        /// <summary>
        /// Combines a channel name stub and usage index into a string name.
        /// </summary>
        /// <param name="baseName">A channel base name stub.</param>
        /// <param name="usageIndex">A channel usage index.</param>
        /// <returns>Resulting encoded name.</returns>
        public static string EncodeName(string baseName, int usageIndex)
        {
            return baseName + usageIndex.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Combines a vertex declaration usage and usage index into a string name.
        /// </summary>
        /// <param name="vertexElementUsage">A vertex declaration.</param>
        /// <param name="usageIndex">An index for the vertex declaration.</param>
        /// <returns>Resulting encoded name.</returns>
        public static string EncodeName(VertexElementUsage vertexElementUsage, int usageIndex)
        {
            return vertexElementUsage.ToString() + usageIndex.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the name of the primary normal channel.
        /// This will typically contain Vector3 data.
        /// </summary>
        /// <returns>Primary normal channel name.</returns>
        public static string Normal()
        {
            return Normal(0);
        }

        /// <summary>
        /// Gets the name of a normal channel with the specified index.
        /// This will typically contain Vector3 data.
        /// </summary>
        /// <param name="usageIndex">Zero-based index of the normal channel being retrieved.</param>
        /// <returns>Normal channel at the specified index.</returns>
        public static string Normal(int usageIndex)
        {
            return EncodeName(VertexElementUsage.Normal, usageIndex);
        }

        /// <summary>
        /// Gets the name of a tangent vector channel with the specified index.
        /// This will typically contain Vector3 data.
        /// </summary>
        /// <param name="usageIndex">Zero-based index of the tangent vector channel being retrieved.</param>
        /// <returns>Name of the retrieved tangent vector channel.</returns>
        public static string Tangent(int usageIndex)
        {
            return EncodeName(VertexElementUsage.Tangent, usageIndex);
        }

        /// <summary>
        /// Gets the name of a texture coordinate channel with the specified index.
        /// This will typically contain Vector3 data.
        /// </summary>
        /// <param name="usageIndex">Zero-based index of the texture coordinate channel being retrieved.</param>
        /// <returns>Name of the retrieved texture coordinate channel.</returns>
        public static string TextureCoordinate(int usageIndex)
        {
            return EncodeName(VertexElementUsage.TextureCoordinate, usageIndex);
        }

        /// <summary>
        /// Gets a vertex declaration usage enumeration from the encoded string format.
        /// </summary>
        /// <param name="encodedName">Encoded name of a vertex declaration.</param>
        /// <param name="usage">Value of the declaration usage for the vertex declaration.</param>
        /// <returns>true if the encoded name maps to a VertexElementUsage enumeration value; false otherwise.</returns>
        public static bool TryDecodeUsage(string encodedName, out VertexElementUsage usage)
        {
            if (string.IsNullOrEmpty(encodedName))
                throw new ArgumentNullException("encodedName");
            // Extract the base name
            string baseName = DecodeBaseName(encodedName);
            if (string.IsNullOrEmpty(baseName))
                throw new InvalidOperationException("encodedName");
            return usages.TryGetValue(baseName, out usage);
        }

        /// <summary>
        /// Gets the name of the primary animation weights channel.
        /// This will typically contain data on the bone weights for a vertex channel. For more information, see BoneWeightCollection.
        /// </summary>
        /// <returns>Name of the primary animation weights channel.</returns>
        public static string Weights()
        {
            return Weights(0);
        }

        /// <summary>
        /// Gets the name of an animation weights channel at the specified index.
        /// This will typically contain data on the bone weights for a vertex channel. For more information, see BoneWeightCollection.
        /// </summary>
        /// <param name="usageIndex">Index of the animation weight channel to be retrieved.</param>
        /// <returns>Name of the retrieved animation weights channel.</returns>
        public static string Weights(int usageIndex)
        {
            // This appears to be the odd one out that doesn't use the VertexElementUsage enum.
            return EncodeName("Weights", usageIndex);
        }
    }
}
