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

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Path = System.IO.Path;

namespace Microsoft.Xna.Framework.Content
{
    public partial class ContentManager : IDisposable
    {
        private string _rootDirectory = string.Empty;
        private IServiceProvider serviceProvider;
        private IGraphicsDeviceService graphicsDeviceService;
        protected Dictionary<string, object> loadedAssets = new Dictionary<string, object>();
        bool disposed;		                        

        private static object ContentManagerLock = new object();
        private static List<ContentManager> ContentManagers = new List<ContentManager>();

        private static void AddContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                ContentManagers.Add(contentManager);
            }
        }

        private static void RemoveContentManager(ContentManager contentManager)
        {
            lock (ContentManagerLock)
            {
                if(ContentManagers.Contains(contentManager))
                    ContentManagers.Remove(contentManager);
            }
        }

        internal static void ReloadAllContent()
        {
            lock (ContentManagerLock)
            {
                foreach (var contentManager in ContentManagers)
                {
                    contentManager.ReloadContent();
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
        }

        // If disposing is true, it was called explicitly.
        // If disposing is false, it was called by the finalizer.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                Unload();
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

            // Check for a previously loaded asset first
            object asset = null;
            if (loadedAssets.TryGetValue(assetName, out asset))
            {
                if (asset is T)
                {
                    return (T)asset;
                }
            }

            // Load the asset.
            var result = ReadAsset<T>(assetName, null);

            // Cache the result.
            if (!loadedAssets.ContainsKey(assetName))
            {

                loadedAssets.Add(assetName, result);
            }

            return result;
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

            // Replace Windows path separators with local path separators
            assetName = GetFilename(assetName);

            // Get the real file name
            if ((typeof(T) == typeof(Curve))) 
            {				
                assetName = CurveReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(Texture2D)))
            {
                assetName = Texture2DReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(SpriteFont)))
            {
                assetName = SpriteFontReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(Effect)))
            {
                assetName = Effect.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(Song)))
            {
                assetName = SongReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(SoundEffect)))
            {
                assetName = SoundEffectReader.Normalize(assetName);
            }
            else if ((typeof(T) == typeof(Video)))
            {
                assetName = Video.Normalize(assetName);
            }

            if (string.IsNullOrEmpty(assetName))
            {
                throw new ContentLoadException("Could not load " + originalAssetName + " asset!");
            }

            if (!Path.HasExtension(assetName))
                assetName = string.Format("{0}.xnb", assetName);

            if (Path.GetExtension(assetName).ToLower() == ".xnb")
            {
                // Load a XNB file
                Stream stream = OpenStream(assetName);
                try
                {
                    using (BinaryReader xnbReader = new BinaryReader(stream))
                    {
                        // The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
                        byte x = xnbReader.ReadByte();
                        byte n = xnbReader.ReadByte();
                        byte b = xnbReader.ReadByte();
                        byte platform = xnbReader.ReadByte();

                        if (x != 'X' || n != 'N' || b != 'B' ||
                            !(platform == 'w' || platform == 'x' || platform == 'm'))
                        {
                            throw new ContentLoadException("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
                        }

                        byte version = xnbReader.ReadByte();
                        byte flags = xnbReader.ReadByte();

                        bool compressed = (flags & 0x80) != 0;
                        if (version != 5 && version != 4)
                        {
                            throw new ContentLoadException("Invalid XNB version");
                        }

                        // The next int32 is the length of the XNB file
                        int xnbLength = xnbReader.ReadInt32();

                        ContentReader reader;
                        if (compressed)
                        {
							
							LzxDecoder dec = new LzxDecoder(16);  							
                            //decompress the xnb
                            //thanks to ShinAli (https://bitbucket.org/alisci01/xnbdecompressor)
                            int compressedSize = xnbLength - 14;
                            int decompressedSize = xnbReader.ReadInt32();
                            int newFileSize = decompressedSize + 10;

                            MemoryStream decompressedStream = new MemoryStream(decompressedSize);

                            int decodedBytes = 0;
                            int pos = 0;							

#if ANDROID
                            // Android native stream does not support the Position property. LzxDecoder.Decompress also uses
                            // Seek.  So we read the entirity of the stream into a memory stream and replace stream with the
                            // memory stream.
                            MemoryStream memStream = new MemoryStream();
                            stream.CopyTo(memStream);
                            memStream.Seek(0, SeekOrigin.Begin);
                            stream.Dispose();
                            stream = memStream;
                            pos = -14;
#endif

                            while (pos < compressedSize)
                            {
                                // let's seek to the correct position
                                // The stream should already be in the correct position, and seeking can be slow
                                stream.Seek(pos + 14, SeekOrigin.Begin);
                                int hi = stream.ReadByte();
                                int lo = stream.ReadByte();
                                int block_size = (hi << 8) | lo;
                                int frame_size = 0x8000;
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

                                if (block_size == 0 || frame_size == 0)
                                    break;

                                int lzxRet = dec.Decompress(stream, block_size, decompressedStream, frame_size);
                                pos += block_size;
                                decodedBytes += frame_size;
                            }

                            if (decompressedStream.Position != decompressedSize)
                            {
                                throw new ContentLoadException("Decompression of " + originalAssetName + "failed. " +
                                                               " Try decompressing with nativeDecompressXnb first.");
                            }

                            decompressedStream.Seek(0, SeekOrigin.Begin);
                            reader = new ContentReader(this, decompressedStream, this.graphicsDeviceService.GraphicsDevice, originalAssetName);
                        }
                        else
                        {
                            reader = new ContentReader(this, stream, this.graphicsDeviceService.GraphicsDevice, originalAssetName);
                        }

                        using (reader)
                        {
                            result = reader.ReadAsset<T>();
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
            else
            {
                if ((typeof(T) == typeof(Texture2D)))
                {
#if IPHONE
					Texture2D texture = Texture2D.FromFile(graphicsDeviceService.GraphicsDevice, assetName);
                    texture.Name = originalAssetName;
                    result = texture;
#else
                    using (Stream assetStream = OpenStream(assetName))
                    {
                        Texture2D texture = Texture2D.FromFile(graphicsDeviceService.GraphicsDevice, assetStream);
                        texture.Name = originalAssetName;
                        result = texture;
                    }
#endif
                }
                else if ((typeof(T) == typeof(SpriteFont)))
                {
                    //result = new SpriteFont(Texture2D.FromFile(graphicsDeviceService.GraphicsDevice,assetName), null, null, null, 0, 0.0f, null, null);
                    throw new NotImplementedException();
                }
                else if ((typeof(T) == typeof(Song)))
                {
                    result = new Song(assetName);
                }
                else if ((typeof(T) == typeof(SoundEffect)))
                {
                    result = new SoundEffect(assetName);
                }
                else if ((typeof(T) == typeof(Video)))
                {
                    result = new Video(assetName);
                }
                else if ((typeof(T) == typeof(Effect)))
                {
                    result = new Effect(graphicsDeviceService.GraphicsDevice, assetName);
                }
            }

            if (result == null)
            {
                throw new ContentLoadException("Could not load " + originalAssetName + " asset!");
            }

            if ( recordDisposableObject != null && result is IDisposable )
                recordDisposableObject(result as IDisposable);

            return (T)result;
        }

        protected void ReloadContent()
        {
            foreach (var asset in loadedAssets)
            {
                ReloadAsset(asset.Key, asset.Value);
            }
        }

        protected void ReloadAsset(string originalAssetName, object currentAsset)
        {
            if (string.IsNullOrEmpty(originalAssetName))
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

            // Replace Windows path separators with local path separators
            var assetName = GetFilename(originalAssetName);

            // Get the real file name
            if ((currentAsset is Curve))
            {
                assetName = CurveReader.Normalize(assetName);
            }
            else if ((currentAsset is Texture2D))
            {
                assetName = Texture2DReader.Normalize(assetName);
            }
            else if ((currentAsset is SpriteFont))
            {
                assetName = SpriteFontReader.Normalize(assetName);
            }
            else if ((currentAsset is Effect))
            {
                assetName = Effect.Normalize(assetName);
            }
            else if ((currentAsset is Song))
            {
                assetName = SongReader.Normalize(assetName);
            }
            else if ((currentAsset is SoundEffect))
            {
                assetName = SoundEffectReader.Normalize(assetName);
            }
            else if ((currentAsset is Video))
            {
                assetName = Video.Normalize(assetName);
            }

            if (string.IsNullOrEmpty(assetName))
            {
                throw new ContentLoadException("Could not load " + originalAssetName + " asset!");
            }

            if (!Path.HasExtension(assetName))
                assetName = string.Format("{0}.xnb", assetName);

            if (Path.GetExtension(assetName).ToLower() == ".xnb")
            {
            }
            else
            {
                if ((currentAsset is Texture2D))
                {
                    using (Stream assetStream = OpenStream(assetName))
                    {
                        var asset = currentAsset as Texture2D;
                        asset.Reload(assetStream);
                    }
                }
                else if ((currentAsset is SpriteFont))
                {
                }
                else if ((currentAsset is Song))
                {
                }
                else if ((currentAsset is SoundEffect))
                {
                }
                else if ((currentAsset is Video))
                {
                }
                else if ((currentAsset is Effect))
                {
                }
            }
        }

        public virtual void Unload()
        {
            // Look for disposable assets.
            foreach (var pair in loadedAssets)
            {
                var disposable = pair.Value as IDisposable;
                if (disposable != null )
                    disposable.Dispose();
            }

            RemoveContentManager(this);
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

        public IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }
    }
}

