// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.ComponentModel;
using System.Linq;
using SharpFont;
using System.Runtime.InteropServices;
using MonoGame.Framework.Content.Pipeline.Builder;
using Glyph = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Glyph;
#if WINDOWS
using Microsoft.Win32;
#endif

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        [DefaultValue(typeof(TextureProcessorOutputFormat), "Compressed")]
        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public FontDescriptionProcessor()
        {
            this.TextureFormat = TextureProcessorOutputFormat.Compressed;
        }

        public override SpriteFontContent Process(FontDescription input,
            ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);

			var fontName = input.FontName;

#if WINDOWS || LINUX
#if WINDOWS
			var windowsfolder = Environment.GetFolderPath (Environment.SpecialFolder.Windows);
		    var fontDirectory = Path.Combine(windowsfolder,"Fonts");
			fontName = FindFontFileFromFontName (fontName, fontDirectory);
#elif LINUX
            fontName = FindFontFileFromFontName(fontName, input.Style.ToString());
#endif
			if (string.IsNullOrWhiteSpace(fontName)) {
				fontName = input.FontName;
#endif
				
			var directory = Path.GetDirectoryName (input.Identity.SourceFilename);

			List<string> directories = new List<string>();
			directories.Add(directory);
			directories.Add("/Library/Fonts");
#if WINDOWS
			directories.Add(fontDirectory);
#endif

			foreach( var dir in directories) {
				if (File.Exists(Path.Combine(dir,fontName+".ttf"))) {
					fontName += ".ttf";
					directory = dir;
					break;
				}
				if (File.Exists (Path.Combine(dir,fontName+".ttc"))) {
					fontName += ".ttc";
					directory = dir;
					break;
				}
				if (File.Exists(Path.Combine(dir,fontName+".otf"))) {
					fontName += ".otf";
					directory = dir;
					break;
				}
            }

            fontName = Path.Combine (directory, fontName);

#if WINDOWS || LINUX
			}
#endif

			context.Logger.LogMessage ("Building Font {0}", fontName);
			try {
				if (!File.Exists(fontName)) {
					throw new Exception(string.Format("Could not load {0}", fontName));
				}
				var lineSpacing = 0f;
				int yOffsetMin = 0;
				var glyphs = ImportFont(input, out lineSpacing, out yOffsetMin, context, fontName);

				// Optimize.
				foreach (Glyph glyph in glyphs)
				{
					GlyphCropper.Crop(glyph);
				}

                var format = GraphicsUtil.GetTextureFormatForPlatform(TextureFormat, context.TargetPlatform);
                var requiresPOT = GraphicsUtil.RequiresPowerOfTwo(format, context.TargetPlatform, context.TargetProfile);
                var requiresSquare = GraphicsUtil.RequiresSquare(format, context.TargetPlatform);

                var face = GlyphPacker.ArrangeGlyphs(glyphs, requiresPOT, requiresSquare);

				// Adjust line and character spacing.
				lineSpacing += input.Spacing;
				output.VerticalLineSpacing = (int)lineSpacing;

				foreach (var glyph in glyphs)
				{
                    output.CharacterMap.Add(glyph.Character);

					var texRect = new Rectangle(glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, glyph.Subrect.Height);
					output.Glyphs.Add(texRect);

					var cropping = new Rectangle(0, (int)(glyph.YOffset - yOffsetMin), (int)glyph.XAdvance, output.VerticalLineSpacing);
					output.Cropping.Add(cropping);

					// Set the optional character kerning.
					if (input.UseKerning)
						output.Kerning.Add(new Vector3(glyph.CharacterWidths.A, glyph.CharacterWidths.B, glyph.CharacterWidths.C));
					else
						output.Kerning.Add(new Vector3(0, texRect.Width, 0));
				}

                output.Texture.Faces[0].Add(face);

                if (GraphicsUtil.IsCompressedTextureFormat(format))
                {
                    GraphicsUtil.CompressTexture(context.TargetProfile, output.Texture, format, context, false, true);
                }
			}
			catch(Exception ex) {
				context.Logger.LogImportantMessage("{0}", ex.ToString());
			}

            return output;
        }

		static Glyph[] ImportFont(FontDescription options, out float lineSpacing, out int yOffsetMin, ContentProcessorContext context, string fontName)
		{
			// Which importer knows how to read this source font?
			IFontImporter importer;

			var TrueTypeFileExtensions = new List<string> { ".ttf", ".ttc", ".otf" };
			//var BitmapFileExtensions = new List<string> { ".bmp", ".png", ".gif" };

			string fileExtension = Path.GetExtension(fontName).ToLowerInvariant();

			//			if (BitmapFileExtensions.Contains(fileExtension))
			//			{
			//				importer = new BitmapImporter();
			//			}
			//			else
			//			{
			if (!TrueTypeFileExtensions.Contains(fileExtension)) 
                throw new PipelineException("Unknown file extension " + fileExtension);

			importer = new SharpFontImporter();

			// Import the source font data.
			importer.Import(options, fontName);

			lineSpacing = importer.LineSpacing;
			yOffsetMin = importer.YOffsetMin;

			// Get all glyphs
			var glyphs = new List<Glyph>(importer.Glyphs);

			// Validate.
			if (glyphs.Count == 0)
			{
				throw new Exception("Font does not contain any glyphs.");
			}

			// Sort the glyphs
			glyphs.Sort((left, right) => left.Character.CompareTo(right.Character));


			// Check that the default character is part of the glyphs
			if (options.DefaultCharacter != null)
			{
				bool defaultCharacterFound = false;
				foreach (var glyph in glyphs)
				{
					if (glyph.Character == options.DefaultCharacter)
					{
						defaultCharacterFound = true;
						break;
					}
				}
				if (!defaultCharacterFound)
				{
					throw new InvalidOperationException("The specified DefaultCharacter is not part of this font.");
				}
			}

			return glyphs.ToArray();
		}

#if WINDOWS
		string FindFontFileFromFontName (string fontName, string fontDirectory)
		{
			var key = Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
			foreach (var font in key.GetValueNames ().OrderBy (x => x)) {
				if (font.StartsWith (fontName, StringComparison.OrdinalIgnoreCase)) {
					var fontPath = key.GetValue (font).ToString ();
					return Path.IsPathRooted (fontPath) ? fontPath : Path.Combine (fontDirectory, fontPath);
				}
			}
			return String.Empty;
		}
#endif

#if LINUX
        string FindFontFileFromFontName(string fontname, string style)
        {
            string s, e;
            ExternalTool.Run("/bin/bash", string.Format ("-c \"fc-match -f '%{{file}}:%{{family}}\\n' '{0}:style={1}'\"", fontname, style), out s, out e);
            s = s.Trim();

            var split = s.Split (':');
            //check font family, fontconfig might return a fallback
            if (split [1].Contains (",")) { //this file defines multiple family names
                var families = split [1].Split (',');
                foreach (var f in families) {
                    if (f.ToLowerInvariant () == fontname.ToLowerInvariant ())
                        return split [0];
                }
                //didn't find it
                return String.Empty;
            } else {
                if (split [1].ToLowerInvariant () != fontname.ToLowerInvariant ())
                    return String.Empty;
            }
            return split [0];
        }
#endif
    }
}
