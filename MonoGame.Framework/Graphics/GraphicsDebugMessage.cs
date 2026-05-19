// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// NOTE: This is only used in DirectX contexts (see: GraphicsDebug.DirectX)
//       The other contexts are set to return an instance with an empty message (see: GraphicsDebug.Native and GraphicsDebug.Default)
//
//       So for reference as to the messages received, you can find more information at
//       https://learn.microsoft.com/en-us/windows/win32/api/d3d11sdklayers/ns-d3d11sdklayers-d3d11_message

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a debug message from the graphics subsystem.
    /// </summary>
    public class GraphicsDebugMessage
    {
        /// <summary>
        /// Gets or Sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or Sets the debug severity level of the message.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Gets or Sets the ID of the debug message.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or Sets the string representation of the debug message ID.
        /// </summary>
        public string IdName { get; set; }

        /// <summary>
        /// Gets or Sets the category of the debug message.
        /// </summary>
        public string Category { get; set; }
    }
}
