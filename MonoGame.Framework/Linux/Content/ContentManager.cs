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

namespace Microsoft.Xna.Framework.Content
{
    public class ContentManager : IDisposable
    {
        private string _rootDirectory;
        private IServiceProvider serviceProvider;
		private IGraphicsDeviceService graphicsDeviceService;

        public ContentManager(IServiceProvider serviceProvider)
        {
			if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
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
        }

        public void Dispose()
        {
        }
		
        public T Load<T>(string assetName)
        {			
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
			
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentException("assetname");
			}	
			
			if (!string.IsNullOrEmpty(_rootDirectory)) 
			{
				assetName = _rootDirectory+Path.DirectorySeparatorChar+assetName;
			}
			
			// avoid windows programmers to have to change path separator on every content load.
			assetName = assetName.Replace('\\',Path.DirectorySeparatorChar);
			
			// Get the real file name
			if ((typeof(T) == typeof(Texture2D))) 
			{				
				assetName = Texture2DReader.Normalize(assetName);
			}			
			if ((typeof(T) == typeof(SpriteFont))) 
			{
				assetName = SpriteFontReader.Normalize(assetName);
			}
			if ((typeof(T) == typeof(Song))) 
			{
				assetName = SongReader.Normalize(assetName);
			}
			if ((typeof(T) == typeof(SoundEffect))) 
			{
				assetName = SoundEffectReader.Normalize(assetName);
			}
			if ((typeof(T) == typeof(Video))) 
			{
				assetName = Video.Normalize(assetName);
			}
			if ((typeof(T) == typeof(Effect))) 
			{
				assetName = Effect.Normalize(assetName);
			}
			
			if (string.IsNullOrEmpty(assetName))
			{	
				throw new ContentLoadException("Could not load "  + originalAssetName + " asset!");
			}
			
			if(!Path.HasExtension(assetName))
				assetName = string.Format("{0}.xnb", assetName);
			
			if(Path.GetExtension(assetName).ToUpper() == ".XNB")
			{
				// Load a XNB file
				FileStream stream = new FileStream(assetName, FileMode.Open, FileAccess.Read, FileShare.Read);
		
				ContentReader reader = new ContentReader(this,stream,this.graphicsDeviceService.GraphicsDevice);
				ContentTypeReaderManager typeManager = new ContentTypeReaderManager(reader);
				reader.TypeReaders = typeManager.LoadAssetReaders(reader);
	            foreach (ContentTypeReader r in reader.TypeReaders)
	            {
	                r.Initialize(typeManager);
	            }
	            // we need to read a byte here for things to work out, not sure why
	            reader.ReadByte();
				
				// Get the 1-based index of the typereader we should use to start decoding with
          		int index = reader.ReadByte();
				ContentTypeReader contentReader = reader.TypeReaders[index - 1];
           		result = reader.ReadObject<T>(contentReader);

				reader.Close();
				stream.Close();
			}
			else
			{
				if ((typeof(T) == typeof(Texture2D))) 
					result = Texture2D.FromFile(graphicsDeviceService.GraphicsDevice,assetName);
				if ((typeof(T) == typeof(SpriteFont)))
				{
					//result = new SpriteFont(Texture2D.FromFile(graphicsDeviceService.GraphicsDevice,assetName), null, null, null, 0, 0.0f, null, null);
					throw new NotImplementedException();
				}
				if ((typeof(T) == typeof(Song)))
					result = new Song (assetName);		
				if ((typeof(T) == typeof(SoundEffect)))
					result = new SoundEffect (assetName);		
				if ((typeof(T) == typeof(Video)))
					result = new Video (assetName);		
				if (typeof(T) == typeof(Effect))
					result = new Effect(graphicsDeviceService.GraphicsDevice, assetName);
			}
						
			if (result == null)
			{	
				throw new ContentLoadException("Could not load "  + originalAssetName + " asset!");
			}
			
			return (T) result;
        }
		
		
        public virtual void Unload()
        {
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

