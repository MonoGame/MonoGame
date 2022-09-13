// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Flags that describe style information to be applied to text.
    /// You can combine these flags by using a bitwise OR operator (|).
    /// 
    /// FW (12.09.2022):
    /// Reorg the flags and added underline and strikeout. 
    /// </summary>
    [Flags]
    public enum FontDescriptionStyle
    {
        /// <summary>
        /// Normal text.
        /// </summary>
        Regular = 0x01,

        /// <summary>
        /// Bold text.
        /// </summary>
        Bold = 0x02,

        /// <summary>
        /// Italic text.
        /// </summary>
        Italic = 0x04,

        /// <summary>
        /// Underlined text.
        /// </summary>
        Underline = 0x08,

        /// <summary>
        /// Strikeout text.
        /// </summary>
        Strikeout = 0x10,
    }
}
