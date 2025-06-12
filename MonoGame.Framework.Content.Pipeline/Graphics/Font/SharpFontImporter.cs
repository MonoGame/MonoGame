// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FreeTypeAPI;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    // Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
    unsafe internal class SharpFontImporter : IFontImporter
    {
        // Properties hold the imported font data.
        public IEnumerable<Glyph> Glyphs { get; private set; }

        public float LineSpacing { get; private set; }

        public long YOffsetMin { get; private set; }

        // Size of the temp surface used for GDI+ rasterization.
        const int MaxGlyphSize = 1024;

        public void Import(FontDescription options, string fontName)
        {
            CheckError(FreeType.FT_Init_FreeType(out FT_Library* library));

            // Create a bunch of GDI+ objects.
            var face = CreateFontFace(library, options, fontName);

            // Which characters do we want to include?
            var characters = options.Characters;

            var glyphList = new List<Glyph>();
            var glyphMaps = new Dictionary<uint, GlyphData>();

            // Rasterize each character in turn.
            foreach (char character in characters)
            {
                uint glyphIndex = FreeType.FT_Get_Char_Index(face, new CULong(character));
                if (!glyphMaps.TryGetValue(glyphIndex, out GlyphData glyphData))
                {
                    glyphData = ImportGlyph(glyphIndex, face);
                    glyphMaps.Add(glyphIndex, glyphData);
                }

                var glyph = new Glyph(character, glyphData);
                glyphList.Add(glyph);
            }
            Glyphs = glyphList;

            // Store the font height.
            LineSpacing = face->size->metrics.height.Value >> 6;

            // The height used to calculate the Y offset for each character.
            YOffsetMin = -face->size->metrics.ascender.Value >> 6;

            CheckError(FreeType.FT_Done_Face(face));
            CheckError(FreeType.FT_Done_FreeType(library));
        }

        private void CheckError(int error)
        {
            if (error == 0)
                return;

            throw new Exception("An error occured in freetype."); // TODO: Fill the error
        }

        // Attempts to instantiate the requested GDI+ font object.
        private FT_Face* CreateFontFace(FT_Library* library, FontDescription options, string fontName)
        {
            const uint dpi = 96;

            CheckError(FreeType.FT_New_Face(library, fontName, new CLong(0), out FT_Face* face));

            var fixedSize = ((int)options.Size) << 6;
            CheckError(FreeType.FT_Set_Char_Size(face, new CLong(0), new CLong(fixedSize), dpi, dpi));

            return face;
        }

        // Rasterizes a single character glyph.
        private GlyphData ImportGlyph(uint glyphIndex, FT_Face* face)
        {
            CheckError(FreeType.FT_Load_Glyph(face, glyphIndex));
            CheckError(FreeType.FT_Render_Glyph(face->glyph));

            // Render the character.
            BitmapContent glyphBitmap = null;
            if (face->glyph->bitmap.width > 0 && face->glyph->bitmap.rows > 0)
            {
                glyphBitmap = new PixelBitmapContent<byte>((int)face->glyph->bitmap.width, (int)face->glyph->bitmap.rows);
                byte[] gpixelAlphas = new byte[face->glyph->bitmap.width * face->glyph->bitmap.rows];
                //if the character bitmap has 1bpp we have to expand the buffer data to get the 8bpp pixel data
                //each byte in bitmap.bufferdata contains the value of to 8 pixels in the row
                //if bitmap is of width 10, each row has 2 bytes with 10 valid bits, and the last 6 bits of 2nd byte must be discarded
                if ((FT_Pixel_Mode)face->glyph->bitmap.pixel_mode == FT_Pixel_Mode.FT_PIXEL_MODE_MONO)
                {
                    //variables needed for the expansion, amount of written data, length of the data to write
                    int written = 0, length = (int)(face->glyph->bitmap.width * face->glyph->bitmap.rows);
                    for (int i = 0; written < length; i++)
                    {
                        //width in pixels of each row
                        int width = (int)face->glyph->bitmap.width;
                        while (width > 0)
                        {
                            //valid data in the current byte
                            int stride = MathHelper.Min(8, width);
                            //copy the valid bytes to pixeldata
                            //System.Array.Copy(ExpandByte(face.Glyph.Bitmap.BufferData[i]), 0, gpixelAlphas, written, stride);
                            ExpandByteAndCopy(face->glyph->bitmap.buffer[i], stride, gpixelAlphas, written);
                            written += stride;
                            width -= stride;
                            if (width > 0)
                                i++;
                        }
                    }
                }
                else
                {
                    gpixelAlphas = new Span<byte>(face->glyph->bitmap.buffer, gpixelAlphas.Length).ToArray();
                }
                glyphBitmap.SetPixelData(gpixelAlphas);
            }

            if (glyphBitmap == null)
            {
                var gHA = face->glyph->metrics.horiAdvance.Value >> 6;
                var gVA = face->size->metrics.height.Value >> 6;

                gHA = gHA > 0 ? gHA : gVA;
                gVA = gVA > 0 ? gVA : gHA;

                glyphBitmap = new PixelBitmapContent<byte>((int)gHA, (int)gVA);
            }

            // not sure about this at all
            var abc = new ABCFloat();
            abc.A = face->glyph->metrics.horiBearingX.Value >> 6;
            abc.B = face->glyph->metrics.width.Value >> 6;
            abc.C = (face->glyph->metrics.horiAdvance.Value >> 6) - (abc.A + abc.B);
            abc.A -= face->glyph->bitmap_left;
            abc.B += face->glyph->bitmap_left;

            // Construct the output Glyph object.
            return new GlyphData(glyphIndex, glyphBitmap)
            {
                XOffset = -(face->glyph->advance.x.Value >> 6),
                XAdvance = face->glyph->metrics.horiAdvance.Value >> 6,
                YOffset = -(face->glyph->metrics.horiBearingY.Value >> 6),
                CharacterWidths = abc
            };
        }


        /// <summary>
        /// Reads each individual bit of a byte from left to right and expands it to a full byte, 
        /// ones get byte.maxvalue, and zeros get byte.minvalue.
        /// </summary>
        /// <param name="origin">Byte to expand and copy</param>
        /// <param name="length">Number of Bits of the Byte to copy, from 1 to 8</param>
        /// <param name="destination">Byte array where to copy the results</param>
        /// <param name="startIndex">Position where to begin copying the results in destination</param>
        private static void ExpandByteAndCopy(byte origin, int length, byte[] destination, int startIndex)
        {
            byte tmp;
            for (int i = 7; i > 7 - length; i--)
            {
                tmp = (byte)(1 << i);
                if (origin / tmp == 1)
                {
                    destination[startIndex + 7 - i] = byte.MaxValue;
                    origin -= tmp;
                }
                else
                    destination[startIndex + 7 - i] = byte.MinValue;
            }
        }
    }
}
