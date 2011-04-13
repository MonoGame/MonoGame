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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using Android.Util;
using OpenTK.Graphics.ES11;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class Texture2DReader : ContentTypeReader<Texture2D>
    {
        internal Texture2DReader()
        {
            // Do nothing
        }

		public static string Normalize(string FileName)
		{
		    int index = FileName.LastIndexOf(Path.DirectorySeparatorChar);
		    string path = string.Empty;
		    string file = FileName;
            if (index >= 0) {
                file = FileName.Substring(index + 1, FileName.Length - index - 1);
                path = FileName.Substring(0, index);
            }
		    string[] files = Game.contextInstance.Assets.List(path);

            if (Contains(file, files))
				return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
		
            return Path.Combine(path, TryFindAnyCased(file, files, ".xnb", ".jpg", ".bmp", ".jpeg", ".png", ".gif"));
		}

        private static string TryFindAnyCased(string search, string[] arr, params string[] extensions)
        {
            return arr.FirstOrDefault(s => extensions.Any(ext => s.ToLower() == (search.ToLower() + ext)));
        }

        private static bool Contains(string search, string[] arr)
        {
            return arr.Any(s => s == search);
        }

        protected internal override Texture2D Read(ContentReader reader, Texture2D existingInstance)
		{
			Texture2D texture = null;
			
			SurfaceFormat surfaceFormat = (SurfaceFormat)reader.ReadInt32 ();
			int width = (reader.ReadInt32 ());
			int height = (reader.ReadInt32 ());
			int levelCount = (reader.ReadInt32 ());
			SetDataOptions compressionType = (SetDataOptions)reader.ReadInt32 ();
			int imageLength = width * height * 4;
			
			if (surfaceFormat == SurfaceFormat.Dxt3)
			{
				ESTexture2D temp = ESTexture2D.InitiFromDxt3File(reader,imageLength,width,height);
				texture = new Texture2D (new ESImage (temp));
			}
			else {
				byte[] imageBytes = reader.ReadBytes (imageLength);
				IntPtr ptr = Marshal.AllocHGlobal (imageLength);
				try 
				{
					Marshal.Copy (imageBytes, 0, ptr, imageLength);					
					ESTexture2D temp = new ESTexture2D (ptr, SurfaceFormat.Rgba32, width, height, new Size (width, height), All.Linear);
					texture = new Texture2D (new ESImage (temp));					
				} 
				finally 
				{		
					Marshal.FreeHGlobal (ptr);
				}
			}
			
			return texture;
		}
    }
}
