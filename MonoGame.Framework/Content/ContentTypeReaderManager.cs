// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Defines a manager that constructs and keeps track of <see cref="ContentTypeReader"/> objects.
    /// </summary>
    public sealed class ContentTypeReaderManager
    {
        private static readonly object _locker;

        private static readonly Dictionary<Type, ContentTypeReader> _contentReadersCache;

        private Dictionary<Type, ContentTypeReader> _contentReaders;

        private static readonly string _assemblyName;

        private static readonly bool _isRunningOnNetCore = typeof(object).Assembly.GetName().Name == "System.Private.CoreLib";

        static ContentTypeReaderManager()
        {
            _locker = new object();
            _contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
            _assemblyName = ReflectionHelpers.GetAssembly(typeof(ContentTypeReaderManager)).FullName;

            // cache the most common type readers to avoid the most common AOT/trimming related issues
            // (especially on MonoGame standard content types)
            AddTypeCreator<AlphaTestEffectReader>(() => new AlphaTestEffectReader());
            AddTypeCreator<ArrayReader<int>>(() => new ArrayReader<int>());
            AddTypeCreator<ArrayReader<float>>(() => new ArrayReader<float>());
            AddTypeCreator<ArrayReader<char>>(() => new ArrayReader<char>());
            AddTypeCreator<ArrayReader<string>>(() => new ArrayReader<string>());
            AddTypeCreator<ArrayReader<Point>>(() => new ArrayReader<Point>());
            AddTypeCreator<ArrayReader<Vector2>>(() => new ArrayReader<Vector2>());
            AddTypeCreator<ArrayReader<Vector3>>(() => new ArrayReader<Vector3>());
            AddTypeCreator<ArrayReader<Matrix>>(() => new ArrayReader<Matrix>());
            AddTypeCreator<ArrayReader<Rectangle>>(() => new ArrayReader<Rectangle>());
            AddTypeCreator<ArrayReader<Color>>(() => new ArrayReader<Color>());
            AddTypeCreator<ArrayReader<StringReader>>(() => new ArrayReader<StringReader>());
            AddTypeCreator<BasicEffectReader>(() => new BasicEffectReader());
            AddTypeCreator<BooleanReader>(() => new BooleanReader());
            AddTypeCreator<BoundingBoxReader>(() => new BoundingBoxReader());
            AddTypeCreator<BoundingFrustumReader>(() => new BoundingFrustumReader());
            AddTypeCreator<BoundingSphereReader>(() => new BoundingSphereReader());
            AddTypeCreator<ByteReader>(() => new ByteReader());
            AddTypeCreator<CharReader>(() => new CharReader());
            AddTypeCreator<ColorReader>(() => new ColorReader());
            AddTypeCreator<CurveReader>(() => new CurveReader());
            AddTypeCreator<DateTimeReader>(() => new DateTimeReader());
            AddTypeCreator<DecimalReader>(() => new DecimalReader());
            // DictionaryReader<TKey, TValue>
            AddTypeCreator<DoubleReader>(() => new DoubleReader());
            AddTypeCreator<DualTextureEffectReader>(() => new DualTextureEffectReader());
            AddTypeCreator<EffectMaterialReader>(() => new EffectMaterialReader());
            AddTypeCreator<EffectReader>(() => new EffectReader());
            AddTypeCreator<EnumReader<Graphics.SpriteEffects>>(() => new EnumReader<Graphics.SpriteEffects>());
            AddTypeCreator<EnumReader<Graphics.Blend>>(() => new EnumReader<Graphics.Blend>());
            AddTypeCreator<EnvironmentMapEffectReader>(() => new EnvironmentMapEffectReader());
            AddTypeCreator<ExternalReferenceReader>(() => new ExternalReferenceReader());
            AddTypeCreator<IndexBufferReader>(() => new IndexBufferReader());
            AddTypeCreator<Int16Reader>(() => new Int16Reader());
            AddTypeCreator<Int32Reader>(() => new Int32Reader());
            AddTypeCreator<Int64Reader>(() => new Int64Reader());
            AddTypeCreator<ListReader<int>>(() => new ListReader<int>());
            AddTypeCreator<ListReader<float>>(() => new ListReader<float>());
            AddTypeCreator<ListReader<char>>(() => new ListReader<char>());
            AddTypeCreator<ListReader<string>>(() => new ListReader<string>());
            AddTypeCreator<ListReader<Point>>(() => new ListReader<Point>());
            AddTypeCreator<ListReader<Vector2>>(() => new ListReader<Vector2>());
            AddTypeCreator<ListReader<Vector3>>(() => new ListReader<Vector3>());
            AddTypeCreator<ListReader<Matrix>>(() => new ListReader<Matrix>());
            AddTypeCreator<ListReader<Rectangle>>(() => new ListReader<Rectangle>());
            AddTypeCreator<ListReader<Color>>(() => new ListReader<Color>());
            AddTypeCreator<ListReader<StringReader>>(() => new ListReader<StringReader>());
            AddTypeCreator<MatrixReader>(() => new MatrixReader());
            AddTypeCreator<ModelReader>(() => new ModelReader());
            // MultiArrayReader<T>
            AddTypeCreator<NullableReader<Rectangle>>(() => new NullableReader<Rectangle>());
            AddTypeCreator<PlaneReader>(() => new PlaneReader());
            AddTypeCreator<PointReader>(() => new PointReader());
            AddTypeCreator<QuaternionReader>(() => new QuaternionReader());
            AddTypeCreator<RayReader>(() => new RayReader());
            AddTypeCreator<RectangleReader>(() => new RectangleReader());
            // ReflectiveReader<T>
            AddTypeCreator<SByteReader>(() => new SByteReader());
            AddTypeCreator<SingleReader>(() => new SingleReader());
            AddTypeCreator<SkinnedEffectReader>(() => new SkinnedEffectReader());
            AddTypeCreator<SongReader>(() => new SongReader());
            AddTypeCreator<SoundEffectReader>(() => new SoundEffectReader());
            AddTypeCreator<SpriteFontReader>(() => new SpriteFontReader());
            AddTypeCreator<StringReader>(() => new StringReader());
            AddTypeCreator<Texture2DReader>(() => new Texture2DReader());
            AddTypeCreator<Texture3DReader>(() => new Texture3DReader());
            AddTypeCreator<TextureCubeReader>(() => new TextureCubeReader());
            AddTypeCreator<TextureReader>(() => new TextureReader());
            AddTypeCreator<TimeSpanReader>(() => new TimeSpanReader());
            AddTypeCreator<UInt16Reader>(() => new UInt16Reader());
            AddTypeCreator<UInt32Reader>(() => new UInt32Reader());
            AddTypeCreator<UInt64Reader>(() => new UInt64Reader());
            AddTypeCreator<Vector2Reader>(() => new Vector2Reader());
            AddTypeCreator<Vector3Reader>(() => new Vector3Reader());
            AddTypeCreator<Vector4Reader>(() => new Vector4Reader());
            AddTypeCreator<VertexBufferReader>(() => new VertexBufferReader());
            AddTypeCreator<VertexDeclarationReader>(() => new VertexDeclarationReader());
            AddTypeCreator<VideoReader>(() =>  new VideoReader());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ContentReader"/> class initialized for the specified type.
        /// </summary>
        /// <param name="targetType">The type the <see cref="ContentReader"/> will handle.</param>
        /// <returns>
        /// The <see cref="ContentReader"/> created by this method if a content reader of the specified type has been
        /// registered with this content manager; otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="targetType"/> parameter is null.</exception>
        public ContentTypeReader GetTypeReader(Type targetType)
        {
            if (targetType.IsArray && targetType.GetArrayRank() > 1)
                targetType = typeof(Array);

            ContentTypeReader reader;
            if (_contentReaders.TryGetValue(targetType, out reader))
                return reader;

            return null;
        }

        internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
        {
            // The first content byte i read tells me the number of content readers in this XNB file
            var numberOfReaders = reader.Read7BitEncodedInt();
            var contentReaders = new ContentTypeReader[numberOfReaders];
            var needsInitialize = new BitArray(numberOfReaders);
            _contentReaders = new Dictionary<Type, ContentTypeReader>(numberOfReaders);

            // Lock until we're done allocating and initializing any new
            // content type readers...  this ensures we can load content
            // from multiple threads and still cache the readers.
            lock (_locker)
            {
                // For each reader in the file, we read out the length of the string which contains the type of the reader,
                // then we read out the string. Finally we instantiate an instance of that reader using reflection
                for (var i = 0; i < numberOfReaders; i++)
                {
                    // This string tells us what reader we need to decode the following data
                    // string readerTypeString = reader.ReadString();
                    string originalReaderTypeString = reader.ReadString();

                    // in AOT scenarios, unused readers cause an empty string to be returned.
                    // This is fine, as it means that the reader is not needed for this content file.
                    if (string.IsNullOrEmpty(originalReaderTypeString))
                    {
                        contentReaders[i] = null;
                        needsInitialize[i] = false;
                        continue;
                    }

                    Func<ContentTypeReader> readerFunc;
#pragma warning disable IL2057
                    Type l_readerType = Type.GetType(originalReaderTypeString);
#pragma warning restore IL2057
                    if (l_readerType != null && typeCreators.TryGetValue(l_readerType, out readerFunc))
                    {
                        contentReaders[i] = readerFunc();
                        needsInitialize[i] = true;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine(originalReaderTypeString);

                        // Need to resolve namespace differences
                        string readerTypeString = originalReaderTypeString;

                        readerTypeString = PrepareType(readerTypeString);

                        try
                        {
                            // this might fail in AOT context and we need to properly warn the user on what to do if it happens
#pragma warning disable IL2057
                            l_readerType = Type.GetType(readerTypeString);
#pragma warning restore IL2057
                        }
                        catch (NotSupportedException e)
                        {
                            throw new NotSupportedException("It seems that you are using PublishAot and trying to load assets with a reflection-based serializer (which is not natively supported). To work around this error, call ContentTypeReaderManager.AddTypeCreator() in your Game constructor with the type mentionned in the following message: " + e.Message);
                        }

                        if (l_readerType != null)
                        {
                            ContentTypeReader typeReader;
                            if (!_contentReadersCache.TryGetValue(l_readerType, out typeReader))
                            {
                                try
                                {
                                    typeReader = l_readerType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
                                }
                                catch (TargetInvocationException ex)
                                {
                                    // If you are getting here, the Mono runtime is most likely not able to JIT the type.
                                    // In particular, MonoTouch needs help instantiating types that are only defined in strings in Xnb files.
                                    throw new InvalidOperationException(
                                        "Failed to get default constructor for ContentTypeReader. To work around, add a creation function to ContentTypeReaderManager.AddTypeCreator() " +
                                        "with the following failed type string: " + originalReaderTypeString, ex);
                                }

                                needsInitialize[i] = true;

                                _contentReadersCache.Add(l_readerType, typeReader);
                            }

                            contentReaders[i] = typeReader;
                        }
                        else
                            throw new ContentLoadException(
                                "Could not find ContentTypeReader Type. Please ensure the name of the Assembly that contains the Type matches the assembly in the full type name: " +
                                originalReaderTypeString + " (" + readerTypeString + "). " +
                                " If you are using trimming, PublishAOT, or targeting mobile platforms, you should call ContentTypeReaderManager.AddTypeCreator() on that reader type somewhere in your code.");
                    }

                    var targetType = contentReaders[i].TargetType;
                    if (targetType != null)
                        if (!_contentReaders.ContainsKey(targetType))
                            _contentReaders.Add(targetType, contentReaders[i]);

                    // I think the next 4 bytes refer to the "Version" of the type reader,
                    // although it always seems to be zero
                    reader.ReadInt32();
                }

                // Initialize any new readers.
                for (var c = 0; c < contentReaders.Length; c++)
                {
                    if (needsInitialize.Get(c))
                        contentReaders[c].Initialize(this);
                }

            } // lock (_locker)

            return contentReaders;
        } 

        /// <summary>
        /// Removes the Version, Culture, and PublicKeyToken from a fully-qualified type name string.
        /// </summary>
        /// <remarks>
        /// Supports multiple generic types (e.g. Dictionary&lt;TKey,TValue&gt;) and nested generic types (e.g. List&lt;List&lt;int&gt;&gt;).
        /// </remarks>
        /// <param name="type">A string containing the fully-qualified type name to prepare.</param>
        /// <returns>A new string with the Version, Culture and PublicKeyToken removed.</returns>
        public static string PrepareType(string type)
        {
            //Needed to support nested types
            int count = type.Split(new[] { "[[" }, StringSplitOptions.None).Length - 1;

            string preparedType = type;

            for (int i = 0; i < count; i++)
            {
                preparedType = Regex.Replace(preparedType, @"\[(.+?), Version=.+?\]", "[$1]");
            }

            //Handle non generic types
            if (preparedType.Contains("PublicKeyToken"))
                preparedType = Regex.Replace(preparedType, @"(.+?), Version=.+?$", "$1");

            preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Graphics", string.Format(", {0}", _assemblyName));
            preparedType = preparedType.Replace(", Microsoft.Xna.Framework.Video", string.Format(", {0}", _assemblyName));
            preparedType = preparedType.Replace(", Microsoft.Xna.Framework", string.Format(", {0}", _assemblyName));

            if (_isRunningOnNetCore)
                preparedType = preparedType.Replace("mscorlib", "System.Private.CoreLib");
            else
                preparedType = preparedType.Replace("System.Private.CoreLib", "mscorlib");

            return preparedType;
        }

        // Static map of type names to creation functions. Required as iOS requires all types at compile time
        private static Dictionary<Type, Func<ContentTypeReader>> typeCreators = new Dictionary<Type, Func<ContentTypeReader>>();

        /// <summary>
        /// Registers a function to create a <see cref="ContentTypeReader"/> instance used to read an object of the
        /// type specified.
        /// Call this method to register content readers that may fail loading when trimming or PublishAot are used.
        /// </summary>
        /// <typeparam name="T">The content reader type.</typeparam>
        /// <param name='createFunction'>The function responsible for creating an instance of the <see cref="ContentTypeReader"/> class.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="typeString"/> parameter is null or an empty string.</exception>
        public static void AddTypeCreator<T>(Func<ContentTypeReader> createFunction) where T : ContentTypeReader
        {
            var type = typeof(T);
            if (!typeCreators.ContainsKey(type))
                typeCreators.Add(type, createFunction);
        }

        /// <summary>
        /// Clears all content type creators that were registered with <see cref="AddTypeCreator(Type, Func{ContentTypeReader})"/>
        /// </summary>
        public static void ClearTypeCreators()
        {
            typeCreators.Clear();
        }

    }
}
