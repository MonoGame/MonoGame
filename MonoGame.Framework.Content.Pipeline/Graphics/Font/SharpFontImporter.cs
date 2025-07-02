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
        /// <summary>
        /// The collection of glyphs that were imported from the font.
        /// Each glyph represents a character in the font, along with its bitmap and metrics.
        /// </summary>
        public IEnumerable<Glyph> Glyphs { get; private set; }

        /// <summary>
        /// The line spacing for the font.
        /// This is the distance between the baselines of two consecutive lines of text.
        /// </summary>
        public float LineSpacing { get; private set; }

        /// <summary>
        /// The minimum Y offset for the font.
        /// This is used to calculate the Y offset for each character.
        /// </summary>
        public long YOffsetMin { get; private set; }

        /// <summary>
        /// Indicates if the italic style had to be simulated.
        /// </summary>
        public bool Italicized { get; private set; }

        /// <summary>
        /// Indicates if the bold style had to be simulated.
        /// </summary>
        public bool Emboldened { get; private set; }

        // Size of the temp surface used for GDI+ rasterization.
        const int MaxGlyphSize = 1024;

        public void Import(FontDescription options, string fontName)
        {
            CheckError(FreeType.FT_Init_FreeType(out FT_Library* library));

            // Create a bunch of GDI+ objects.
            var face = CreateFontFace(library, options, fontName);

            Italicized = options.Style.HasFlag(FontDescriptionStyle.Italic) && !face.StyleFlags.HasFlag(StyleFlags.Italic);
            Emboldened = options.Style.HasFlag(FontDescriptionStyle.Bold) && !face.StyleFlags.HasFlag(StyleFlags.Bold);
            bool draw3Times = options.Size > 15;

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
                    glyphData = ImportGlyph(glyphIndex, face, Italicized, Emboldened, draw3Times);
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
        private GlyphData ImportGlyph(uint glyphIndex, FT_Face* face, bool italicize, bool embolden, bool draw3Times)
        {
            CheckError(FreeType.FT_Load_Glyph(face, glyphIndex));
            CheckError(FreeType.FT_Render_Glyph(face->glyph));

            // Render the character.
            BitmapContent glyphBitmap = null;
            var abc = new ABCFloat();
            int finalWidth = face->glyph->bitmap.width;
            int finalHeight = face->glyph->bitmap.rows;

            if (finalWidth > 0 && finalHeight > 0)
            {
                glyphBitmap = new PixelBitmapContent<byte>((int)finalWidth, (int)finalHeight);
                byte[] gpixelAlphas = new byte[finalWidth * finalHeight];
                //if the character bitmap has 1bpp we have to expand the buffer data to get the 8bpp pixel data
                //each byte in bitmap.bufferdata contains the value of to 8 pixels in the row
                //if bitmap is of width 10, each row has 2 bytes with 10 valid bits, and the last 6 bits of 2nd byte must be discarded
                if ((FT_Pixel_Mode)face->glyph->bitmap.pixel_mode == FT_Pixel_Mode.FT_PIXEL_MODE_MONO)
                {
                    //variables needed for the expansion, amount of written data, length of the data to write
                    int written = 0, length = (int)(finalWidth * finalHeight);
                    for (int i = 0; written < length; i++)
                    {
                        //width in pixels of each row
                        int width = (int)finalWidth;
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

                if (embolden)
                {
                    Embolden(ref gpixelAlphas, ref abc, finalWidth, finalHeight, ref finalWidth, ref finalHeight, draw3Times);
                }
                if (italicize)
                {
                    Italicize(ref gpixelAlphas, ref abc, finalWidth, finalHeight, ref finalWidth);
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

            // I wouldn't say I'm a 100% sure, but I feel a lot surer about this than what it was before.
            abc.A += face.Glyph.Metrics.HorizontalBearingX >> 6;
            abc.B += face.Glyph.Metrics.Width >> 6;
            abc.C += (face.Glyph.Metrics.HorizontalAdvance >> 6) - (abc.A + abc.B) + finalWidth - face.Glyph.Bitmap.Width;

            // nkast fix, but only when necessary, this way we can have nice arial fonts without breaking the crucial Kingthings Petrock.
            if ((*face->glyph).bitmap_left < 0)
            {
                abc.A -= face->glyph->bitmap_left;
                abc.B += face->glyph->bitmap_left;
            }

            return new GlyphData(glyphIndex, glyphBitmap)
            {
                XOffset = face->glyph->bitmap_left,
                XAdvance = abc.A + abc.B + abc.C,
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
        /// <param name="startIndex">Position where to begin copying the results in destination</param>F
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

        private void Embolden(ref byte[] bitmap, ref ABCFloat charSize, int initialWidth, int height, ref int finalWidth, ref int finalHeight, bool draw3times)
        {
            finalWidth = initialWidth + (draw3times ? 2 : 1);
            charSize.A -= draw3times ? 2 : 1;
            charSize.C -= draw3times ? 2 : 1;
            int kLimit = draw3times ? 1 : 0;
            finalHeight += draw3times ? 1 : 0;
            byte[] destination = new byte[finalWidth * finalHeight];
            byte orig, dest;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < initialWidth; j++)
                {
                    orig = bitmap[initialWidth * i + j];
                    for (int k = -1; k <= kLimit; k++)
                    {
                        dest = destination[i * finalWidth + j + 1 + k];
                        destination[i * finalWidth + j + 1 + k] = orig + dest > byte.MaxValue ? byte.MaxValue : (byte)(orig + dest);
                        if (draw3times)
                        {
                            dest = destination[i * finalWidth + j + 1 + k + finalWidth];
                            destination[i * finalWidth + j + 1 + k + finalWidth] = orig + dest > byte.MaxValue ? byte.MaxValue : (byte)(orig + dest);
                        }
                    }
                }
            }
            bitmap = destination;
        }

        private void Italicize(ref byte[] bitmap, ref ABCFloat charSize, int initialWidth, int height, ref int finalWidth)
        {
            double displacement = Math.Tan(0.349066);//20 degrees converted to radians
            finalWidth += (int)Math.Ceiling(displacement * height);
            int extraWidth = finalWidth - initialWidth;
            charSize.A -= extraWidth;
            charSize.C -= extraWidth;
            byte[] destination = new byte[finalWidth * height];
            double currentDisplacement = 0;
            for (int row = height - 1; row >= 0; row--, currentDisplacement += displacement)
            {
                double leftVal = Math.Ceiling(currentDisplacement) - currentDisplacement;
                double rightVal = currentDisplacement - Math.Floor(currentDisplacement);
                if (leftVal == 0 && rightVal == 0)
                    leftVal = 1;
                for (int j = 0; j < initialWidth; j++)
                {
                    int OrigPos = row * initialWidth + j;
                    int DestPos = row * finalWidth + j + (int)Math.Floor(currentDisplacement);
                    if (destination[DestPos] + bitmap[OrigPos] * leftVal > byte.MaxValue)
                    {
                        destination[DestPos] = byte.MaxValue;
                    }
                    else
                    {
                        destination[DestPos] += (byte)(bitmap[OrigPos] * leftVal);
                    }
                    if (destination[DestPos + 1] + bitmap[OrigPos] * rightVal > byte.MaxValue)
                    {
                        destination[DestPos + 1] = byte.MaxValue;
                    }
                    else
                    {
                        destination[DestPos + 1] += (byte)(bitmap[OrigPos] * rightVal);
                    }
                }
            }
            bitmap = destination;
        }
    }
}
