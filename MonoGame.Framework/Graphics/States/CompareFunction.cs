// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The comparison function used for depth, stencil, and alpha tests.
    /// </summary>
    public enum CompareFunction
    {
        /// <summary>
        /// Always passes the test.
        /// </summary>
        Always,
        /// <summary>
        /// Never passes the test.
        /// </summary>
        Never,
        /// <summary>
        /// Passes the test when the new pixel value is less than current pixel value.
        /// </summary>
        Less,
        /// <summary>
        /// Passes the test when the new pixel value is less than or equal to current pixel value.
        /// </summary>
        LessEqual,
        /// <summary>
        /// Passes the test when the new pixel value is equal to current pixel value.
        /// </summary>
        Equal,
        /// <summary>
        /// Passes the test when the new pixel value is greater than or equal to current pixel value.
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// Passes the test when the new pixel value is greater than current pixel value.
        /// </summary>
        Greater,
        /// <summary>
        /// Passes the test when the new pixel value does not equal to current pixel value.
        /// </summary>
        NotEqual
    }
}