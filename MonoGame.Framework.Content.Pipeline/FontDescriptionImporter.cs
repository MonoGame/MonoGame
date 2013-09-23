// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Drawing;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
	/// <summary>
	/// Provides methods for reading .spritefont files for use in the Content Pipeline.
	/// </summary>
	[ContentImporter(".spritefont", DisplayName = "Sprite Font Importer - MonoGame", DefaultProcessor = "FontDescriptionProcessor")]
	public class FontDescriptionImporter : ContentImporter<FontDescription>
	{
		/// <summary>
		/// Initializes a new instance of FontDescriptionImporter.
		/// </summary>
		public FontDescriptionImporter()
		{
		}

		/// <summary>
		/// Called by the XNA Framework when importing a .spritefont file to be used as a game asset. This is the method called by the XNA Framework when an asset is to be imported into an object that can be recognized by the Content Pipeline.
		/// </summary>
		/// <param name="filename">Name of a game asset file.</param>
		/// <param name="context">Contains information for importing a game asset, such as a logger interface.</param>
		/// <returns>Resulting game asset.</returns>
		public override FontDescription Import(string filename, ContentImporterContext context)
		{
			var xmldoc = XElement.Load(filename, LoadOptions.PreserveWhitespace);
			xmldoc = xmldoc.Element("Asset");

			var fontName = xmldoc.Element("FontName").Value;
			var fontSize = float.Parse(xmldoc.Element("Size").Value);
			var spacing = float.Parse(xmldoc.Element("Spacing").Value);
			var useKerning = false;
			var useKerningElement = xmldoc.Element ("UseKerning");
			if (useKerningElement != null)
				useKerning = bool.Parse(useKerningElement.Value);

			FontDescriptionStyle style = FontDescriptionStyle.Regular;
			var styleElement = xmldoc.Element ("Style");
			if (styleElement != null) {
				var styleVal = styleElement.Value;

				if (styleVal.Contains ("Bold") && styleVal.Contains ("Italic"))
					style = FontDescriptionStyle.Bold | FontDescriptionStyle.Italic;
				else
					style = (FontDescriptionStyle)Enum.Parse (typeof (FontDescriptionStyle), styleVal, false);
			}

			char? defaultCharacter = null;
			var defChar = xmldoc.Element("DefaultCharacter");
			if (defChar != null)
				defaultCharacter = defChar.Value[0];

			var characterRegions = new List<CharacterRegion> ();

			foreach (var region in xmldoc.Descendants("CharacterRegion"))
			{
				var Start = (int)region.Element("Start").Value[0];
				var End = (int)region.Element("End").Value[0];

				characterRegions.Add(new CharacterRegion((char)Start, (char)End));
			}

			var fontDescription = new FontDescription(fontName, fontSize, spacing, style, useKerning, characterRegions);
			fontDescription.DefaultCharacter = defaultCharacter;
			fontDescription.Identity = new ContentIdentity (filename);
			return fontDescription;
		}
	}
}
