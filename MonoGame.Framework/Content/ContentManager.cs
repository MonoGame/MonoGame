// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MonoGame.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// The ContentManager is a run-time component which loads managed objects from .xnb binary files produced by the
    /// design time MonoGame Content Builder.  It also manages the lifespan of the loaded objects, disposing the
    /// content manager will also dispose any assets which are themselves <see cref="IDisposable"/>.
    /// </summary>
	public partial class ContentManager : IDisposable
	{
        const byte ContentCompressedLzx = 0x80;
        const byte ContentCompressedLz4 = 0x40;

		private string _rootDirectory = string.Empty;
		private IServiceProvider serviceProvider;
        private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private List<IDisposable> disposableAssets = new List<IDisposable>();
        private bool disposed;

		private static object ContentManagerLock = new object();
        private static List<WeakReference> ContentManagers = new List<WeakReference>();

        internal static readonly ByteBufferPool ScratchBufferPool = new ByteBufferPool(1024 * 1024, Environment.ProcessorCount);

        private static readonly List<char> targetPlatformIdentifiers = new List<char>()
        {
            'w', // Windows (XNA & DirectX)
            'x', // Xbox360 (XNA)
            'i', // iOS
            'a', // Android
            'd', // DesktopGL
            'X', // MacOSX
            'n', // NativeClient
            'r', // RaspberryPi
            'P', // PlayStation4
            '5', // PlayStation5
            'O', // XboxOne
            'S', // Nintendo Switch
            'G', // Google Stadia
            'b', // WebAssembly and Bridge.NET

            // NOTE: There are additional identifiers for consoles that
            // are not defined in this repository.  Be sure to ask the
            // console port maintainers to ensure no collisions occur.


            // Legacy identifiers... these could be reused in the
            // future if we feel enough time has passed.

            'W', // WindowsStoreApp
            'M', // WindowsPhone8
            'm', // WindowsPhone7.0 (XNA)
            'p', // PlayStationMobile
            'v', // PSVita
            'g', // Windows (OpenGL)
            'l', // Linux
        };


        static partial void PlatformStaticInit();

        static ContentManager()
        {
            // Allow any per-platform static initialization to occur.
            PlatformStaticInit();
        }

        private static void AddContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                // Check if the list contains this content manager already. Also take
                // the opportunity to prune the list of any finalized content managers.
                bool contains = false;
                for (int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    var contentRef = ContentManagers[i];
                    if (ReferenceEquals(contentRef.Target, contentManager))
                        contains = true;
                    if (!contentRef.IsAlive)
                        ContentManagers.RemoveAt(i);
                }
                if (!contains)
                    ContentManagers.Add(new WeakReference(contentManager));
            }
        }

        private static void RemoveContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                // Check if the list contains this content manager and remove it. Also
                // take the opportunity to prune the list of any finalized content managers.
                for (int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    var contentRef = ContentManagers[i];
                    if (!contentRef.IsAlive || ReferenceEquals(contentRef.Target, contentManager))
                        ContentManagers.RemoveAt(i);
                }
            }
        }

        internal static void ReloadGraphicsContent()
        {
            lock (ContentManagerLock)
            {
                // Reload the graphic assets of each content manager. Also take the
                // opportunity to prune the list of any finalized content managers.
                for (int i = ContentManagers.Count - 1; i >= 0; --i)
                {
                    var contentRef = ContentManagers[i];
                    if (contentRef.IsAlive)
                    {
                        var contentManager = (ContentManager)contentRef.Target;
                        if (contentManager != null)
                            contentManager.ReloadGraphicsAssets();
                    }
                    else
                    {
                        ContentManagers.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary />
		~ContentManager()
		{
			Dispose(false);
		}

        /// <summary>
        /// Initializes a new instance of the ContentMangaer.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         By default, the ContentMangaer searches for content in the directory where the executable is located.
        ///     </para>
        ///     <para>
        ///         When creating a new ContentManager, if no instance of <see cref="Game"/> is otherwise required by
        ///         the application, it is often better to create a new class that implements the
        ///         <see cref="IServiceProvider"/> interface rather than creating an instance of <see cref="Game"/> just
        ///         to create a new instance of <see cref="GraphicsDeviceManager"/>.
        ///     </para>
        /// </remarks>
        /// <param name="serviceProvider">The service provider that the ContentManager should use to locate services.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceProvider"/> parameter is null.</exception>
		public ContentManager(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			this.serviceProvider = serviceProvider;
            AddContentManager(this);
		}

        /// <inheritdoc cref="ContentManager.ContentManager(IServiceProvider)"/>
        /// <param name="rootDirectory">The root directory the ContentManager will search for content in.</param>
        public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (rootDirectory == null)
			{
				throw new ArgumentNullException("rootDirectory");
			}
			this.RootDirectory = rootDirectory;
			this.serviceProvider = serviceProvider;
            AddContentManager(this);
		}

        /// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
            // Once disposed, content manager wont be used again
            RemoveContentManager(this);
		}

        /// <inheritdoc cref="Dispose()"/>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
                if (disposing)
                {
                    Unload();
                }

				disposed = true;
			}
		}

        /// <summary>
        /// Loads an asset that has been processed by the Content Pipeline.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method attempts to load the asset based on the <see cref="CultureInfo.CurrentCulture"/>
        ///         searching for the asset by name and appending it with with the culture name (e.g. "assetName.en-US")
        ///         or two letter ISO language name (e.g. "assetName.en"). If unsuccessful in finding the asset with
        ///         the culture information appended, it will fall back to loading the default asset.
        ///     </para>
        ///     <para>
        ///         Before a ContentManager can load an asset, you need to add the asset to your game project using
        ///         the steps described in
        ///         <see href="https://docs.monogame.net/articles/content_pipeline/index.html">Adding Content - MonoGame</see>.
        ///     </para>
        /// </remarks>
        /// <typeparam name="T">
        ///     <para>
        ///         The type of asset to load.
        ///     </para>
        ///     <para>
        ///         <see cref="Effect"/>, <see cref="Model"/>, <see cref="SoundEffect"/>,
        ///         <see cref="Song"/>, <see cref="SpriteFont"/>, <see cref="Texture"/>, <see cref="Texture2D"/>,
        ///         and <see cref="TextureCube"/> are all supported by default by the standard Content Pipeline
        ///         processor, but additional types may be loaded by extending the processor.
        ///     </para>
        /// </typeparam>
        /// <param name="assetName">
        /// The asset name, relative to the <see cref="RootDirectory">ContentManager.RootDirectory</see>, and not
        /// including the .xnb extension.
        /// </param>
        /// <returns>
        /// The loaded asset. Repeated calls to load the same asset will return the same object instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="assetName"/> parameter is null or an empty string.</exception>
        /// <exception cref="ObjectDisposedException">This was called after the ContentManger was disposed.</exception>
        /// <exception cref="ContentLoadException">
        /// The type of the <paramref name="assetName"/> in the file does not match the type of asset requested as
        /// specified by <typeparamref name="T"/>.
        ///
        /// -or-
        ///
        /// A content file matching the <paramref name="assetName"/> parameter could not be found.
        ///
        /// -or-
        ///
        /// The specified path in the <paramref name="assetName"/> parameter is invalid (for example, a
        /// directory in the path does not exist).
        ///
        /// -or-
        ///
        /// An error occurred while opening the content file.
        /// </exception>
        public virtual T LoadLocalized<T> (string assetName)
        {
            string [] cultureNames =
            {
                CultureInfo.CurrentCulture.Name,                        // eg. "en-US"
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName     // eg. "en"
            };

            // Look first for a specialized language-country version of the asset,
            // then if that fails, loop back around to see if we can find one that
            // specifies just the language without the country part.
            foreach (string cultureName in cultureNames) {
                string localizedAssetName = assetName + '.' + cultureName;

                try {
                    return Load<T> (localizedAssetName);
                } catch (ContentLoadException) { }
            }

            // If we didn't find any localized asset, fall back to the default name.
            return Load<T> (assetName);
        }


        /// <summary>
        /// Loads an asset that has been processed by the Content Pipeline.
        /// </summary>
        /// <remarks>
        /// Before a ContentManager can load an asset, you need to add the asset to your game project using
        /// the steps described in
        /// <see href="https://docs.monogame.net/articles/content_pipeline/index.html">Adding Content - MonoGame</see>.
        /// </remarks>
        /// <typeparam name="T">
        ///     <para>
        ///         The type of asset to load.
        ///     </para>
        ///     <para>
        ///         <see cref="Effect"/>, <see cref="Model"/>, <see cref="SoundEffect"/>,
        ///         <see cref="Song"/>, <see cref="SpriteFont"/>, <see cref="Texture"/>, <see cref="Texture2D"/>,
        ///         and <see cref="TextureCube"/> are all supported by default by the standard Content Pipeline
        ///         processor, but additional types may be loaded by extending the processor.
        ///     </para>
        /// </typeparam>
        /// <param name="assetName">
        /// The asset name, relative to the <see cref="RootDirectory">ContentManager.RootDirectory</see>, and not
        /// including the .xnb extension.
        /// </param>
        /// <returns>
        /// The loaded asset. Repeated calls to load the same asset will return the same object instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="assetName"/> parameter is null or an empty string.</exception>
        /// <exception cref="ObjectDisposedException">This was called after the ContentManger was disposed.</exception>
        /// <exception cref="ContentLoadException">
        /// The type of the <paramref name="assetName"/> in the file does not match the type of asset requested as
        /// specified by <typeparamref name="T"/>.
        ///
        /// -or-
        ///
        /// A content file matching the <paramref name="assetName"/> parameter could not be found.
        ///
        /// -or-
        ///
        /// The specified path in the <paramref name="assetName"/> parameter is invalid (for example, a
        /// directory in the path does not exist).
        ///
        /// -or-
        ///
        /// An error occurred while opening the content file.
        /// </exception>
		public virtual T Load<T>(string assetName)
		{
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }

            T result = default(T);

            // On some platforms, name and slash direction matter.
            // We store the asset by a /-separating key rather than how the
            // path to the file was passed to us to avoid
            // loading "content/asset1.xnb" and "content\\ASSET1.xnb" as if they were two
            // different files. This matches stock XNA behavior.
            // The dictionary will ignore case differences
            var key = assetName.Replace('\\', '/');

            // Check for a previously loaded asset first
            object asset = null;
            if (loadedAssets.TryGetValue(key, out asset))
            {
                if (asset is T)
                {
                    return (T)asset;
                }
            }

            // Load the asset.
            result = ReadAsset<T>(assetName, null);

            loadedAssets[key] = result;
            return result;
		}

        /// <summary />
		protected virtual Stream OpenStream(string assetName)
		{
			Stream stream;
			try
            {
                var assetPath = Path.Combine(RootDirectory, assetName) + ".xnb";

                // This is primarily for editor support.
                // Setting the RootDirectory to an absolute path is useful in editor
                // situations, but TitleContainer can ONLY be passed relative paths.
#if DESKTOPGL || WINDOWS
                if (Path.IsPathRooted(assetPath))
                    stream = File.OpenRead(assetPath);
                else
#endif
                stream = TitleContainer.OpenStream(assetPath);
#if ANDROID
                // Read the asset into memory in one go. This results in a ~50% reduction
                // in load times on Android due to slow Android asset streams.
                MemoryStream memStream = new MemoryStream();
                stream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                stream.Close();
                stream = memStream;
#endif
			}
			catch (FileNotFoundException fileNotFound)
			{
				throw new ContentLoadException("The content file was not found.", fileNotFound);
			}

			catch (DirectoryNotFoundException directoryNotFound)
			{
				throw new ContentLoadException("The directory was not found.", directoryNotFound);
			}
			catch (Exception exception)
			{
				throw new ContentLoadException("Opening stream error.", exception);
			}
			return stream;
		}

        /// <summary />
		protected T ReadAsset<T>(string assetName, Action<IDisposable> recordDisposableObject)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("ContentManager");
			}

			string originalAssetName = assetName;
			object result = null;

            // Try to load as XNB file
            var stream = OpenStream(assetName);
            using (var xnbReader = new BinaryReader(stream))
            {
                using (var reader = GetContentReaderFromXnb(assetName, stream, xnbReader, recordDisposableObject))
                {
                    result = reader.ReadAsset<T>();
                    if (result is GraphicsResource)
                        ((GraphicsResource)result).Name = originalAssetName;
                }
            }

			if (result == null)
				throw new ContentLoadException("Could not load " + originalAssetName + " asset!");

			return (T)result;
		}

        private ContentReader GetContentReaderFromXnb(string originalAssetName, Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
        {
            // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
            byte x = xnbReader.ReadByte();
            byte n = xnbReader.ReadByte();
            byte b = xnbReader.ReadByte();
            byte platform = xnbReader.ReadByte();

            if (x != 'X' || n != 'N' || b != 'B' ||
                !(targetPlatformIdentifiers.Contains((char)platform)))
            {
                throw new ContentLoadException("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
            }

            byte version = xnbReader.ReadByte();
            byte flags = xnbReader.ReadByte();

            bool compressedLzx = (flags & ContentCompressedLzx) != 0;
            bool compressedLz4 = (flags & ContentCompressedLz4) != 0;
            if (version != 5 && version != 4)
            {
                throw new ContentLoadException("Invalid XNB version");
            }

            // The next int32 is the length of the XNB file
            int xnbLength = xnbReader.ReadInt32();

            Stream decompressedStream = null;
            if (compressedLzx || compressedLz4)
            {
                // Decompress the xnb
                int decompressedSize = xnbReader.ReadInt32();

                if (compressedLzx)
                {
                    int compressedSize = xnbLength - 14;
                    decompressedStream = new LzxDecoderStream(stream, decompressedSize, compressedSize);
                }
                else if (compressedLz4)
                {
                    decompressedStream = new Lz4DecoderStream(stream);
                }
            }
            else
            {
                decompressedStream = stream;
            }

            var reader = new ContentReader(this, decompressedStream,
                                                        originalAssetName, version, recordDisposableObject);

            return reader;
        }

        internal void RecordDisposable(IDisposable disposable)
        {
            Debug.Assert(disposable != null, "The disposable is null!");

            // Avoid recording disposable objects twice. ReloadAsset will try to record the disposables again.
            // We don't know which asset recorded which disposable so just guard against storing multiple of the same instance.
            if (!disposableAssets.Contains(disposable))
                disposableAssets.Add(disposable);
        }

        /// <summary />
        protected virtual Dictionary<string, object> LoadedAssets
        {
            get { return loadedAssets; }
        }

        /// <summary />
		protected virtual void ReloadGraphicsAssets()
        {
            foreach (var asset in LoadedAssets)
            {
                // This never executes as asset.Key is never null.  This just forces the
                // linker to include the ReloadAsset function when AOT compiled.
                if (asset.Key == null)
                    ReloadAsset(asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()));

                var methodInfo = ReflectionHelpers.GetMethodInfo(typeof(ContentManager), "ReloadAsset");
                var genericMethod = methodInfo.MakeGenericMethod(asset.Value.GetType());
                genericMethod.Invoke(this, new object[] { asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()) });
            }
        }

        /// <summary />
        protected virtual void ReloadAsset<T>(string originalAssetName, T currentAsset)
        {
			string assetName = originalAssetName;
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("ContentManager");
			}

            var stream = OpenStream(assetName);
            using (var xnbReader = new BinaryReader(stream))
            {
                using (var reader = GetContentReaderFromXnb(assetName, stream, xnbReader, null))
                {
                    reader.ReadAsset<T>(currentAsset);
                }
            }
		}

        /// <summary>
        /// Unloads all assets that were loaded by this ContentManger.
        /// </summary>
        /// <remarks>
        /// If an asset being unloaded implements the <see cref="IDisposable"/> interface, then the
        /// <see cref="IDisposable.Dispose">IDisposable.Dispose</see> method will be called before unloading.
        /// </remarks>
		public virtual void Unload()
		{
		    // Look for disposable assets.
		    foreach (var disposable in disposableAssets)
		    {
		        if (disposable != null)
		            disposable.Dispose();
		    }
			disposableAssets.Clear();
		    loadedAssets.Clear();
        }

        /// <summary>
        /// Unloads a single asset that was loaded by this ContentManager.
        /// </summary>
        /// <remarks>
        /// If the asset being unloaded implements the <see cref="IDisposable"/> interface, then the
        /// <see cref="IDisposable.Dispose">IDisposable.Dispose </see > method will be called before unloading.
        /// </remarks>
        /// <param name="assetName">
        /// The asset name, relative to the <see cref="RootDirectory">ContentManager.RootDirectory</see>, and not
        /// including the .xnb extension.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="assetName"/> parameter is null or an empty string.</exception>
        /// <exception cref="ObjectDisposedException">This was called after the ContentManger was disposed.</exception>
        public virtual void UnloadAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }

            //Check if the asset exists
            object asset;
            if (loadedAssets.TryGetValue(assetName, out asset))
            {
                //Check if it's disposable and remove it from the disposable list if so
                var disposable = asset as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                    disposableAssets.Remove(disposable);
                }

                loadedAssets.Remove(assetName);
            }
        }

        /// <summary>
        /// Unloads a set of assets loaded by this ContentManager where each element in the provided collection
        /// represents the name of an asset to unload.
        /// </summary>
        /// <remarks>
        /// If the asset being unloaded implements the <see cref="IDisposable"/> interface, then the
        /// <see cref="IDisposable.Dispose">IDisposable.Dispose </see > method will be called before unloading.
        /// </remarks>
        /// <param name="assetNames">The collection containing the names of assets to unload.</param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="assetNames"/> parameter is null.
        ///
        /// -or-
        ///
        /// If an element in the collection null or an empty string.
        /// </exception>
        /// <exception cref="ObjectDisposedException">This was called after the ContentManger was disposed.</exception>
        public virtual void UnloadAssets(IList<string> assetNames)
        {
            if (assetNames == null)
            {
                throw new ArgumentNullException("assetNames");
            }
            if (disposed)
            {
                throw new ObjectDisposedException("ContentManager");
            }

            for (int i = 0; i < assetNames.Count; i++)
            {
                UnloadAsset(assetNames[i]);
            }
        }

        /// <summary>
        /// Gets or Sets the root directory that this ContentManager will search for assets in.
        /// </summary>
		public string RootDirectory
		{
			get
			{
				return _rootDirectory;
			}
			set
			{
				_rootDirectory = value;
			}
		}

        internal string RootDirectoryFullPath
        {
            get
            {
                return Path.Combine(TitleContainer.Location, RootDirectory);
            }
        }

        /// <summary>
        /// Gets the service provider instance used by this ContentManager.
        /// </summary>
		public IServiceProvider ServiceProvider
		{
			get
			{
				return this.serviceProvider;
			}
		}
    }
}
