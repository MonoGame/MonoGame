// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines types for effect parameters and shader constants.
	/// </summary>
	public enum EffectParameterType
	{
        /// <summary>
        /// Pointer to void type.
        /// </summary>
		Void,
        /// <summary>
        /// Boolean type. Any non-zero will be <c>true</c>; <c>false</c> otherwise.
        /// </summary>
		Bool,
        /// <summary>
        /// 32-bit integer type.
        /// </summary>
		Int32,
        /// <summary>
        /// Float type.
        /// </summary>
		Single,
        /// <summary>
        /// String type.
        /// </summary>
		String,
        /// <summary>
        /// Any texture type.
        /// </summary>
		Texture,
        /// <summary>
        /// 1D-texture type.
        /// </summary>
        Texture1D,  
        /// <summary>
        /// 2D-texture type.
        /// </summary>
        Texture2D,
        /// <summary>
        /// 3D-texture type.
        /// </summary>
        Texture3D,
        /// <summary>
        /// Cubic texture type.
        /// </summary>
		TextureCube
	}
}