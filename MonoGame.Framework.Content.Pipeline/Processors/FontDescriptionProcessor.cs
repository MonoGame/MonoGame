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
using System.Text.RegularExpressions;
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
        /// <summary>
        /// Valid font extension
        /// </summary>
        string[] _SupportedFontExtensions=new string[] {".ttf",".otf",".ttc" };
        /// <summary>
        /// Possible filename indicators of a bold font
        /// </summary>
        string[] _BoldFileNameTerminators=new string[] {"bd","b" };
        /// <summary>
        /// Possible filename indicators of an italic font
        /// </summary>
        string[] _ItalicFileNameTerminators=new string[] {"i"};
        /// <summary>
        /// Possible filename indicators of a bold and italic font, when it can't be inferred from the other terminators arrays
        /// </summary>
        string[] _BoldAndItalicFileNameTerminators=new[] { "z" };
        public FontDescriptionProcessor()
        {
            this.TextureFormat = TextureProcessorOutputFormat.Compressed;
        }

        public override SpriteFontContent Process(FontDescription input,
            ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);

			List<string> directories = new List<string>();
			var directory = Path.GetDirectoryName (input.Identity.SourceFilename);
			directories.Add(directory);
			directories.Add("/Library/Fonts");
#if WINDOWS
			var windowsfolder = Environment.GetFolderPath (Environment.SpecialFolder.Windows);
		    var fontDirectory = Path.Combine(windowsfolder,"Fonts");
			directories.Add(fontDirectory);
#endif

			string fontName = FindFontByFontFamily(input.FontName, directories, input.Style);
#if DEBUG
            if (fontName != null)
                context.Logger.LogMessage("Font \"{0}\" for style: {1} found as {2} with SharpFont", input.FontName, input.Style, fontName);
#endif
            if (string.IsNullOrWhiteSpace(fontName))
            {
#if WINDOWS
                fontName = FindFontFileFromFontName(input.FontName, fontDirectory, input.Style);
#elif LINUX
                fontName = FindFontFileFromFontName(input.FontName, input.Style.ToString());
#endif
#if DEBUG
                if (fontName != null)
                    context.Logger.LogMessage("Font \"{0}\" for style: {1} found as {2} in the platform specific procedure", input.FontName, input.Style, fontName);
#endif
            }
            if (string.IsNullOrWhiteSpace(fontName)) {
                fontName = FindFontByFileName(input.FontName, directories, input.Style);
#if DEBUG
                if (fontName != null)
                    context.Logger.LogMessage("Font \"{0}\" for style: {1} found as {2} with filename matching procedure", input.FontName, input.Style, fontName);
#endif
            }


            context.Logger.LogMessage ("Building Font {0}", fontName);
			try {
				if (!File.Exists(fontName)) {
					throw new Exception(string.Format("Could not find any font with name \"{0}\"", input.FontName));
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
				//context.Logger.LogImportantMessage("{0}", ex.ToString());
                throw ex;
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

        string FindFontByFontFamily(string fontname, List<string> fontDirectories, FontDescriptionStyle style)
        {
            var files = fontDirectories.Where(fd => Directory.Exists(fd)).SelectMany<string, string>(fd =>
            Directory.GetFiles(fd).Where(f =>
            _SupportedFontExtensions.Any(e => f.Substring(f.Length - e.Length).Equals(e, StringComparison.OrdinalIgnoreCase))));
            List<Tuple<string, int, StyleFlags>> candidates = new List<Tuple<string, int, StyleFlags>>();
            SharpFont.Library lib = new Library();
            foreach (var font in files)
            {
                Face face = lib.NewFace(font, 0);
                if (face.FamilyName.Equals(fontname, StringComparison.OrdinalIgnoreCase))
                {
                    int faceCount = face.FaceCount;
                    face.Dispose();
                    for (int i = 0; i < faceCount; i++)
                    {
                        face = lib.NewFace(font, i);
                        candidates.Add(new Tuple<string, int, StyleFlags>(font, i, face.StyleFlags));
                        face.Dispose();
                    }
                }
            }
            lib.Dispose();
            //StyleFlags requestedStyle = style.ToStyleFlags();
            var boldCandidates = candidates.Where(c => c.Item3.HasFlag(StyleFlags.Bold));
            var italicCandidates = candidates.Where(c => c.Item3.HasFlag(StyleFlags.Italic));
            bool bold = style.HasFlag(FontDescriptionStyle.Bold) && boldCandidates.Any();
            bool italic = style.HasFlag(FontDescriptionStyle.Italic) && italicCandidates.Any();
            if (bold && italic)
                return boldCandidates.Intersect(italicCandidates).FirstOrDefault().Item1;
            if (italic)
                //Needs to discard fonts that also have bold styles
                return italicCandidates.FirstOrDefault(ic => ic.Item3 == StyleFlags.Italic).Item1;
            if (bold)
                //Needs to discard fonts that also have italic styles
                return boldCandidates.FirstOrDefault(ic => ic.Item3 == StyleFlags.Bold).Item1;
            if (candidates.Any())
                return candidates.FirstOrDefault().Item1;
            return null;
        }

        /// <summary>
        /// Retrieves the path to the font with the requested filename looking for variations for the style
        /// </summary>
        /// <param name="fontname">Filename of the font, without extension</param>
        /// <param name="fontDirectories">List of possible folders that may contain the font</param>
        /// <param name="style">Style of the font</param>
        /// <returns>Full path to the found font file or null.
        /// When selecting the file to return, the function returns the better matches the requested style.
        /// If the requested style is bold and italic, but there's no file that has both styles,
        /// the first option is returning the italic one, then the bold one, and finally the regular one</returns>
        string FindFontByFileName(string fontname, List<string> fontDirectories, FontDescriptionStyle style)
        {
            //If the user typed filename including its extension try to find it, and return what was found.
            if (Path.HasExtension(fontname))
                return fontDirectories.Where(fd => Directory.Exists(fd)).SelectMany(fd => Directory.GetFiles(fd)).FirstOrDefault(f => f.EndsWith(fontname, StringComparison.OrdinalIgnoreCase));

            int maxSuffixLength = Math.Max(_BoldFileNameTerminators.Max(bft => bft.Length) + _ItalicFileNameTerminators.Max(ifn => ifn.Length), _BoldAndItalicFileNameTerminators.Max(bift => bift.Length));
            var files = fontDirectories.Where(fd => Directory.Exists(fd)).SelectMany<string, FileInfo>(fd => Directory.GetFiles(fd). //Get the filenames from each directory
              Select<string, FileInfo>(f => new FileInfo(f))). //Convert them to fileInfo
            Where(fi => _SupportedFontExtensions.Contains(fi.Extension.ToLowerInvariant())).//Exclude the ones with not supported formats
            Where(fi => fi.Name.StartsWith(fontname, StringComparison.OrdinalIgnoreCase) && fi.Name.Length - fi.Extension.Length - fontname.Length <= maxSuffixLength);//Get the ones that begin with fontname and its length is nearly the same
            
            if (style == (FontDescriptionStyle.Bold | FontDescriptionStyle.Italic))
            {
                //At this time only check for bold and italic terminators
                var boldAndItalicFiles = files.Where(fi =>
                 _BoldAndItalicFileNameTerminators.Any(bift =>
                 fi.Name.Substring(fontname.Length, fi.Name.Length - fontname.Length - fi.Extension.Length).ToLowerInvariant().Contains(bift)));
                if (boldAndItalicFiles.Any())
                {
                    return GetShortestNameFromFileInfo(boldAndItalicFiles);
                }
            }
            IEnumerable<FileInfo> boldCandidates=null;
            IEnumerable<FileInfo> italicCandidates=null;
            if (style.HasFlag(FontDescriptionStyle.Bold))
            {
                boldCandidates= files.Where(fi => 
                _BoldFileNameTerminators.Any(bft =>
                fi.Name.Substring(fontname.Length, fi.Name.Length - fontname.Length - fi.Extension.Length).ToLowerInvariant().Contains(bft)));
            }
            if (style.HasFlag(FontDescriptionStyle.Italic))
            {
                italicCandidates=files.Where(fi =>
                _ItalicFileNameTerminators.Any(ift =>
                fi.Name.Substring(fontname.Length, fi.Name.Length - fontname.Length - fi.Extension.Length).ToLowerInvariant().Contains(ift)));
            }
            bool foundBoldCandidates = boldCandidates != null && boldCandidates.Any();
            bool foundItalicCandidates = italicCandidates != null && italicCandidates.Any();
            if (foundBoldCandidates && foundItalicCandidates)
            {
                var crossedCandidates = boldCandidates.Intersect(italicCandidates, new FileInfoComparer() );
                if (crossedCandidates.Any())
                {
                    return GetShortestNameFromFileInfo(crossedCandidates);
                }
            }
            if (foundItalicCandidates)  
            {
                //If there's no font with italics and bold try to return the italic one as it is easier to render
                //a faux bold from italics than the other way around
                return GetShortestNameFromFileInfo(italicCandidates);
            }
            if (foundBoldCandidates)
            {
                return GetShortestNameFromFileInfo(boldCandidates);
            }
            if (files.Any())
                return GetShortestNameFromFileInfo(files);
            return null;
        }

        private static string GetShortestNameFromFileInfo(IEnumerable<FileInfo> candidates)
        {
            //Return the shortest candidate to avoid returning derivates of the font
            return candidates.OrderBy(cc => cc.Name.Length).First().FullName;
        }
#if WINDOWS
        /// <summary>
        /// Searches the given font family name in the installed fonts register
        /// </summary>
        /// <param name="fontName">Name of the font family</param>
        /// <param name="fontDirectory">System directory for fonts</param>
        /// <param name="style">Style of the font to retrieve</param>
        /// <returns>Full path to the found font, or null.
        /// for italics and bold fonts, if there's no font with both styles this returns the italic one.
        /// If there's no italic, the bold one, or the Regular if there's no bold one</returns>
        string FindFontFileFromFontName(string fontName, string fontDirectory, FontDescriptionStyle style)
        {
            const string registrySuffix = " (TrueType)";

            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", false);
            var fonts = key.GetValueNames().Where(k => k.StartsWith(fontName, StringComparison.OrdinalIgnoreCase));
            string fontKey = null;
            if (style == FontDescriptionStyle.Regular)
                fonts = fonts.Where(f => !Regex.IsMatch(f.Substring(fontName.Length), "Italic|Bold", RegexOptions.IgnoreCase));
            else
            {
                IEnumerable<String> boldFonts = null;
                IEnumerable<String> italicFonts = null;
                if (style.HasFlag(FontDescriptionStyle.Bold))
                    boldFonts = fonts.Where(f => Regex.IsMatch(f.Substring(fontName.Length), "Bold", RegexOptions.IgnoreCase));
                if (style.HasFlag(FontDescriptionStyle.Italic))
                    italicFonts = fonts.Where(f => Regex.IsMatch(f.Substring(fontName.Length), "Italic", RegexOptions.IgnoreCase));
                bool foundBoldFonts = boldFonts != null && boldFonts.Any();
                bool foundItalicFonts = italicFonts != null && italicFonts.Any();
                if (foundBoldFonts && foundItalicFonts)
                {
                    var crossedFonts = boldFonts.Intersect(italicFonts);
                    if (crossedFonts.Any())
                        fontKey = crossedFonts.OrderBy(cf => cf.Contains(registrySuffix) ? cf.Length - registrySuffix.Length : cf.Length).First();
                }
                else if (foundItalicFonts)
                    fontKey = italicFonts.OrderBy(i => i.Contains(registrySuffix) ? i.Length - registrySuffix.Length : i.Length).First();
                else if (foundBoldFonts)
                    fontKey = boldFonts.OrderBy(b => b.Contains(registrySuffix) ? b.Length - registrySuffix.Length : b.Length).First();
            }
            if (fontKey == null && fonts.Any())
                fontKey = fonts.OrderBy(f => f.Contains(registrySuffix) ? f.Length - registrySuffix.Length : f.Length).First();
            if (fontKey != null)
            {
                var fontPath = key.GetValue(fontKey).ToString();
                return Path.IsPathRooted(fontPath) ? fontPath : Path.Combine(fontDirectory, fontPath);
            }
            return null;
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
        private class FileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo x, FileInfo y)
            {
               return x.FullName == y.FullName;
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}
