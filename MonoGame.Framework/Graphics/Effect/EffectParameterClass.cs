// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines classes for effect parameters and shader constants.
    /// </summary>
	public enum EffectParameterClass
	{
        /// <summary>
        /// Scalar class type.
        /// </summary>
		Scalar,
        /// <summary>
        /// Vector class type.
        /// </summary>
		Vector,
        /// <summary>
        /// Matrix class type.
        /// </summary>
		Matrix,
        /// <summary>
        /// Class type for textures, shaders or strings. 
        /// </summary>
		Object,
        /// <summary>
        /// Structure class type.
        /// </summary>
		Struct
	}
}

