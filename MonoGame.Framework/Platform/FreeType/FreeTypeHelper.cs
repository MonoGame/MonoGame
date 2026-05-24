// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

internal static unsafe class FreeTypeHelper
{
    public readonly struct Fixed26Dot6
    {
        // FT_26Dot6 in fttypes.h is a signed 26.6 fixed-point value.
        // Shifting by 6 converts form 1/64th pixels to whole pixels.
        public const int Fixed26Dot6FractionalBits = 6;

        private readonly long _value;

        public Fixed26Dot6(int value) => _value = value;
        public Fixed26Dot6(long value) => _value = value;
        public int Floor() => (int)(_value >> Fixed26Dot6FractionalBits);
    }

    public static unsafe int GetAscender(FreeType.FT_FaceRec* face)
    {
        FreeType.FT_SizeRec* size = GetSize(face);

        long ascender = FreeType.CLongSize == 4 ?
                        ((FreeType.FT_SizeRec32*)size)->metrics.ascender :
                        ((FreeType.FT_SizeRec64*)size)->metrics.ascender;

        Fixed26Dot6 fixedValue = new Fixed26Dot6(ascender);
        return fixedValue.Floor();
    }

    public static unsafe int GetLineSpacing(FreeType.FT_FaceRec* face)
    {
        FreeType.FT_SizeRec* size = GetSize(face);

        long height = FreeType.CLongSize == 4 ?
                      ((FreeType.FT_SizeRec32*)size)->metrics.height :
                      ((FreeType.FT_SizeRec64*)size)->metrics.height;

        Fixed26Dot6 fixedValue = new Fixed26Dot6(height);
        return fixedValue.Floor();
    }

    public static unsafe FreeType.FT_GlyphSlotRec* GetGlyphSlot(FreeType.FT_FaceRec* face)
    {
        return FreeType.CLongSize == 4 ?
               ((FreeType.FT_FaceRec32*)face)->glyph :
               ((FreeType.FT_FaceRec64*)face)->glyph;
    }

    public static unsafe FreeType.FT_Bitmap GetGlyphBitmap(FreeType.FT_GlyphSlotRec* glyphSlot)
    {
        return FreeType.CLongSize == 4 ?
               ((FreeType.FT_GlyphSlotRec32*)glyphSlot)->bitmap :
               ((FreeType.FT_GlyphSlotRec64*)glyphSlot)->bitmap;
    }

    public static unsafe int GetGlyphBitmapLeft(FreeType.FT_GlyphSlotRec* glyphSlot)
    {
        return FreeType.CLongSize == 4 ?
               ((FreeType.FT_GlyphSlotRec32*)glyphSlot)->bitmap_left :
               ((FreeType.FT_GlyphSlotRec64*)glyphSlot)->bitmap_left;
    }

    public static unsafe int GetGlyphBitmapTop(FreeType.FT_GlyphSlotRec* glyphSlot)
    {
        return FreeType.CLongSize == 4 ?
               ((FreeType.FT_GlyphSlotRec32*)glyphSlot)->bitmap_top :
               ((FreeType.FT_GlyphSlotRec64*)glyphSlot)->bitmap_top;
    }

    public static unsafe int GetGlyphHorizontalAdvance(FreeType.FT_GlyphSlotRec* glyphSlot)
    {
        long horiAdvance = FreeType.CLongSize == 4 ?
                           ((FreeType.FT_GlyphSlotRec32*)glyphSlot)->metrics.horiAdvance :
                           ((FreeType.FT_GlyphSlotRec64*)glyphSlot)->metrics.horiAdvance;

        Fixed26Dot6 fixedValue = new Fixed26Dot6(horiAdvance);
        return fixedValue.Floor();
    }

    private static unsafe FreeType.FT_SizeRec* GetSize(FreeType.FT_FaceRec* face)
    {
        return FreeType.CLongSize == 4 ?
               ((FreeType.FT_FaceRec32*)face)->size :
               ((FreeType.FT_FaceRec64*)face)->size;
    }
}
