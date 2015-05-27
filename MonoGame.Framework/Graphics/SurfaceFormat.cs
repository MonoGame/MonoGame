// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines types of surface formats.
    /// </summary>
    public enum SurfaceFormat
    {
        /// <summary>
        /// Unsigned 32-bit ARGB pixel format for store 8 bits per channel. 
        /// </summary>
        Color,
        /// <summary>
        /// Unsigned 16-bit BGR pixel format for store 5 bits for blue, 6 bits for green, and 5 bits for red.   
        /// </summary>
        Bgr565,
        /// <summary>
        /// Unsigned 16-bit BGRA pixel format where 5 bits reserved for each color and last bit is reserved for alpha.
        /// </summary>
        Bgra5551,
        /// <summary>
        /// Unsigned 16-bit BGRA pixel format for store 4 bits per channel.
        /// </summary>
        Bgra4444,
        /// <summary>
        /// DXT1. Texture format with compression. Surface dimensions must be a multiple 4.
        /// </summary>
        Dxt1,
        /// <summary>
        /// DXT3. Texture format with compression. Surface dimensions must be a multiple 4.
        /// </summary>
        Dxt3, 
        /// <summary>
        /// DXT5. Texture format with compression. Surface dimensions must be a multiple 4.
        /// </summary>
        Dxt5 = 6,
        /// <summary>
        /// Signed 16-bit bump-map format for store 8 bits for <c>u</c> and <c>v</c> data.
        /// </summary>
        NormalizedByte2,
        /// <summary>
        /// Signed 16-bit bump-map format for store 8 bits per channel.
        /// </summary>
        NormalizedByte4,
        /// <summary>
        /// Unsigned 32-bit RGBA pixel format for store 10 bits for each color and 2 bits for alpha.
        /// </summary>
        Rgba1010102,
        /// <summary>
        /// Unsigned 32-bit RG pixel format using 16 bits per channel.
        /// </summary>
        Rg32,
        /// <summary>
        /// Unsigned 64-bit RGBA pixel format using 16 bits per channel.
        /// </summary>
        Rgba64,
        /// <summary>
        /// Unsigned A 8-bit format for store 8 bits to alpha channel.
        /// </summary>
        Alpha8,
        /// <summary>
        /// IEEE 32-bit R float format for store 32 bits to red channel.
        /// </summary>
        Single,
        /// <summary>
        /// IEEE 64-bit RG float format for store 32 bits per channel.
        /// </summary>
        Vector2,
        /// <summary>
        /// IEEE 128-bit RGBA float format for store 32 bits per channel.
        /// </summary>
        Vector4,
        /// <summary>
        /// Float 16-bit R format for store 16 bits to red channel.   
        /// </summary>
        HalfSingle,
        /// <summary>
        /// Float 32-bit RG format for store 16 bits per channel. 
        /// </summary>
        HalfVector2,
        /// <summary>
        /// Float 64-bit ARGB format for store 16 bits per channel. 
        /// </summary>
        HalfVector4,
        /// <summary>
        /// Float pixel format for high dynamic range data.
        /// </summary>
        HdrBlendable,

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
        /// Unsigned 32-bit RGBA sRGB pixel format that supports 8 bits per channel.
        /// </summary>
        ColorSRgb = 30,
        /// <summary>
        /// Unsigned 32-bit sRGB pixel format that supports 8 bits per channel. 8 bits are unused.
        /// </summary>
        Bgr32SRgb = 31,
        /// <summary>
        /// Unsigned 32-bit sRGB pixel format that supports 8 bits per channel.
        /// </summary>
        Bgra32SRgb = 32,
        /// <summary>
        /// DXT1. sRGB texture format with compression. Surface dimensions must be a multiple of 4.
        /// </summary>
        Dxt1SRgb = 33,
        /// <summary>
        /// DXT3. sRGB texture format with compression. Surface dimensions must be a multiple of 4.
        /// </summary>
        Dxt3SRgb = 34,
        /// <summary>
        /// DXT5. sRGB texture format with compression. Surface dimensions must be a multiple of 4.
        /// </summary>
        Dxt5SRgb = 35,

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
        RgbaAtcExplicitAlpha =  80,
        /// <summary>
        /// ATC/ATITC compression (Android)
        /// </summary>
        RgbaAtcInterpolatedAlpha = 81,

        #endregion
    }
}

