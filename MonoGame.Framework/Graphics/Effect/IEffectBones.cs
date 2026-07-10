// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Interface for Effects that support bone transforms.
    /// </summary>
    public interface IEffectBones
    {
        /// <summary>
        /// Sets an array of skinning bone transform matrices.
        /// </summary>
        void SetBoneTransforms(Matrix[] boneTransforms);
    }
}

