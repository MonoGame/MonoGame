// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private string _name;
		private int _playCount = 0;
        private TimeSpan _duration = TimeSpan.Zero;
        bool disposed;
        /// <summary>
        /// Gets the Album on which the Song appears.
        /// </summary>
        public Album Album
        {
            get { return PlatformGetAlbum(); }
#if WINDOWS_STOREAPP || WINDOWS_UAP
            internal set { PlatformSetAlbum(value); }
#endif
        }

        /// <summary>
        /// Gets the Artist of the Song.
        /// </summary>
        public Artist Artist
        {
            get { return PlatformGetArtist(); }
        }

        /// <summary>
        /// Gets the Genre of the Song.
        /// </summary>
        public Genre Genre
        {
            get { return PlatformGetGenre(); }
        }
        
        public bool IsDisposed
        {
            get { return disposed; }
        }

#if ANDROID || OPENAL || WEB || IOS
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
			_name = fileName;

            PlatformInitialize(fileName);
        }

        ~Song()
        {
            Dispose(false);
        }

        internal string FilePath
		{
			get { return _name; }
		}

        public static Song FromUri(string name, Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                var song = new Song(uri.OriginalString);
                song._name = name;
                return song;
            }
            else
            {
                throw new NotImplementedException("Loading songs from an absolute path is not implemented");
            }
        }
		
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

        public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

        public bool Equals(Song song)
        {
#if DIRECTX
            return song != null && song.FilePath == FilePath;
#else
			return ((object)song != null) && (Name == song.Name);
#endif
		}
		
		
		public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}
		
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}
		
		public static bool operator !=(Song song1, Song song2)
		{
		  return ! (song1 == song2);
		}

        public TimeSpan Duration
        {
            get { return PlatformGetDuration(); }
        }	

        public bool IsProtected
        {
            get { return PlatformIsProtected(); }
        }

        public bool IsRated
        {
            get { return PlatformIsRated(); }
        }

        public string Name
        {
            get { return PlatformGetName(); }
        }

        public int PlayCount
        {
            get { return PlatformGetPlayCount(); }
        }

        public int Rating
        {
            get { return PlatformGetRating(); }
        }

        public int TrackNumber
        {
            get { return PlatformGetTrackNumber(); }
        }
    }
}

