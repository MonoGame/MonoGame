// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;

namespace MonoGame.Interop;

[MGHandle]
internal readonly struct MGF_RuntimeFont
{
}

internal enum MGF_RuntimeFontErrorCode
{
    None = 0,
    InvalidArgument = 1,
    OutOfMemory = 2,
    AtlasCapacityExceeded = 3,
    NoGlyphData = 4,
    Unknown = 5
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
    public int Size;
    public int PageIndex;
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

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGF_PageUpdate
{
    public int PageIndex;
    public byte* AtlasRgba;
    public int AtlasWidth;
    public int AtlasHeight;
    [MarshalAs(UnmanagedType.U1)]
    public bool AtlasRebuilt;
}

internal static unsafe class MGF
{
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_Create", ExactSpelling = true)]
    public static extern MGF_RuntimeFont* RuntimeFont_Create(byte* data, int dataBytes);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_Destroy", ExactSpelling = true)]
    public static extern void RuntimeFont_Destroy(MGF_RuntimeFont* runtimeFont);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_EnsureGlyphs", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool RuntimeFont_EnsureGlyphs(MGF_RuntimeFont* runtimeFont,
                                                       int size,
                                                       MGF_CharacterRegion* characterRegions,
                                                       int characterRegionCount,
                                                       out MGF_PageUpdate* pageUpdates,
                                                       out int pageUpdateCount,
                                                       out MGF_Glyph* glyphs,
                                                       out int glyphCount,
                                                       out int lineSpacing);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_GetLastErrorCode", ExactSpelling = true)]
    public static extern int RuntimeFont_GetLastErrorCode(MGF_RuntimeFont* runtimeFont);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGF_RuntimeFont_GetLastErrorMessage", ExactSpelling = true)]
    public static extern nint RuntimeFont_GetLastErrorMessage(MGF_RuntimeFont* runtimeFont);    

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
