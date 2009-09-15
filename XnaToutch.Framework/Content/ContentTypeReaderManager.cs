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
using System.Reflection;
using System.Text;

namespace XnaTouch.Framework.Content
{
    public sealed class ContentTypeReaderManager
    {
        ContentReader _reader;

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
            int numberOfReaders;
            ContentTypeReader[] contentReaders;
            // The first 4 bytes should be the "XNBw" header. i use that to detect an invalid file

            byte[] headerBuffer = new byte[4];
            reader.Read(headerBuffer, 0, 4);
            string headerString = Encoding.UTF8.GetString(headerBuffer, 0, 4);
            if (string.Compare(headerString, "XNBw", StringComparison.InvariantCultureIgnoreCase) != 0)
                throw new ContentLoadException("Asset does not appear to be a valid XNB file.  Did you process your content for Windows Live not XBOX?");

            // I think these two bytes are some kind of version number. Either for the XNB file or the type readers
            byte version = reader.ReadByte();
            byte compressed = reader.ReadByte();
            // The next int32 is the length of the XNB file
            int xnbLength = reader.ReadInt32();

            if (compressed != 0)
            {
                throw new NotImplementedException("XnaTouch cannot read compressed XNB files. Please use the XNB files from the Debug build of your XNA game instead. If someone wants to contribute decompression logic, that would be fantastic.");
            }

            // The next byte i read tells me the number of content readers in this XNB file
            numberOfReaders = reader.ReadByte();
            contentReaders = new ContentTypeReader[numberOfReaders];
		
            // For each reader in the file, we read out the length of the string which contains the type of the reader,
            // then we read out the string. Finally we instantiate an instance of that reader using reflection
            for (int i = 0; i < numberOfReaders; i++)
            {
                // This string tells us what reader we need to decode the following data
                string readerTypeString = reader.ReadString();

                // I think the next 4 bytes refer to the "Version" of the type reader,
                // although it always seems to be zero
                int typeReaderVersion = reader.ReadInt32();
 
				if (readerTypeString.IndexOf("Texture2DReader") != -1)
				{
					contentReaders[i] = new Texture2DReader();
				}
				if (readerTypeString.IndexOf("SpriteFontReader") != -1)
				{
					contentReaders[i] = new SpriteFontReader();
				}
				if (readerTypeString.IndexOf("CharReader") != -1)
				{
					contentReaders[i] = new CharReader();
				}
				if (readerTypeString.IndexOf("RectangleReader") != -1)
				{
					contentReaders[i] = new RectangleReader();
				}
				if (readerTypeString.IndexOf("ListReader") != -1)
				{
					// Ok..i need fix this later
					if (readerTypeString.IndexOf("Rectangle")!= -1)
					{
						contentReaders[i] = new ListReader<Rectangle>();
					}
					
					if (readerTypeString.IndexOf("Vector3") != -1)
					{
						contentReaders[i] = new ListReader<Vector3>();
					}
					
					if (readerTypeString.IndexOf("Char") != -1)
					{
						contentReaders[i] = new ListReader<Char>();
					}
				}
				if (readerTypeString.IndexOf("StringReader") != -1)
				{
					contentReaders[i] = new StringReader();
				}
				if (readerTypeString.IndexOf("Vector3Reader") != -1)
				{
					contentReaders[i] = new Vector3Reader();
				}
            }

            return contentReaders;
        }

    }
}
