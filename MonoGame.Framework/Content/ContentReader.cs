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
    /// Defines a worker object that implements most of <see cref="ContentManager.Load{T}(string)">ContentManger.Load</see>.
    /// A new content reader is constructed for each asset loaded.
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

        /// <summary>
        /// Gets a reference to the <see cref="ContentManager"/> instance that is using this content reader.
        /// </summary>
        public ContentManager ContentManager
        {
            get
            {
                return contentManager;
            }
        }

        /// <summary>
        /// Gets the name of the asset currently being read by this content reader.
        /// </summary>
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
        /// Reads a relative link to an external file from the underlying stream and returns an asset of type
        /// <typeparamref name="T"/> loaded from the external file.
        /// </summary>
        /// <typeparam name="T">The type of asset to expected to be loaded from the external file.</typeparam>
        /// <returns>
        /// The asset of type <typeparamref name="T"/> loaded from the external file at the relative link read from
        /// the underlying stream.  If the relative link read was null or an empty string, then the default
        /// implementation of type <typeparamref name="T"/> is returned.
        /// </returns>
        /// <exception cref="ArgumentNullException">The relative link to the external file read is null or an empty string.</exception>
        /// <exception cref="ObjectDisposedException">
        /// This was called after the <see cref="ContentManager">ContentReader.ContentManager</see> was disposed.
        ///
        /// -or
        ///
        /// The stream is closed.
        /// </exception>
        /// <exception cref="ContentLoadException">
        /// The type of asset in the external file does not match the type specified by <typeparamref name="T"/>.
        /// 
        /// -or-
        ///
        /// A content file matching the relative link to the external file could not be found.
        ///
        /// -or-
        ///
        /// The specified path in the relative link to the external file read is invalid (for example, a directory in
        /// the path does not exist).
        ///
        /// -or-
        ///
        /// An error occurred while opening the external file.
        /// </exception>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
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
        /// Reads a <see cref="Matrix"/> value from the underlying stream.
        /// </summary>
        /// <returns>The <see cref="Matrix"/> that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
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

        /// <summary>
        /// Reads a single managed object from the underlying stream as an instance of the specified type. Can be
        /// called recursively.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <returns>The object that was read.</returns>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="FormatException">The stream is corrupted.</exception>
        /// <exception cref="ContentLoadException">Type reader index read from stream is out of bounds</exception>
        public T ReadObject<T>()
        {
            return InnerReadObject(default(T));
        }

        /// <summary>
        /// Reads a single managed object from the underlying stream as an instance of the specified type. Can be
        /// called recursively.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="typeReader">The <see cref="ContentTypeReader"/> to use to read the object.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="FormatException">The stream is corrupted.</exception>
        /// <exception cref="ContentLoadException">Type reader index read from stream is out of bounds</exception>
        public T ReadObject<T>(ContentTypeReader typeReader)
        {
            var result = (T)typeReader.Read(this, default(T));            
            RecordDisposable(result);
            return result;
        }

        /// <summary>
        /// Reads a single managed object from the underlying stream as an instance of the specified type. Can be
        /// called recursively.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="existingInstance">An existing object to write into.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="FormatException">The stream is corrupted.</exception>
        /// <exception cref="ContentLoadException">Type reader index read from stream is out of bounds</exception>
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
        /// Reads a single managed object from the underlying stream as an instance of the specified type. Can be
        /// called recursively.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="typeReader">The <see cref="ContentTypeReader"/> to use to read the object.</param>
        /// <param name="existingInstance">An existing object to write into.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="FormatException">The stream is corrupted.</exception>
        /// <exception cref="ContentLoadException">Type reader index read from stream is out of bounds</exception>
        public T ReadObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            if (!ReflectionHelpers.IsValueType(typeReader.TargetType))
                return ReadObject(existingInstance);

            var result = (T)typeReader.Read(this, existingInstance);

            RecordDisposable(result);

            return result;
        }

        /// <summary>
        /// Reads a <see cref="Quaternion"/> from the underlying stream.
        /// </summary>
        /// <returns>The <see cref="Quaternion"/> that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public Quaternion ReadQuaternion()
        {
            Quaternion result = new Quaternion();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            result.W = ReadSingle();
            return result;
        }

        /// <summary>
        /// Reads a single managed object from the current stream as an instance of the specified type.  If you specify
        /// a base class of the actual object type, this method reads data only from the base type.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <returns>The object that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public T ReadRawObject<T>()
        {
			return (T)ReadRawObject<T> (default(T));
        }

        /// <summary>
        /// Reads a single managed object from the current stream as an instance of the specified type.  If you specify
        /// a base class of the actual object type, this method reads data only from the base type.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="typeReader">The <see cref="ContentTypeReader"/> to use to read the object.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public T ReadRawObject<T>(ContentTypeReader typeReader)
        {
            return (T)ReadRawObject<T>(typeReader, default(T));
        }

        /// <summary>
        /// Reads a single managed object from the current stream as an instance of the specified type.  If you specify
        /// a base class of the actual object type, this method reads data only from the base type.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="existingInstance">An existing object to write into.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
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

        /// <summary>
        /// Reads a single managed object from the current stream as an instance of the specified type.  If you specify
        /// a base class of the actual object type, this method reads data only from the base type.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="typeReader">The <see cref="ContentTypeReader"/> to use to read the object.</param>
        /// <param name="existingInstance">An existing object to write into.</param>
        /// <returns>The object that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public T ReadRawObject<T>(ContentTypeReader typeReader, T existingInstance)
        {
            return (T)typeReader.Read(this, existingInstance);
        }

        /// <summary>
        /// Reads a shared resource ID and records it for subsequent fix-up.
        /// </summary>
        /// <typeparam name="T">The type of the shared resource.</typeparam>
        /// <param name="fixup">The fix-up action to perform.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="FormatException">The stream is corrupted.</exception>
        /// <exception cref="ContentLoadException">
        /// The type of the shared resource read does not match the specified <typeparamref name="T"/> type.
        /// </exception>
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
        /// Reads a <see cref="Vector2"/> from the underlying stream.
        /// </summary>
        /// <returns>The <see cref="Vector2"/> that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public Vector2 ReadVector2()
        {
            Vector2 result = new Vector2();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            return result;
        }

        /// <summary>
        /// Reads a <see cref="Vector3"/> from the underlying stream.
        /// </summary>
        /// <returns>The <see cref="Vector3"/> that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public Vector3 ReadVector3()
        {
            Vector3 result = new Vector3();
            result.X = ReadSingle();
            result.Y = ReadSingle();
            result.Z = ReadSingle();
            return result;
        }

        /// <summary>
        /// Reads a <see cref="Vector4"/> from the underlying stream.
        /// </summary>
        /// <returns>The <see cref="Vector4"/> that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
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
        /// Reads a <see cref="Color"/> from the underlying stream.
        /// </summary>
        /// <returns>The <see cref="Color"/> that was read.</returns>
        /// <exception cref="EndOfStreamException">The end of stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
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
