#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Lz4;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Path = System.IO.Path;
using System.Diagnostics;

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
		
		private static object ContentManagerLock = new object();
        private static List<WeakReference> ContentManagers = new List<WeakReference>();

	static List<char> targetPlatformIdentifiers = new List<char>()
        {
            'w', // Windows (DirectX)
            'x', // Xbox360
            'm', // WindowsPhone
            'i', // iOS
            'a', // Android
            'l', // Linux
            'X', // MacOSX
            'W', // WindowsStoreApp
            'n', // NativeClient
            'u', // Ouya
            'p', // PlayStationMobile
            'M', // WindowsPhone8
            'r', // RaspberryPi
            'P', // PlayStation4
            'g', // Windows (OpenGL)
        };

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
                    if (Object.ReferenceEquals(contentRef.Target, contentManager))
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
                    if (!contentRef.IsAlive || Object.ReferenceEquals(contentRef.Target, contentManager))
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
                string assetPath = Path.Combine(RootDirectory, assetName) + ".xnb";
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

            ContentReader reader;
            if (compressedLzx || compressedLz4)
            {
                // Decompress the xnb
                int decompressedSize = xnbReader.ReadInt32();
                MemoryStream decompressedStream = null;

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
                    // Decompress to a byte[] because Windows 8 doesn't support MemoryStream.GetBuffer()
                    var buffer = new byte[decompressedSize];
                    using (var decoderStream = new Lz4DecoderStream(stream))
                    {
                        if (decoderStream.Read(buffer, 0, buffer.Length) != decompressedSize)
                        {
                            throw new ContentLoadException("Decompression of " + originalAssetName + " failed. ");
                        }
                    }
                    // Creating the MemoryStream with a byte[] shares the buffer so it doesn't allocate any more memory
                    decompressedStream = new MemoryStream(buffer);
                }

                reader = new ContentReader(this, decompressedStream, this.graphicsDeviceService.GraphicsDevice,
                                                            originalAssetName, version, recordDisposableObject);
            }
            else
            {
                reader = new ContentReader(this, stream, this.graphicsDeviceService.GraphicsDevice,
                                                            originalAssetName, version, recordDisposableObject);
            }
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

#if WINDOWS_STOREAPP
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
	}
}
