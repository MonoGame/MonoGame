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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentTypeReaderManager
    {
        ContentReader _reader;
		
		static string assemblyName;
		
		static ContentTypeReaderManager()
		{
			assemblyName = Assembly.GetExecutingAssembly().FullName;
		}

        public ContentTypeReaderManager(ContentReader reader)
        {
            _reader = reader;
        }

        public ContentTypeReader GetTypeReader(Type targetType)
        {
            foreach (ContentTypeReader r in _reader.TypeReaders)
            {
                if (targetType == r.TargetType) return r;
            }
            return null;
        }
		
		public ContentTypeReader[] LoadAssetReaders(ContentReader reader)
        {			
			// Dummy variables required for it to work on iDevices ** DO NOT DELETE ** 
			// This forces the classes not to be optimized out when deploying to iDevices
			ListReader<Char> hCharListReader = new ListReader<Char>();
			ListReader<Rectangle> hRectangleListReader = new ListReader<Rectangle>();
			ListReader<Vector3> hVector3ListReader = new ListReader<Vector3>();
			ListReader<StringReader> hStringListReader = new ListReader<StringReader>();
			SpriteFontReader hSpriteFontReader = new SpriteFontReader();
			Texture2DReader hTexture2DReader = new Texture2DReader();
			CharReader hCharReader = new CharReader();
			RectangleReader hRectangleReader = new RectangleReader();
			StringReader hStringReader = new StringReader();
			Vector3Reader hVector3Reader = new Vector3Reader();
			CurveReader hCurveReader = new CurveReader();
			
            int numberOfReaders;
            ContentTypeReader[] contentReaders;		
			

            // The first content byte i read tells me the number of content readers in this XNB file
            numberOfReaders = reader.ReadByte();
            contentReaders = new ContentTypeReader[numberOfReaders];
		
            // For each reader in the file, we read out the length of the string which contains the type of the reader,
            // then we read out the string. Finally we instantiate an instance of that reader using reflection
            for (int i = 0; i < numberOfReaders; i++)
            {
                // This string tells us what reader we need to decode the following data
                // string readerTypeString = reader.ReadString();
				string originalReaderTypeString = reader.ReadString();
 
				// Need to resolve namespace differences
				string readerTypeString = originalReaderTypeString;
								
				readerTypeString = PrepareType(readerTypeString);

				Type l_readerType = Type.GetType(readerTypeString);
				
            	if(l_readerType !=null)
					contentReaders[i] = (ContentTypeReader)Activator.CreateInstance(l_readerType,true);
            	else
					throw new ContentLoadException("Could not find matching content reader of type " + originalReaderTypeString + " (" + readerTypeString + ")");
				
				// I think the next 4 bytes refer to the "Version" of the type reader,
                // although it always seems to be zero
                int typeReaderVersion = reader.ReadInt32();
            }

            return contentReaders;
        }
		
		/// <summary>
		/// Removes Version, Culture and PublicKeyToken from a type string.
		/// </summary>
		/// <remarks>
		/// Supports multiple generic types (e.g. Dictionary<TKey,TValue>) and nested generic types (e.g. List<List<int>>).
		/// </remarks> 
		/// <param name="type">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string PrepareType(string type)
		{			
			//Needed to support nested types
			int count = type.Split(new[] {"[["}, StringSplitOptions.None).Length - 1;
			
			string preparedType = type;
			
			for(int i=0; i<count; i++)
			{
				preparedType = Regex.Replace(preparedType, @"\[(.+?), Version=.+?\]", "[$1]");
			}
						
			//Handle non generic types
			if(preparedType.Contains("PublicKeyToken"))
				preparedType = Regex.Replace(preparedType, @"(.+?), Version=.+?$", "$1");
			
			preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Graphics", string.Format(", {0}", assemblyName));
			preparedType = preparedType.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", assemblyName));
			
			return preparedType;
		}
    }
}
