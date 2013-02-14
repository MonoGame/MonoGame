// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

using System;
using System.IO;

using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content
{
	internal class SongReader : ContentTypeReader<Song>
	{
#if ANDROID
        static string[] supportedExtensions = new string[] { ".mp3", ".ogg", ".mid" };
#else
        static string[] supportedExtensions = new string[] { ".mp3" };
#endif

        internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		protected internal override Song Read(ContentReader input, Song existingInstance)
		{
			var path = input.ReadString();
			
			if (!String.IsNullOrEmpty(path))
			{
#if WINRT
				const char notSeparator = '/';
				const char separator = '\\';
#else
				const char notSeparator = '\\';
				var separator = Path.DirectorySeparatorChar;
#endif
				path = path.Replace(notSeparator, separator);
				
				// Get a uri for the asset path using the file:// schema and no host
				var src = new Uri("file:///" + input.AssetName.Replace(notSeparator, separator));
				
				// Add the relative path to the external reference
				var dst = new Uri(src, path);
				
				// The uri now contains the path to the external reference within the content manager
				// Get the local path and skip the first character (the path separator)
				path = dst.LocalPath.Substring(1);
				
				// Adds the ContentManager's RootDirectory
                path = Path.Combine(input.ContentManager.RootDirectoryFullPath, path);
			}
			
			var durationMs = input.ReadObject<int>();

            return new Song(path, durationMs); 
		}
	}
}
