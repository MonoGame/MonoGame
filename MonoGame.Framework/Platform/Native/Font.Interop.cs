// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;

namespace MonoGame.Interop;

[MGHandle]
internal readonly struct MGF_Font
{
}

internal enum MGF_ResultCode
{
    Success = 0,
    InvalidArgument = 1,
    InvalidFontData = 2,
    OutOfMemory = 3,
    BackendInitializationFailed = 4,
    FontSizeSetupFailed = 5,
    GlyphLoadFailed = 6,
    GlyphRenderFailed = 7,
    UnsupportedGlyphBitmapFormat = 8,
    AtlasCapacityExceeded = 9,
    NoGlyphData = 10,
    InternalError = 11
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
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGF_BakeSpriteFontRequest
{
    public byte* Data;
    public int DataBytes;
    public int Size;
    public MGF_CharacterRegion* CharacterRegion;
    public int characterRegionCount;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGF_BakeSpriteFontResult
{
    public byte* AtlasRgba;
    public int AtlasWidth;
    public int AtlasHeight;
    public MGF_Glyph* Glyphs;
    public int GlyphCount;
    public int LineSpacing;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGF_FontEnsureGlyphsRequest
{
    public MGF_Font* Font;
    public int Size;
    public MGF_CharacterRegion* CharacterRegions;
    public int CharacterRegionCount;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGF_FontEnsureGlyphsResult
{
    public MGF_PageUpdate* PageUpdates;
    public int PageUpdateCount;
    public MGF_Glyph* Glyphs;
    public int GlyphCount;
    public int LineSpacing;
}

internal static unsafe class MGF
{
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = nameof(MGF_Font_Create), ExactSpelling = true)]
    public static extern MGF_ResultCode MGF_Font_Create(byte* data, int dataBytes, out MGF_Font* font);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = nameof(MGF_Font_Destroy), ExactSpelling = true)]
    public static extern void MGF_Font_Destroy(MGF_Font* font);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = nameof(MGF_Font_EnsureGlyphs), ExactSpelling = true)]
    public static extern MGF_ResultCode MGF_Font_EnsureGlyphs(MGF_FontEnsureGlyphsRequest* request,
                                                              MGF_FontEnsureGlyphsResult* result);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = nameof(MGF_BakeSpriteFont), ExactSpelling = true)]
    public static extern MGF_ResultCode MGF_BakeSpriteFont(MGF_BakeSpriteFontRequest* request,
                                                           MGF_BakeSpriteFontResult* result);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = nameof(MGF_Free), ExactSpelling = true)]
    public static extern void MGF_Free(void* resource);
}
