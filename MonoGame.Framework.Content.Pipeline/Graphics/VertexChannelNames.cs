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
            // Subtract the base name from the string and convert the remainder to an integer
            return Int32.Parse(encodedName.Substring(baseName.Length), CultureInfo.InvariantCulture);
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
