// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
	public partial class MediaLibrary : IDisposable
	{
        public AlbumCollection Albums { get { return PlatformGetAlbums();  } }
        //public ArtistCollection Artists { get; private set; }
        //public GenreCollection Genres { get; private set; }
        public bool IsDisposed { get; private set; }
        public MediaSource MediaSource { get { return null; } }
		//public PlaylistCollection Playlists { get; private set; }
        public SongCollection Songs { get { return PlatformGetSongs(); } }

		public MediaLibrary()
		{
		}

        /// <summary>
        /// Load the contents of MediaLibrary. This blocking call might take up to a few minutes depending on the platform and the size of the user's music library.
        /// </summary>
        /// <param name="progressCallback">Callback that reports back the progress of the music library loading in percents (0-100).</param>
        public void Load(Action<int> progressCallback = null)
	    {
	        PlatformLoad(progressCallback);
	    }
		
		public MediaLibrary(MediaSource mediaSource)
		{
            throw new NotSupportedException("Initializing from MediaSource is not supported");
		}
		
		public void Dispose()
		{
		    PlatformDispose();
		    this.IsDisposed = true;
		}
	}
}

