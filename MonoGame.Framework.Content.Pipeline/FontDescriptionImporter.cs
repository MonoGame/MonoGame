// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

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
	        FontDescription fontDescription = null;

	        using (var input = XmlReader.Create(filename))
	            fontDescription = IntermediateSerializer.Deserialize<FontDescription>(input, filename);

	        fontDescription.Identity = new ContentIdentity(new FileInfo(filename).FullName, "FontDescriptionImporter");

	        return fontDescription;
	    }
	}
}
