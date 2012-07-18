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
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for managing a keyframe. A keyframe describes the position of an animation channel at a single point in time.
    /// </summary>
    public sealed class AnimationKeyframe : IComparable<AnimationKeyframe>
    {
        TimeSpan time;
        Matrix transform;

        /// <summary>
        /// Gets the time offset from the start of the animation to the position described by this keyframe.
        /// </summary>
        public TimeSpan Time
        {
            get
            {
                return time;
            }
        }

        /// <summary>
        /// Gets or sets the position described by this keyframe.
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return transform;
            }
            set
            {
                transform = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of AnimationKeyframe with the specified time offsetand transform.
        /// </summary>
        /// <param name="time">Time offset of the keyframe.</param>
        /// <param name="transform">Position of the keyframe.</param>
        public AnimationKeyframe(TimeSpan time, Matrix transform)
        {
            this.time = time;
            this.transform = transform;
        }

        /// <summary>
        /// Compares this instance of a keyframe to another.
        /// </summary>
        /// <param name="other">Keyframe being compared to.</param>
        /// <returns>Indication of their relative values.</returns>
        public int CompareTo(AnimationKeyframe other)
        {
            // No sense in comparing the transform, so compare the time.
            // This would be used for sorting keyframes in time order.
            return time.CompareTo(other.time);
        }
    }
}
