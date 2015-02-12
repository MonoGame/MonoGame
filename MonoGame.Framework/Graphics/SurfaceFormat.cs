// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines types of surface formats.
    /// </summary>
    /// <remarks>
    /// Good explanation of compressed formats for mobile devices (aimed at Android, but describes PVRTC)
    /// http://developer.motorola.com/docstools/library/understanding-texture-compression/
    /// </remarks>
    public enum SurfaceFormat
    {
        /// <summary>
        /// Unsigned 32-bit ARGB pixel format for store 8 bits per channel. 
        /// </summary>
        Color = 0,
        /// <summary>
        /// Unsigned 16-bit BGR pixel format for store 5 bits for blue, 6 bits for green, and 5 bits for red.   
        /// </summary>
        Bgr565 = 1,
        /// <summary>
        /// Unsigned 16-bit BGRA pixel format where 5 bits reserved for each color and last bit is reserved for alpha.
        /// </summary>
        Bgra5551 = 2,
        /// <summary>
        /// Unsigned 16-bit BGRA pixel format for store 4 bits per channel.
        /// </summary>
        Bgra4444 = 3,
        /// <summary>
        /// DXT1. Texture format with compression. Surface dimensions must be a multiple 4.
        /// </summary>
        Dxt1 = 4,
        /// <summary>
        /// DXT3. Texture format with compression. Surface dimensions must be a multiple 4.
        /// </summary>
        Dxt3 = 5, 
        /// <summary>
        /// DXT5. Texture format with compression. Surface dimensions must be a multiple 4.
        /// </summary>
        Dxt5 = 6,
        /// <summary>
        /// Signed 16-bit bump-map format for store 8 bits for <c>u</c> and <c>v</c> data.
        /// </summary>
        NormalizedByte2 = 7,
        /// <summary>
        /// Signed 16-bit bump-map format for store 8 bits per channel.
        /// </summary>
        NormalizedByte4 = 8,
        /// <summary>
        /// Unsigned 32-bit RGBA pixel format for store 10 bits for each color and 2 bits for alpha.
        /// </summary>
        Rgba1010102 = 9,
        /// <summary>
        /// Unsigned 32-bit RG pixel format using 16 bits per channel.
        /// </summary>
        Rg32 = 10,
        /// <summary>
        /// Unsigned 64-bit RGBA pixel format using 16 bits per channel.
        /// </summary>
        Rgba64 = 11,
        /// <summary>
        /// Unsigned A 8-bit format for store 8 bits to alpha channel.
        /// </summary>
        Alpha8 = 12,
        /// <summary>
        /// IEEE 32-bit R float format for store 32 bits to red channel.
        /// </summary>
        Single = 13,
        /// <summary>
        /// IEEE 64-bit RG float format for store 32 bits per channel.
        /// </summary>
        Vector2 = 14,
        /// <summary>
        /// IEEE 128-bit RGBA float format for store 32 bits per channel.
        /// </summary>
        Vector4 = 15,
        /// <summary>
        /// Float 16-bit R format for store 16 bits to red channel.   
        /// </summary>
        HalfSingle = 16,
        /// <summary>
        /// Float 32-bit RG format for store 16 bits per channel. 
        /// </summary>
        HalfVector2 = 17,
        /// <summary>
        /// Float 64-bit ARGB format for store 16 bits per channel. 
        /// </summary>
        HalfVector4 = 18,
        /// <summary>
        /// Float pixel format for high dynamic range data.
        /// </summary>
        HdrBlendable = 19,

        #region Extensions

        /// <summary>
        /// For compatibility with WPF D3DImage.
        /// </summary>
        Bgr32 = 20,     // B8G8R8X8
        /// <summary>
        /// For compatibility with WPF D3DImage.
        /// </summary>
        Bgra32 = 21,    // B8G8R8A8    
		/// <summary>
        /// PowerVR texture compression format (iOS and Android).
		/// </summary>
		RgbPvrtc2Bpp = 50,
        /// <summary>
        /// PowerVR texture compression format (iOS and Android).
        /// </summary>
		RgbPvrtc4Bpp = 51,
        /// <summary>
        /// PowerVR texture compression format (iOS and Android).
        /// </summary>
		RgbaPvrtc2Bpp = 52,
        /// <summary>
        /// PowerVR texture compression format (iOS and Android).
        /// </summary>
		RgbaPvrtc4Bpp = 53,
		/// <summary>
        /// Ericcson Texture Compression (Android)
		/// </summary>
		RgbEtc1 = 60,
        /// <summary>
        /// DXT1 version where 1-bit alpha is used.
        /// </summary>
        Dxt1a = 70,
        /// <summary>
        /// ATC/ATITC compression (Android)
        /// </summary>
        RgbaATCExplicitAlpha =  80,
        /// <summary>
        /// ATC/ATITC compression (Android)
        /// </summary>
        RgbaATCInterpolatedAlpha = 81,

        #endregion
    }
}

