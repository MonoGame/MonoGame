// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Contains an effect subclass which is used to load data for an EffectMaterialContent type.
    /// </summary>
    /// <remarks>
    /// For most purposes, this type can be ignored, and treated exactly like a regular effect.
    /// When an EffectMaterial type is loaded from .xnb format, its parameter values and textures
    /// are also loaded and automatically set on the effect, in addition to the HLSL shader code.
    /// Use this class to write a content pipeline extension to store materials inside a custom data type.
    /// </remarks>
	public class EffectMaterial : Effect
	{
        /// <summary>
        /// Creates a new instance of <see cref="EffectMaterial"/>.
        /// </summary>
        /// <param name="cloneSource">An instance of an object to copy initialization data from.</param>
		public EffectMaterial (Effect cloneSource) : base(cloneSource)
		{
		}
	}
}