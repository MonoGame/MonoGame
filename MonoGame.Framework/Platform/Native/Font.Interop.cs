// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;

namespace MonoGame.Interop;

[MGHandle]
internal readonly struct MGF_RuntimeFont
{
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGF_CharacterRegion
{
    public char Start;
    public char End;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGF_Glyph
{
    public char Character;
    public int BoundsX;
    public int BoundsY;
    public int BoundsWidth;
    public int BoundsHeight;
    public int CroppingX;
    public int CroppingY;
    public int CroppingWidth;
    public int CroppingHeight;
    public float LeftSideBearing;
    public float Width;
    public float RightSideBearing;
}

internal static unsafe class MGF
{
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_Create", ExactSpelling = true)]
    public static extern MGF_RuntimeFont* RuntimeFont_Create(byte* data, int dataBytes, int size);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_Destroy", ExactSpelling = true)]
    public static extern void RuntimeFont_Destroy(MGF_RuntimeFont* runtimeFont);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_EnsureGlyphs", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RuntimeFont_EnsureGlyphs(MGF_RuntimeFont* runtimeFont,
                                                       MGF_CharacterRegion* characterRegions,
                                                       int characterRegionCount,
                                                       out byte* atlasRgba,
                                                       out int atlasWidth,
                                                       out int atlasHeight,
                                                       [MarshalAs(UnmanagedType.U1)] out bool atlasRebuilt,
                                                       out MGF_Glyph* glyphs,
                                                       out int glyphCount,
                                                       out int lineSpacing);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_BakeSpriteFont", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool BakeSpriteFont(byte* data,
                                             int dataBytes,
                                             int size,
                                             MGF_CharacterRegion* characterRegions,
                                             int characterRegionCount,
                                             out byte* atlasRgba,
                                             out int atlasWidth,
                                             out int atlasHeight,
                                             out MGF_Glyph* glyphs,
                                             out int glyphCount,
                                             out int lineSpacing);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_Free", ExactSpelling = true)]
    public static extern void Free(void* resource);
}
