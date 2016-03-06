// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Utilities;
using Microsoft.Xna.Framework.Graphics;

#if !WINRT
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endif

namespace Microsoft.Xna.Framework.Content
{
	public partial class ContentManager : IDisposable
	{
        const byte ContentCompressedLzx = 0x80;
        const byte ContentCompressedLz4 = 0x40;

		private string _rootDirectory = string.Empty;
		private IServiceProvider serviceProvider;
		private IGraphicsDeviceService graphicsDeviceService;
        private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private List<IDisposable> disposableAssets = new List<IDisposable>();
        private bool disposed;
        private byte[] scratchBuffer;

		private static object ContentManagerLock = new object();
        private static List<WeakReference> ContentManagers = new List<WeakReference>();

        private static readonly List<char> targetPlatformIdentifiers = new List<char>()
        {
            'w', // Windows (DirectX)
            'x', // Xbox360
            'm', // WindowsPhone
            'i', // iOS
            'a', // Android
            'd', // DesktopGL
            'X', // MacOSX
            'W', // WindowsStoreApp
            'n', // NativeClient
            'M', // WindowsPhone8
            'r', // RaspberryPi
            'P', // PlayStation4

            // NOTE: There are additional idenfiers for consoles that 
            // are not defined in this repository.  Be sure to ask the
            // console port maintainers to ensure no collisions occur.

            
            // Legacy identifiers... these could be reused in the
            // future if we feel enough time has passed.

            'p', // PlayStationMobile
            'g', // Windows (OpenGL)
            'l', // Linux
            'u', // Ouya
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

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~ContentManager()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}

		public ContentManager(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			this.serviceProvider = serviceProvider;
            AddContentManager(this);
		}

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

		public void Dispose()
		{
			Dispose(true);
			// Tell the garbage collector not to call the finalizer
			// since all the cleanup will already be done.
			GC.SuppressFinalize(this);
            // Once disposed, content manager wont be used again
            RemoveContentManager(this);
		}

		// If disposing is true, it was called explicitly and we should dispose managed objects.
		// If disposing is false, it was called by the finalizer and managed objects should not be disposed.
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
            // We store the asset by a /-seperating key rather than how the
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
		
		protected virtual Stream OpenStream(string assetName)
		{
			Stream stream;
			try
            {
                var assetPath = Path.Combine(RootDirectory, assetName) + ".xnb";

                // This is primarily for editor support. 
                // Setting the RootDirectory to an absolute path is useful in editor
                // situations, but TitleContainer can ONLY be passed relative paths.                
#if DESKTOPGL || MONOMAC || WINDOWS
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
#if !WINRT
			catch (DirectoryNotFoundException directoryNotFound)
			{
				throw new ContentLoadException("The directory was not found.", directoryNotFound);
			}
#endif
			catch (Exception exception)
			{
				throw new ContentLoadException("Opening stream error.", exception);
			}
			return stream;
		}

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

			if (this.graphicsDeviceService == null)
			{
				this.graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
				if (this.graphicsDeviceService == null)
				{
					throw new InvalidOperationException("No Graphics Device Service");
				}
			}
			
			Stream stream = null;
			try
            {
				//try load it traditionally
				stream = OpenStream(assetName);

                // Try to load as XNB file
                try
                {
                    using (BinaryReader xnbReader = new BinaryReader(stream))
                    {
                        using (ContentReader reader = GetContentReaderFromXnb(assetName, ref stream, xnbReader, recordDisposableObject))
                        {
                            result = reader.ReadAsset<T>();
                            if (result is GraphicsResource)
                                ((GraphicsResource)result).Name = originalAssetName;
                        }
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
            }
            catch (ContentLoadException ex)
            {
				//MonoGame try to load as a non-content file

                assetName = TitleContainer.GetFilename(Path.Combine(RootDirectory, assetName));

                assetName = Normalize<T>(assetName);
	
				if (string.IsNullOrEmpty(assetName))
				{
					throw new ContentLoadException("Could not load " + originalAssetName + " asset as a non-content file!", ex);
				}

                result = ReadRawAsset<T>(assetName, originalAssetName);

                // Because Raw Assets skip the ContentReader step, they need to have their
                // disopsables recorded here. Doing it outside of this catch will 
                // result in disposables being logged twice.
                if (result is IDisposable)
                {
                    if (recordDisposableObject != null)
                        recordDisposableObject(result as IDisposable);
                    else
                        disposableAssets.Add(result as IDisposable);
                }
			}
            
			if (result == null)
				throw new ContentLoadException("Could not load " + originalAssetName + " asset!");

			return (T)result;
		}

        protected virtual string Normalize<T>(string assetName)
        {
            if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
            {
                return Texture2DReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(SpriteFont)))
            {
                return SpriteFontReader.Normalize(assetName);
            }
#if !WINRT
            else if ((typeof(T) == typeof(Song)))
            {
                return SongReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(SoundEffect)))
            {
                return SoundEffectReader.Normalize(assetName);
            }
#endif
            else if ((typeof(T) == typeof(Effect)))
            {
                return EffectReader.Normalize(assetName);
            }
            return null;
        }

        protected virtual object ReadRawAsset<T>(string assetName, string originalAssetName)
        {
            if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
            {
                using (Stream assetStream = TitleContainer.OpenStream(assetName))
                {
                    Texture2D texture = Texture2D.FromStream(
                        graphicsDeviceService.GraphicsDevice, assetStream);
                    texture.Name = originalAssetName;
                    return texture;
                }
            }
            else if ((typeof(T) == typeof(SpriteFont)))
            {
                //result = new SpriteFont(Texture2D.FromFile(graphicsDeviceService.GraphicsDevice,assetName), null, null, null, 0, 0.0f, null, null);
                throw new NotImplementedException();
            }
#if !DIRECTX
            else if ((typeof(T) == typeof(Song)))
            {
                return new Song(assetName);
            }
            else if ((typeof(T) == typeof(SoundEffect)))
            {
                using (Stream s = TitleContainer.OpenStream(assetName))
                    return SoundEffect.FromStream(s);
            }
#endif
            else if ((typeof(T) == typeof(Effect)))
            {
                using (Stream assetStream = TitleContainer.OpenStream(assetName))
                {
                    var data = new byte[assetStream.Length];
                    assetStream.Read(data, 0, (int)assetStream.Length);
                    return new Effect(this.graphicsDeviceService.GraphicsDevice, data);
                }
            }
            return null;
        }

        private ContentReader GetContentReaderFromXnb(string originalAssetName, ref Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
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
                    //thanks to ShinAli (https://bitbucket.org/alisci01/xnbdecompressor)
                    // default window size for XNB encoded files is 64Kb (need 16 bits to represent it)
                    LzxDecoder dec = new LzxDecoder(16);
                    decompressedStream = new MemoryStream(decompressedSize);
                    int compressedSize = xnbLength - 14;
                    long startPos = stream.Position;
                    long pos = startPos;

                    while (pos - startPos < compressedSize)
                    {
                        // the compressed stream is seperated into blocks that will decompress
                        // into 32Kb or some other size if specified.
                        // normal, 32Kb output blocks will have a short indicating the size
                        // of the block before the block starts
                        // blocks that have a defined output will be preceded by a byte of value
                        // 0xFF (255), then a short indicating the output size and another
                        // for the block size
                        // all shorts for these cases are encoded in big endian order
                        int hi = stream.ReadByte();
                        int lo = stream.ReadByte();
                        int block_size = (hi << 8) | lo;
                        int frame_size = 0x8000; // frame size is 32Kb by default
                        // does this block define a frame size?
                        if (hi == 0xFF)
                        {
                            hi = lo;
                            lo = (byte)stream.ReadByte();
                            frame_size = (hi << 8) | lo;
                            hi = (byte)stream.ReadByte();
                            lo = (byte)stream.ReadByte();
                            block_size = (hi << 8) | lo;
                            pos += 5;
                        }
                        else
                            pos += 2;

                        // either says there is nothing to decode
                        if (block_size == 0 || frame_size == 0)
                            break;

                        dec.Decompress(stream, block_size, decompressedStream, frame_size);
                        pos += block_size;

                        // reset the position of the input just incase the bit buffer
                        // read in some unused bytes
                        stream.Seek(pos, SeekOrigin.Begin);
                    }

                    if (decompressedStream.Position != decompressedSize)
                    {
                        throw new ContentLoadException("Decompression of " + originalAssetName + " failed. ");
                    }

                    decompressedStream.Seek(0, SeekOrigin.Begin);
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

            var reader = new ContentReader(this, decompressedStream, this.graphicsDeviceService.GraphicsDevice,
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

        /// <summary>
        /// Virtual property to allow a derived ContentManager to have it's assets reloaded
        /// </summary>
        protected virtual Dictionary<string, object> LoadedAssets
        {
            get { return loadedAssets; }
        }

		protected virtual void ReloadGraphicsAssets()
        {
            foreach (var asset in LoadedAssets)
            {
                // This never executes as asset.Key is never null.  This just forces the 
                // linker to include the ReloadAsset function when AOT compiled.
                if (asset.Key == null)
                    ReloadAsset(asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()));

#if WINDOWS_STOREAPP || WINDOWS_UAP
                var methodInfo = typeof(ContentManager).GetType().GetTypeInfo().GetDeclaredMethod("ReloadAsset");
#else
                var methodInfo = typeof(ContentManager).GetMethod("ReloadAsset", BindingFlags.NonPublic | BindingFlags.Instance);
#endif
                var genericMethod = methodInfo.MakeGenericMethod(asset.Value.GetType());
                genericMethod.Invoke(this, new object[] { asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()) }); 
            }
        }

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

			if (this.graphicsDeviceService == null)
			{
				this.graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
				if (this.graphicsDeviceService == null)
				{
					throw new InvalidOperationException("No Graphics Device Service");
				}
			}
			
			Stream stream = null;
			try
			{
                //try load it traditionally
                stream = OpenStream(assetName);

                // Try to load as XNB file
                try
                {
                    using (BinaryReader xnbReader = new BinaryReader(stream))
                    {
                        using (ContentReader reader = GetContentReaderFromXnb(assetName, ref stream, xnbReader, null))
                        {
                            reader.InitializeTypeReaders();
                            reader.ReadObject<T>(currentAsset);
                            reader.ReadSharedResources();
                        }
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
			}
			catch (ContentLoadException)
			{
				// Try to reload as a non-xnb file.
                // Just textures supported for now.

                assetName = TitleContainer.GetFilename(Path.Combine(RootDirectory, assetName));

                assetName = Normalize<T>(assetName);

                ReloadRawAsset(currentAsset, assetName, originalAssetName);
            }
		}

        protected virtual void ReloadRawAsset<T>(T asset, string assetName, string originalAssetName)
        {
            if (asset is Texture2D)
            {
                using (Stream assetStream = TitleContainer.OpenStream(assetName))
                {
                    var textureAsset = asset as Texture2D;
                    textureAsset.Reload(assetStream);
                }
            }
        }

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
		
		public IServiceProvider ServiceProvider
		{
			get
			{
				return this.serviceProvider;
			}
		}

        internal byte[] GetScratchBuffer(int size)
        {            
            size = Math.Max(size, 1024 * 1024);
            if (scratchBuffer == null || scratchBuffer.Length < size)
                scratchBuffer = new byte[size];
            return scratchBuffer;
        }
	}
}
