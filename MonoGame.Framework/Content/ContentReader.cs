// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Content Reader class, inherits from Binary Reader class
    /// </summary>
    public sealed class ContentReader : BinaryReader
    {
        private ContentManager contentManager;
        private Action<IDisposable> recordDisposableObject;
        private ContentTypeReaderManager typeReaderManager;
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

        internal ContentReader(ContentManager manager, Stream stream, string assetName, int version, Action<IDisposable> recordDisposableObject)
            : base(stream)
        {
            this.recordDisposableObject = recordDisposableObject;
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
            InitializeTypeReaders();

            // Read primary object
            object result = ReadObject<T>();

            // Read shared resources
            ReadSharedResources();
            
            return result;
        }

        internal object ReadAsset<T>(T existingInstance)
        {
            InitializeTypeReaders();

            // Read primary object
            object result = ReadObject<T>(existingInstance);

            // Read shared resources
            ReadSharedResources();

            return result;
        }

        internal void InitializeTypeReaders()
        {
            typeReaderManager = new ContentTypeReaderManager();
            typeReaders = typeReaderManager.LoadAssetReaders(this);
            sharedResourceCount = Read7BitEncodedInt();
            sharedResourceFixups = new List<KeyValuePair<int, Action<object>>>();
        }

        internal void ReadSharedResources()
        {
            if (sharedResourceCount <= 0)
                return;

            var sharedResources = new object[sharedResourceCount];
            for (var i = 0; i < sharedResourceCount; ++i)
                sharedResources[i] = InnerReadObject<object>(null);

            // Fixup shared resources by calling each registered action
            foreach (var fixup in sharedResourceFixups)
                fixup.Value(sharedResources[fixup.Key]);
        }

        /// <summary>
        /// Returns the external file reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns> Returns ContentReader</returns>
        public T ReadExternalReference<T>()
        {
            var externalReference = ReadString();

            if (!String.IsNullOrEmpty(externalReference))
            {
                return contentManager.Load<T>(FileHelpers.ResolveRelativePath(assetName, externalReference));
            }

            return default(T);
        }

        /// <summary>
        /// Reads all the rows and columns from the matrix
        /// </summary>
        /// <returns> Matrix Result </returns>
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
            
        private void RecordDisposable<T>(T result)
        {
            var disposable = result as IDisposable;
            if (disposable == null)
                return;

            if (recordDisposableObject != null)
                recordDisposableObject(disposable);
            else
                contentManager.RecordDisposable(disposable);
        }

        public T ReadObject<T>()
        {
            return InnerReadObject(default(T));
        }

        public T ReadObject<T>(ContentTypeReader typeReader)
        {
            var result = (T)typeReader.Read(this, default(T));            
            RecordDisposable(result);
            return result;
        }

        public T ReadObject<T>(T existingInstance)
        {
            return InnerReadObject(existingInstance);
        }

        private T InnerReadObject<T>(T existingInstance)
        {
            var typeReaderIndex = Read7BitEncodedInt();
            if (typeReaderIndex == 0)
                return existingInstance;

            if (typeReaderIndex > typeReaders.Length)
                throw new ContentLoadException("Incorrect type reader index found!");

            var typeReader = typeReaders[typeReaderIndex - 1];
            var result = (T)typeReader.Read(this, existingInstance);

            RecordDisposable(result);

            return result;
        }

        /// <summary>
        /// Reads 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeReader"></param>
        /// <param name="existingInstance"></param>
        /// <returns></returns>
        public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            if (!ReflectionHelpers.IsValueType(typeReader.TargetType))
                return ReadObject(existingInstance);

            var result = (T)typeReader.Read(this, existingInstance);

            RecordDisposable(result);

            return result;
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
            return (T)ReadRawObject<T>(typeReader, default(T));
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

        /// <summary>
        /// Reads X and Y from stream and sets to the Vector
        /// </summary>
        /// <returns> Vector containing X and Y </returns>
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

        /// <summary>
        /// Reads the color in RGBA format
        /// </summary>
        /// <returns> Returns the color in Color type</returns>
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
