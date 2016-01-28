// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpFont;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    // Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
    internal class SharpFontImporter : IFontImporter
    {
        // Properties hold the imported font data.
        public IEnumerable<Glyph> Glyphs
        {
            get; private set;
        }

        public float LineSpacing
        {
            get; private set;
        }

        public int YOffsetMin
        {
            get; private set;
        }

        // Size of the temp surface used for GDI+ rasterization.
        const int MaxGlyphSize = 1024;

        Library lib = null;

        public void Import(FontDescription options, string fontName)
        {
            lib = new Library();
            // Create a bunch of GDI+ objects.
            var face = CreateFontFace(options, fontName);
            int maxAscender = int.MinValue, maxDescender = int.MinValue;
            try
            {
                // Which characters do we want to include?
                var characters = options.Characters;
                var glyphIndexes = new List<KeyValuePair<char, uint>>(characters.Count);
                //Get the glyph index for each requested character
                foreach (var item in characters)
                    glyphIndexes.Add(new KeyValuePair<char, uint>(item, face.GetCharIndex(item)));
                var glyphList = new List<Glyph>();
                // Load each glyph in the font get the max ascender and descender in the whole font,
                // if the glyph is one of the requested characters prepare its data
                for (uint i = 0; i < face.GlyphCount; i++)
                {
                    face.LoadGlyph(i, LoadFlags.Default, LoadTarget.Normal);
                    if (maxAscender < face.Glyph.Metrics.HorizontalBearingY)
                        maxAscender = face.Glyph.Metrics.HorizontalBearingY;
                    if (maxDescender < face.Glyph.Metrics.Height - face.Glyph.Metrics.HorizontalBearingY)
                        maxDescender = face.Glyph.Metrics.Height - face.Glyph.Metrics.HorizontalBearingY;

                    var found = glyphIndexes.FindIndex(p => p.Value == i);
                    //Glyph 0 is the default glyph for the font and will be used for the requested characters not included in the font
                    if (found != -1 && i != 0)
                    {
                        face.Glyph.RenderGlyph(RenderMode.Normal);
                        glyphList.Add(RetrieveGlyphData(glyphIndexes[found].Key, face));
                        //glyphIndexes.RemoveAt(found);
                    }
                }
                glyphIndexes.RemoveAll(p => p.Value != 0);
                //For each character not included in the font load the glyph with index 0
                face.LoadGlyph(0, LoadFlags.Default, LoadTarget.Normal);
                face.Glyph.RenderGlyph(RenderMode.Normal);
                foreach (var character in glyphIndexes)
                {
                    //var glyph = ImportGlyph(character.Key, face);
                    var glyph = RetrieveGlyphData(character.Key, face);
                    glyphList.Add(glyph);
                }
                Glyphs = glyphList;

                // Store the font height.
                LineSpacing = (maxAscender + maxDescender) >> 6;

                // The height used to calculate the Y offset for each character.
                YOffsetMin = -maxAscender >> 6;
            }
            finally
            {
                if (face != null)
                    face.Dispose();
                if (lib != null)
                {
                    lib.Dispose();
                    lib = null;
                }
            }
        }


        // Attempts to instantiate the requested GDI+ font object.
        private Face CreateFontFace(FontDescription options, string fontName)
        {
            try
            {
                const uint dpi = 96;
                var face = lib.NewFace(fontName, 0);
                var fixedSize = ((int) options.Size) << 6;
                face.SetCharSize(0, fixedSize, dpi, dpi);

                if (face.FamilyName == "Microsoft Sans Serif" && options.FontName != "Microsoft Sans Serif")
                    throw new PipelineException(string.Format("Font {0} is not installed on this computer.", options.FontName));

                return face;

                // A font substitution must have occurred.
                //throw new Exception(string.Format("Can't find font '{0}'.", options.FontName));
            }
            catch
            {
                throw;
            }
        }

        // Rasterizes a single character glyph.
        //private Glyph ImportGlyph(char character, Face face)
        //      {
        //          uint glyphIndex = face.GetCharIndex(character);
        //          face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
        //          face.Glyph.RenderGlyph(RenderMode.Normal);

        //          return RetrieveGlyphData(character, face);
        //      }

        private Glyph RetrieveGlyphData(char character, Face face)
        {
            // Render the character.
            BitmapContent glyphBitmap = null;
            if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0)
            {
                glyphBitmap = new PixelBitmapContent<byte>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                byte[] gpixelAlphas = new byte[face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows];
                //if the character bitmap has 1bpp we have to expand the buffer data to get the 8bpp pixel data
                //each byte in bitmap.bufferdata contains the value of up to 8 pixels in the row
                //if bitmap is of width 10, each row has 2 bytes with 10 valid bits, and the last 6 bits of the 2nd byte must be discarded
                if (face.Glyph.Bitmap.PixelMode == PixelMode.Mono)
                {
                    //variables needed for the expansion, amount of written data, length of the data to write
                    int written = 0, length = face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows;
                    for (int i = 0; written < length; i++)
                    {
                        //width in pixels of each row
                        int width = face.Glyph.Bitmap.Width;
                        while (width > 0)
                        {
                            //valid data in the current byte
                            int stride = MathHelper.Min(8, width);
                            //copy the valid bytes to pixeldata
                            ExpandByteAndCopy(face.Glyph.Bitmap.BufferData[i], stride, gpixelAlphas, written);
                            written += stride;
                            width -= stride;
                            if (width > 0)
                                i++;
                        }
                    }
                }
                else
                    Marshal.Copy(face.Glyph.Bitmap.Buffer, gpixelAlphas, 0, gpixelAlphas.Length);
                glyphBitmap.SetPixelData(gpixelAlphas);
            }

            if (glyphBitmap == null)
            {
                var gHA = face.Glyph.Metrics.HorizontalAdvance >> 6;
                var gVA = face.Size.Metrics.Height >> 6;

                gHA = gHA > 0 ? gHA : gVA;
                gVA = gVA > 0 ? gVA : gHA;

                glyphBitmap = new PixelBitmapContent<byte>(gHA, gVA);
            }

            // not sure about this at all
            var abc = new ABCFloat();
            //Margin to leave empty to the left of the glyph bitmap
            abc.A = face.Glyph.Metrics.HorizontalBearingX >> 6;
            //Width of the glyph bitmap
            abc.B = face.Glyph.Metrics.Width >> 6;
            //Margin to leave empty to the right of the glyph bitmap
            abc.C = (face.Glyph.Metrics.HorizontalAdvance >> 6) - (abc.A + abc.B);

            // Construct the output Glyph object.
            return new Glyph(character, glyphBitmap)
            {
                XOffset = -(face.Glyph.Advance.X >> 6),
                XAdvance = face.Glyph.Metrics.HorizontalAdvance >> 6,
                YOffset = -(face.Glyph.Metrics.HorizontalBearingY >> 6),
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
                tmp = (byte) (1 << i);
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