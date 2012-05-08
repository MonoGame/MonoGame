#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

namespace Microsoft.Xna.Framework.Graphics
{
    using System;

    public enum SurfaceFormat
    {
        Color = 0,
        Bgr565 = 1,
        Bgra5551 = 2,
        Bgra4444 = 3,
        Dxt1 = 4,
        Dxt3 = 5,
        Dxt5 = 6,
        NormalizedByte2 = 7,
        NormalizedByte4 = 8,
        Rgba1010102 = 9,
        Rg32 = 10,
        Rgba64 = 11,
        Alpha8 = 12,
        Single = 13,
        Vector2 = 14,
        Vector4 = 15,
        HalfSingle = 16,
        HalfVector2 = 17,
        HalfVector4 = 18,
        HdrBlendable = 19,
        
		// Good explanation of compressed formats for mobile devices (aimed at Android, but describes PVRTC)
		// http://developer.motorola.com/docstools/library/understanding-texture-compression/

		// PowerVR texture compression (iOS and Android)
		RgbPvrtc2Bpp = 50,
		RgbPvrtc4Bpp = 51,
		RgbaPvrtc2Bpp = 52,
		RgbaPvrtc4Bpp = 53,

		// Ericcson Texture Compression (Android)
		RgbEtc1 = 60,
    }
    
    public enum SurfaceFormat_Legacy
    {
        Unknown = -1,
        Color = 1,
        Bgr32 = 2,
        Bgra1010102 = 3,
        Rgba32 = 4,
        Rgb32 = 5,
        Rgba1010102 = 6,
        Rg32 = 7,
        Rgba64 = 8,
        Bgr565 = 9,
        Bgra5551 = 10,
        Bgr555 = 11,
        Bgra4444 = 12,
        Bgr444 = 13,
        Bgra2338 = 14,
        Alpha8 = 15,
        Bgr233 = 16,
        Bgr24 = 17,
        NormalizedByte2 = 18,
        NormalizedByte4 = 19,
        NormalizedShort2 = 20,
        NormalizedShort4 = 21,
        Single = 22,
        Vector2 = 23,
        Vector4 = 24,
        HalfSingle = 25,
        HalfVector2 = 26,
        HalfVector4 = 27,
        Dxt1 = 28,
        Dxt2 = 29,
        Dxt3 = 30,
        Dxt4 = 31,
        Dxt5 = 32,
        Luminance8 = 33,
        Luminance16 = 34,
        LuminanceAlpha8 = 35,
        LuminanceAlpha16 = 36,
        Palette8 = 37,
        PaletteAlpha16 = 38,
        NormalizedLuminance16 = 39,
        NormalizedLuminance32 = 40,
        NormalizedAlpha1010102 = 41,
        NormalizedByte2Computed = 42,
        VideoYuYv = 43,
        VideoUyVy = 44,
        VideoGrGb = 45,
        VideoRgBg = 46,
        Multi2Bgra32 = 47,
        Depth24Stencil8 = 48,
        Depth24Stencil8Single = 49,
        Depth24Stencil4 = 50,
        Depth24 = 51,
        Depth32 = 52,
        Depth16 = 54,
        Depth15Stencil1 = 56,
    }
}

