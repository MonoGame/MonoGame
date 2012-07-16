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
using System.IO;
using System.Text;
#if WINRT
using System.Reflection;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    public sealed class ContentReader : BinaryReader
    {
        private ContentManager contentManager;
        private ContentTypeReaderManager typeReaderManager;
        private GraphicsDevice graphicsDevice;
        private string assetName;
        private List<KeyValuePair<int, Action<object>>> sharedResourceFixups;
        private ContentTypeReader[] typeReaders;
		internal int version;
		internal int sharedResourceCount;

        internal ContentTypeReader[] TypeReaders
        {
            get
            {
                return typeReaders;
            }
        }

        internal GraphicsDevice GraphicsDevice
        {
            get
            {
                return this.graphicsDevice;
            }
        }

        internal ContentReader(ContentManager manager, Stream stream, GraphicsDevice graphicsDevice, string assetName, int version)
            : base(stream)
        {
            this.graphicsDevice = graphicsDevice;
            this.contentManager = manager;
            this.assetName = assetName;
			this.version = version;
        }

        public ContentManager ContentManager
        {
            get
            {
                return contentManager;
            }
        }
        
        public string AssetName
        {
            get
            {
                return assetName;
            }
        }

        internal object ReadAsset<T>()
        {
            object result = null;

            typeReaderManager = new ContentTypeReaderManager(this);
            typeReaders = typeReaderManager.LoadAssetReaders();
            foreach (ContentTypeReader r in typeReaders)
            {
                r.Initialize(typeReaderManager);
            }

            sharedResourceCount = Read7BitEncodedInt();
            sharedResourceFixups = new List<KeyValuePair<int, Action<object>>>();

            // Read primary object
            int index = Read7BitEncodedInt();
            if (index > 0)
            {
                ContentTypeReader contentReader = typeReaders[index - 1];
                result = ReadObject<T>(contentReader);
            }

            // Read shared resources
            if (sharedResourceCount > 0)
            {
                ReadSharedResources(sharedResourceCount);
            }

            return result;
        }

        void ReadSharedResources(int sharedResourceCount)
        {
            object[] sharedResources = new object[sharedResourceCount];
            for (int i = 0; i < sharedResourceCount; ++i)
            {
                int index = Read7BitEncodedInt();
                if (index > 0)
                {
                    ContentTypeReader contentReader = typeReaders[index - 1];
                    sharedResources[i] = ReadObject<object>(contentReader);
                }
                else
                {
                    sharedResources[i] = null;
                }
            }
            // Fixup shared resources by calling each registered action
            foreach (KeyValuePair<int, Action<object>> fixup in sharedResourceFixups)
            {
                fixup.Value(sharedResources[fixup.Key]);
            }
        }

        public T ReadExternalReference<T>()
		{
            var externalReference = ReadString();
			
            if (!String.IsNullOrEmpty(externalReference))
            {
#if WINRT
                var notSeparator = '/';
                var separator = '\\';

                externalReference = externalReference.Replace(notSeparator, separator);

                var fullAssetName = assetName.Replace(notSeparator, separator);
                var pathDirectory = Path.GetDirectoryName(fullAssetName);
                var fullAssetPath = Path.Combine(pathDirectory, externalReference);

                // HACK: This is the only way i can find of normalizing/canonicalizing paths
                // in WinRT.  We should look for a better method in the upcoming updates.
                {
                    var package = Windows.ApplicationModel.Package.Current;
                    fullAssetPath = Path.Combine(package.InstalledLocation.Path, fullAssetPath);
                    fullAssetPath = new Uri(fullAssetPath).LocalPath;
                    fullAssetPath = fullAssetPath.Substring(package.InstalledLocation.Path.Length + 1);
                }

                return contentManager.Load<T>(fullAssetPath);
#else
                externalReference = externalReference.Replace('\\', Path.DirectorySeparatorChar);

                // Use Path.GetFullPath to help resolve relative directories
                string fullRootPath = Path.GetFullPath(contentManager.RootDirectory);
				
				// iOS won't find the right name if the \'s are facing the wrong way. be certian we're good here.
				var fullAssetName = Path.Combine(fullRootPath, assetName.Replace('\\', Path.DirectorySeparatorChar)); 
				var pathDirectory = Path.GetDirectoryName(fullAssetName);
				var dirExtCombined = Path.Combine(pathDirectory, externalReference);
				
                string fullAssetPath = Path.GetFullPath(dirExtCombined);

#if ANDROID || PSS
                string externalAssetName = fullAssetPath.Substring(fullRootPath.Length);
#else				
                string externalAssetName = fullAssetPath.Substring(fullRootPath.Length + 1);
#endif
                return contentManager.Load<T>(externalAssetName);
#endif
            }

            return default(T);
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
                            
            return (T)typeReaders[typeReaderIndex - 1].Read(this, default(T));
        }

        public T ReadObject<T>(ContentTypeReader typeReader)
        {
            return (T)typeReader.Read(this, default(T));
        }

        public T ReadObject<T>(T existingInstance)
        {
            ContentTypeReader typeReader = typeReaderManager.GetTypeReader(typeof(T));
            if (typeReader != null)
            {
                return (T)typeReader.Read(this, existingInstance);
            }
            throw new ContentLoadException(String.Format("Could not read object type " + typeof(T).Name));
        }

        public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
#if WINRT
            if (!typeReader.TargetType.GetTypeInfo().IsValueType)
#else
            if (!typeReader.TargetType.IsValueType)
#endif
                return (T)ReadObject<object>();
            return (T)typeReader.Read(this, existingInstance);
        }

        public Quaternion ReadQuaternion()
        {
            Quaternion result = new Quaternion();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            result.W = ReadSingle();
            return result;
        }

        public T ReadRawObject<T>()
        {
			return (T)ReadRawObject<T> (default(T));
        }

        public T ReadRawObject<T>(ContentTypeReader typeReader)
        {
            throw new NotImplementedException();
        }

        public T ReadRawObject<T>(T existingInstance)
        {
            Type objectType = typeof(T);
            foreach(ContentTypeReader typeReader in typeReaders)
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
            int index = Read7BitEncodedInt();
            if (index > 0)
            {
                sharedResourceFixups.Add(new KeyValuePair<int, Action<object>>(index - 1, delegate(object v)
                    {
                        if (!(v is T))
                        {
                            throw new ContentLoadException(String.Format("Error loading shared resource. Expected type {0}, received type {1}", typeof(T).Name, v.GetType().Name));
                        }
                        fixup((T)v);
                    }));
            }
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

        public Vector4 ReadVector4()
        {
            Vector4 result = new Vector4();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            result.W = ReadSingle();
            return result;
        }

        public Color ReadColor()
        {
            Color result = new Color();
            result.R = ReadByte();
            result.G = ReadByte();
            result.B = ReadByte();
            result.A = ReadByte();
            return result;
        }

        internal new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }
		
		internal BoundingSphere ReadBoundingSphere()
		{
			var position = ReadVector3();
            var radius = ReadSingle();
            return new BoundingSphere(position, radius);
		}
    }
}
