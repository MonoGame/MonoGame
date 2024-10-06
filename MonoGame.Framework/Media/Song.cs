// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides access to a song in the song library.
    /// </summary>
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private string _name;
        private string _filePath;
        private int _playCount = 0;
        private TimeSpan _duration = TimeSpan.Zero;
        bool disposed;
        /// <summary>
        /// Gets the <see cref="Album"/> on which the Song appears.
        /// </summary>
        public Album Album
        {
            get { return PlatformGetAlbum(); }
        }

        /// <summary>
        /// Gets the <see cref="Media.Artist"/> of the Song.
        /// </summary>
        public Artist Artist
        {
            get { return PlatformGetArtist(); }
        }

        /// <summary>
        /// Gets the <see cref="Media.Genre"/> of the Song.
        /// </summary>
        public Genre Genre
        {
            get { return PlatformGetGenre(); }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

#if ANDROID || OPENAL || WEB || IOS || NATIVE
        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
#if !DESKTOPGL
        event FinishedPlayingHandler DonePlaying;
#endif
#endif
        internal Song(string fileName, int durationMS)
            : this(fileName)
        {
            _duration = TimeSpan.FromMilliseconds(durationMS);
        }

		internal Song(string fileName)
		{
            _filePath = fileName;
            _name = Path.GetFileNameWithoutExtension(fileName);

            PlatformInitialize(fileName);
        }

        /// <summary/>
        ~Song()
        {
            Dispose(false);
        }

        internal string FilePath
		{
			get { return _filePath; }
		}

        /// <summary>
        /// Returns a song that can be played via <see cref="MediaPlayer"/>.
        /// </summary>
        /// <param name="name">The name for the song. See <see cref="Song.Name">Song.Name</see>.</param>
        /// <param name="uri">The path to the song file.</param>
        /// <returns></returns>
        public static Song FromUri(string name, Uri uri)
        {
            var song = new Song(uri.OriginalString);
            song._name = name;
            return song;
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    PlatformDispose(disposing);
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

        /// <summary>
        /// Determines whether two instances of <see cref="Song"/> are equal.
        /// </summary>
        /// <param name="song">The <see cref="Song"/> to compare with the current object</param>
        /// <returns><see langword="true"/> if the specified <see cref="Song"/> is equal to the current object;
        /// otherwise, <see langword="false"/></returns>
        public bool Equals(Song song)
        {
#if DIRECTX
            return song != null && song.FilePath == FilePath;
#else
			return ((object)song != null) && (Name == song.Name);
#endif
		}

        /// <inheritdoc/>
        public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}

        /// <summary>
        /// Determines whether the specified Song instances are equal.
        /// </summary>
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}

        /// <summary>
        /// Determines whether the specified Song instances are not equal.
        /// </summary>
        public static bool operator !=(Song song1, Song song2)
		{
		    return !(song1 == song2);
		}

        /// <summary>
        /// Gets the duration of the <see cref="Song"/>.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// Gets a value that indicates whether the song is DRM protected content.
        /// </summary>
        public bool IsProtected
        {
            get { return PlatformIsProtected(); }
        }

        /// <summary>
        /// Gets a value that indicates whether the song has been rated by the user.
        /// </summary>
        public bool IsRated
        {
            get { return PlatformIsRated(); }
        }

        /// <summary>
        /// Gets the name of the <see cref="Song"/>.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the song play count.
        /// </summary>
        public int PlayCount
        {
            get { return PlatformGetPlayCount(); }
        }

        /// <summary>
        /// Gets the user's rating for the <see cref="Song"/>.
        /// </summary>
        /// <value>
        /// User's rating for this <see cref="Song"/>, or 0 if the song is unrated.
        /// Ratings range from 1 (dislike the most) to 10 (like the most).
        /// </value>
        public int Rating
        {
            get { return PlatformGetRating(); }
        }

        /// <summary>
        /// Gets the track number of the song on the song's <see cref="Album"/>.
        /// </summary>
        /// <value>
        /// Track number of this <see cref="Song"/> on the song's <see cref="Album"/>.
        /// </value>
        public int TrackNumber
        {
            get { return PlatformGetTrackNumber(); }
        }
    }
}

