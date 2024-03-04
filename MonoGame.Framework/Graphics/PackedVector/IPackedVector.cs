// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// Author: Kenneth James Pouncey

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    /// <summary>
    /// Interface that converts packed vector types to and from <see cref="Vector4"/>
    /// values, allowing multiple encodings to be manipulated in a generic way.
    /// </summary>
	public interface IPackedVector
	{
        /// <summary>
        /// Sets the packed representation from a <see cref="Vector4"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector4"/> to create the packed representation from.
        /// </param>
		void PackFromVector4 (Vector4 vector);

        /// <summary>
        /// Expands the packed representation into a <see cref="Vector4"/>.
        /// </summary>
        /// <returns>The expanded <see cref="Vector4"/>.</returns>
		Vector4 ToVector4 ();
	}

    /// <summary>
    /// Converts packed vector types to and from <see cref="Vector4"/> values.
    /// </summary>
	public interface IPackedVector<TPacked> : IPackedVector
	{
        /// <summary>
        /// Directly gets or sets the packed representation of the value.
        /// </summary>
		TPacked PackedValue { get; set; }
	}

}



