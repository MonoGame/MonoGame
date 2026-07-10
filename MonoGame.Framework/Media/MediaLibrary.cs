// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides access to songs, playlists, and pictures in the device's media library.
    /// </summary>
    /// <remarks>
    /// <see cref="MediaLibrary"/> provides the following properties that return media collections:
    /// <see cref="Albums"/>, <see cref="Songs"/>.
    /// Each property returns a collection object that can be enumerated and indexed.
    /// The collection object represents all media of that type in the device's media library.
    /// </remarks>
	public partial class MediaLibrary : IDisposable
	{
        /// <summary>
        /// Gets the <see cref="AlbumCollection"/> that contains all albums in the media library.
        /// </summary>
        public AlbumCollection Albums { get { return PlatformGetAlbums();  } }
        //public ArtistCollection Artists { get; private set; }
        //public GenreCollection Genres { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Gets the <see cref="MediaSource"/> with which this media library was constructed.
        /// </summary>
        public MediaSource MediaSource { get { return null; } }
        //public PlaylistCollection Playlists { get; private set; }
        /// <summary>
        /// Gets the <see cref="SongCollection"/> that contains all songs in the media library.
        /// </summary>
        public SongCollection Songs { get { return PlatformGetSongs(); } }

        /// <summary>
        /// Creates a new instance of <see cref="MediaLibrary"/>.
        /// </summary>
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

        /// <summary>
        /// Creates a new instance of <see cref="MediaLibrary"/> from a supplie <see cref="MediaSource"/>.
        /// </summary>
        /// <remarks>
        /// Current implenetation will always throw <see cref="NotSupportedException"/>
        /// </remarks>
		public MediaLibrary(MediaSource mediaSource)
		{
            throw new NotSupportedException("Initializing from MediaSource is not supported");
		}

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
		public void Dispose()
		{
		    PlatformDispose();
		    this.IsDisposed = true;
		}
	}
}

