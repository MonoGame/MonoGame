#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
    internal class SpriteFontReader : ContentTypeReader<SpriteFont>
    {
        internal SpriteFontReader()
        {
        }

		public static string Normalize(string FileName)
		{
            if (File.Exists(FileName))
                return FileName;

            // Check the file extension
            if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
            {
                return null;
            }

            // Concat the file name with valid extensions
            if (File.Exists(FileName + ".xnb"))
                return FileName + ".xnb";

            // Concat the file name with valid extensions
            if (File.Exists(FileName + ".spritefont"))
                return FileName + ".spritefont";


            // ATTEMPT TO FIND THE FILE AS A BITMAP FONT STORED IN A PNG 

            if (File.Exists (FileName + ".png")) { return FileName + ".png"; }
			
			return null;
		}

        protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
        {
            Texture2D texture = input.ReadObject<Texture2D>();
			texture.IsSpriteFontTexture = true;
            List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
            List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
            List<char> charMap = input.ReadObject<List<char>>();
            int lineSpacing = input.ReadInt32();
            float spacing = input.ReadSingle();
            List<Vector3> kerning = input.ReadObject<List<Vector3>>();
            char? defaultCharacter = null;
            if (input.ReadBoolean())
            {
                defaultCharacter = new char?(input.ReadChar());
            }
            return new SpriteFont(texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultCharacter);
        }

        /// <summary>
        /// This reads a bitmap font from a texture. Allows for variable length lines and character boxes. 
        ///   Assumes:
        ///     The font is boxed by the standard Magenta color. 
        ///     The characters start with the space (32) character and are contiguous.    
        ///     All lines of characters have the same starting top offset. 
        /// </summary>
        /// <param name="fontTexture">The texture containing the font.</param>
        /// <returns>A valid SpriteFont if successful or NULL.</returns>
        /// <remarks>
        ///     This is used to allow TTF font files to be converted to PNG format and loaded easily into the XNA environment.
        ///     Make sure that you have your SpriteBatch blend state set to "NonPremultiplied" if you store alpha values in the font. 
        ///     This has been tested with the SpriteFont 2.0 tool that converts TTF to PNG.
        ///     The SpriteFontReader Normalize method was updated to check for "png" files. 
        ///     The Content ReadAsset method was updated to support fonts in png format. 
        ///     The only reason that the bmp format is not supported is because of a Access Violation Exception on GetPixel/GetData. 
        /// </remarks>
        protected internal SpriteFont ReadFromTexture (Texture2D fontTexture) {

            SpriteFont font = null; // FONT TO RETURN 


            Color magenta = new Color (255, 0, 255, 255); // MASK COLOR (STANDARD XNA)


            List<Char> characters = new List<char> ();

            List<Rectangle> glyphBounds = new List<Rectangle> ();

            List<Rectangle> croppings = new List<Rectangle> ();

            List<Vector3> kernings = new List<Vector3> ();


            Byte currentCharacterCode = 32; // START CHARACTERS AT SPACE

            Rectangle currentGlyphBounds = new Rectangle (0, 0, 1, 1);


            Int32 currentPositionX = 0;

            Int32 currentPositionY = 0;


            Int32 leadingEdgeTop = 0;

            Int32 leadingEdgeLeft = 0;


            // DETERMINE LEADING EDGE (LEFT AND TOP)

            while (fontTexture.GetPixel (currentPositionX, currentPositionY).Equals (magenta)) {

                currentPositionX = currentPositionX + 1;

                currentPositionY = currentPositionY + 1;


                // CHECK FOR OUT BOUNDS 

                if ((currentPositionX == fontTexture.Width) || (currentPositionY == fontTexture.Height)) {

                    throw new InvalidDataException ("Unable to find starting position for bitmap font.");

                }

            }

            // DETERMINE THE LEFT EDGE 

            while (fontTexture.GetPixel (leadingEdgeLeft, currentPositionY).Equals (magenta)) {

                leadingEdgeLeft = leadingEdgeLeft + 1;

            }


            while (leadingEdgeTop < fontTexture.Height) {

                // DETERMINE LEAD TOP EDGE TO PROCESS CHARACTERS IN LINES 

                while ((leadingEdgeTop < fontTexture.Height) && (fontTexture.GetPixel (leadingEdgeLeft, leadingEdgeTop).Equals (magenta))) {

                    leadingEdgeTop = leadingEdgeTop + 1;

                }

                if (leadingEdgeTop < (fontTexture.Height - 1)) {

                    // A LINE WAS FOUND, PROCESS ALL CHARACTERS IN THE LINE 

                    currentPositionX = leadingEdgeLeft; // SET START OF CHARACTER BOX AT LEFT EDGE 

                    while (currentPositionX < fontTexture.Width) {

                        // FIND LEFT BOUNDARY OF CHARACTER BOX 

                        while ((currentPositionX < fontTexture.Width) && (fontTexture.GetPixel (currentPositionX, leadingEdgeTop).Equals (magenta))) { // LOOKING FOR NON-MAGENTA PIXEL

                            currentPositionX = currentPositionX + 1;

                        }


                        // CHECK TO ENSURE THAT WE DID RUN OUT OF TEXTURE FROM OUR BOX SEARCH 

                        if (currentPositionX >= (fontTexture.Width - 1)) { 

                            currentPositionX = leadingEdgeLeft;

                            break; // EXIT THIS LINE LOOP

                        }

                        currentGlyphBounds.X = currentPositionX;

                        currentGlyphBounds.Y = leadingEdgeTop;


                        // FIND CHARACTER BOX HEIGHT 

                        currentPositionY = leadingEdgeTop;

                        while ((currentPositionY < fontTexture.Height) && (!fontTexture.GetPixel (currentPositionX, currentPositionY).Equals (magenta))) { // LOOKING FOR MAGENTA PIXEL

                            currentPositionY = currentPositionY + 1;

                        }

                        currentGlyphBounds.Height = currentPositionY - currentGlyphBounds.Y;

                        // FIND CHARACTER BOX WIDTH 

                        while ((currentPositionX < fontTexture.Width) && (!fontTexture.GetPixel (currentPositionX, leadingEdgeTop).Equals (magenta))) { // LOOKING FOR MAGENTA PIXEL

                            currentPositionX = currentPositionX + 1;

                        }

                        currentGlyphBounds.Width = currentPositionX - currentGlyphBounds.X;


                        // ADD CHARACTER BOX AND CHARACTER TO FONT 

                        characters.Add ((Char)currentCharacterCode);

                        glyphBounds.Add (currentGlyphBounds);

                        croppings.Add (new Rectangle (0, 0, currentGlyphBounds.Width, currentGlyphBounds.Height));

                        kernings.Add (new Vector3 (0, currentGlyphBounds.Width, 0));

                        currentCharacterCode += 1; // INCREMENT CHARACTER CODE TO PROCESS

                    } // END OF LINE OF CHARACTERS 

                } // END IF LINE FOUND


                // INCREMENT LEADING TOP EDGE BY CHARACTER HEIGHT TO FIND THE NEXT LINE 

                leadingEdgeTop = leadingEdgeTop + currentGlyphBounds.Height;

            } // END OF FONT TEXTURE CHARACTER ROWS 


            // REPLACE MAGENTA WITH TRANSPARENT BACKGROUND 

            Color[] replaceColorData = new Color[fontTexture.Width * fontTexture.Height];

            fontTexture.GetData (replaceColorData);

            for (Int32 currentReplaceIndex = 0; currentReplaceIndex < replaceColorData.Length; currentReplaceIndex++) {

                if (replaceColorData[currentReplaceIndex].Equals (magenta)) {

                    replaceColorData[currentReplaceIndex] = Microsoft.Xna.Framework.Color.Transparent;

                }

            }

            fontTexture.SetData (replaceColorData);


            font = new SpriteFont (fontTexture, glyphBounds, croppings, characters, 0, 2.0f, kernings, null);

            return font;

        }

    }
}
