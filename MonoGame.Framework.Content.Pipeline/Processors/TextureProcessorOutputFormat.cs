// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Specifies the target output (of type SurfaceFormat) of the texture processor. Used by TextureProcessor.TextureFormat.
    /// </summary>
    public enum TextureProcessorOutputFormat
    {
        /// <summary>
        /// The SurfaceFormat value, of the input TextureContent object, is converted to Color by the processor. Typically used for 2D graphics and overlays.
        /// </summary>
        Color,

        /// <summary>
        /// The SurfaceFormat value, of the input TextureContent object, is converted to an appropriate DXT compression by the processor. If the input texture contains fractional alpha values, it is converted to DXT5 format (8 bits per texel); otherwise it is converted to DXT1 (4 bits per texel). This conversion reduces the resource's size on the graphics card. Typically used for 3D textures such as 3D model textures.
        /// </summary>
        DXTCompressed,

        /// <summary>
        /// The SurfaceFormat value, of the input TextureContent object, is not changed by the processor. Typically used for textures processed by an external tool.
        /// </summary>
        NoChange,

        /// <summary>
        /// The SurfaceFormat value, of the input TextureContent object, is converted to an appropriate compressed format for the target platform.
        /// This can include PVRTC for iOS, DXT for Windows 8 and Windows Phone, and ETC1 for Android.  Some Android devices can also support additional
        /// compression formats, including PVRTC and DXT plus ATITC and other formats.
        /// </summary>
        Compressed,
    }
}
