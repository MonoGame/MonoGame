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
        Alpha8 = 15,
        Bgr233 = 0x10,
        Bgr24 = 0x11,
        Bgr32 = 2,
        Bgr444 = 13,
        Bgr555 = 11,
        Bgr565 = 9,
        Bgra1010102 = 3,
        Bgra2338 = 14,
        Bgra4444 = 12,
        Bgra5551 = 10,
        Color = 1,
        Depth15Stencil1 = 0x38,
        Depth16 = 0x36,
        Depth24 = 0x33,
        Depth24Stencil4 = 50,
        Depth24Stencil8 = 0x30,
        Depth24Stencil8Single = 0x31,
        Depth32 = 0x34,
        Dxt1 = 0x1c,
        Dxt2 = 0x1d,
        Dxt3 = 30,
        Dxt4 = 0x1f,
        Dxt5 = 0x20,
        HalfSingle = 0x19,
        HalfVector2 = 0x1a,
        HalfVector4 = 0x1b,
        Luminance16 = 0x22,
        Luminance8 = 0x21,
        LuminanceAlpha16 = 0x24,
        LuminanceAlpha8 = 0x23,
        Multi2Bgra32 = 0x2f,
        NormalizedAlpha1010102 = 0x29,
        NormalizedByte2 = 0x12,
        NormalizedByte2Computed = 0x2a,
        NormalizedByte4 = 0x13,
        NormalizedLuminance16 = 0x27,
        NormalizedLuminance32 = 40,
        NormalizedShort2 = 20,
        NormalizedShort4 = 0x15,
        Palette8 = 0x25,
        PaletteAlpha16 = 0x26,
        Rg32 = 7,
        Rgb32 = 5,
        Rgba1010102 = 6,
        Rgba32 = 4,
        Rgba64 = 8,
        Single = 0x16,
        Unknown = -1,
        Vector2 = 0x17,
        Vector4 = 0x18,
        VideoGrGb = 0x2d,
        VideoRgBg = 0x2e,
        VideoUyVy = 0x2c,
        VideoYuYv = 0x2b
    }
}

