// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Content
{
	internal class SongReader : ContentTypeReader<Song>
	{
		protected internal override Song Read(ContentReader input, Song existingInstance)
		{
			var path = input.ReadString();
			
			if (!String.IsNullOrEmpty(path))
			{
                // Add the ContentManager's RootDirectory
                var dirPath = Path.Combine(input.ContentManager.RootDirectoryFullPath, input.AssetName);

                // Resolve the relative path
                path = FileHelpers.ResolveRelativePath(dirPath, path);
			}
			
			var durationMs = input.ReadObject<int>();

            return new Song(path, durationMs); 
		}
	}
}
