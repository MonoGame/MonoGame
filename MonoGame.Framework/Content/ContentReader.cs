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
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentReader : BinaryReader
    {
        private ContentManager contentManager;
        private GraphicsDevice graphicsDevice;

        public ContentTypeReader[] TypeReaders
        {
            get;
            set;
        }

        internal GraphicsDevice GraphicsDevice
        {
            get { return this.graphicsDevice; }
        }

        internal ContentReader(ContentManager manager, Stream stream, GraphicsDevice graphicsDevice)
            : base(stream)
        {
            this.graphicsDevice = graphicsDevice;
			this.contentManager = manager;
        }

        public ContentManager ContentManager
        {
            get { return this.contentManager; }
        }
		
		public string AssetName
		{
			get
			{
				if(BaseStream is FileStream) {

					// Get the full name
					var name  = ((FileStream)(this.BaseStream)).Name;
					// get the directory name
					var directory = Path.GetDirectoryName(name);
					// strip off the Content/
					var index = directory.LastIndexOf("Content/");
					directory = directory.Substring(index + 8);
					// We then put them back together again without the extension
					name = Path.Combine(directory, Path.GetFileNameWithoutExtension(name));
					return name;
					//return ((FileStream)(this.BaseStream)).Name.Replace("Content/","");
				}
				else
					return string.Empty;
			}
		}

        public T ReadExternalReference<T>()
        {
            throw new NotImplementedException();
        }
		
        public Matrix ReadMatrix()
        {
            Matrix result = new Matrix();
            result.M11 = ReadSingle();
            result.M12 = ReadSingle();
            result.M13 = ReadSingle();
            result.M14 = ReadSingle(); 
            result.M21 = ReadSingle();
            result.M22 = ReadSingle();
            result.M23 = ReadSingle();
            result.M24 = ReadSingle();
            result.M31 = ReadSingle();
            result.M32 = ReadSingle();
            result.M33 = ReadSingle();
            result.M34 = ReadSingle();
            result.M41 = ReadSingle();
            result.M42 = ReadSingle();
            result.M43 = ReadSingle();
            result.M44 = ReadSingle();
            return result;
        }
			
        public T ReadObject<T>()
        {			
			int typeReaderIndex = Read7BitEncodedInt();
        
			if (typeReaderIndex == 0) 
				return default(T);
            				
            return (T)TypeReaders[typeReaderIndex - 1].Read(this, default(T));
		}

        public T ReadObject<T>(ContentTypeReader typeReader)
        {
            return (T)typeReader.Read(this, default(T));
        }

        public T ReadObject<T>(T existingInstance)
        {
            throw new NotImplementedException();
        }

        public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
			if (!typeReader.TargetType.IsValueType)
				return (T) ReadObject<object>();
			else
            return (T)typeReader.Read(this, existingInstance);
        }

        public Quaternion ReadQuaternion()
        {
            throw new NotImplementedException();
        }

        public T ReadRawObject<T>()
        {
            throw new NotImplementedException();
        }

        public T ReadRawObject<T>(ContentTypeReader typeReader)
        {
            throw new NotImplementedException();
        }

        public T ReadRawObject<T>(T existingInstance)
        {
            Type objectType = typeof(T);
			foreach(ContentTypeReader typeReader in TypeReaders)
			{
				if(typeReader.TargetType == objectType)
					return (T)ReadRawObject<T>(typeReader,existingInstance);
			}
            throw new NotSupportedException();
        }

        public T ReadRawObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
		return (T)typeReader.Read(this, existingInstance);
        }

        public void ReadSharedResource<T>(Action<T> fixup)
        {
            throw new NotImplementedException();
        }

        public Vector2 ReadVector2()
        {
            Vector2 result = new Vector2();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            return result;
        }

        public Vector3 ReadVector3()
        {
            Vector3 result = new Vector3();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            return result;
        }
    }
}
